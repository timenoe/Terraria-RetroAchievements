using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using RASharpIntegration.Network;
using RetroAchievements.Commands;
using RetroAchievements.Players;
using RetroAchievements.Tools;

namespace RetroAchievements.Systems
{
    /// <summary>
    /// Used to pass login arguments through an event
    /// </summary>
    /// <param name="user">User name</param>
    /// <param name="password">User password</param>
    public class LoginEventArgs(string user, string password) : EventArgs
    {
        public string User { get; } = user;
        public string Password { get; } = password;
    }

    /// <summary>
    /// Used to pass achievement arguments through an event
    /// </summary>
    /// <param name="name">Name of the achievement</param>
    public class UnlockAchievementEventArgs(string name) : EventArgs
    {
        public string Name { get; } = name;
    }

    /// <summary>
    /// Used to communicate to the RA server
    /// </summary>
    public class NetworkSystem : ModSystem
    {
        /// <summary>
        /// How often an activity ping is sent to the server in minutes
        /// </summary>
        public const int PingInterval = 2;

        /// <summary>
        /// How often failed achievement unlock requests are retried in minutes
        /// </summary>
        public const int RetryInterval = 1;


        /// <summary>
        /// List of all achievements that failed to unlock
        /// </summary>
        private readonly Dictionary<string, int> _failedAchs = [];

        /// <summary>
        /// HTTP client to send requests with
        /// </summary>
        private readonly HttpClient _client = new();

        /// <summary>
        /// Timer to periodically send a game activity ping for the user
        /// </summary>
        private readonly Timer _pingTimer = new(TimeSpan.FromMinutes(PingInterval).TotalMilliseconds);

        /// <summary>
        /// Timer to periodically retry failed unlock requests for the user
        /// </summary>
        private readonly Timer _retryTimer = new(TimeSpan.FromMinutes(RetryInterval).TotalMilliseconds);

        /// <summary>
        /// True if the game has been mastered
        /// </summary>
        private bool _isMastered;

        /// <summary>
        /// True if a user is logged in
        /// </summary>
        private bool _isLogin;

        /// <summary>
        /// Path to the serialized file with cached credentials
        /// </summary>
        private string _cachePath;

        /// <summary>
        /// List of all unlocked hardcore achievements
        /// </summary>
        private List<int> _unlockedAchs = [];

        /// <summary>
        /// Header for API requests
        /// </summary>
        private RequestHeader _header;


        /// <summary>
        /// Event to start a game session for the user
        /// </summary>
        private event EventHandler StartSessionCommand;

        /// <summary>
        /// Event to send a game activity ping for the user
        /// </summary>
        private event EventHandler PingCommand;

        /// <summary>
        /// Event to retry failed achievement unlocks
        /// </summary>
        private event EventHandler RetryCommand;


        /// <summary>
        /// True if the game has been mastered
        /// </summary>
        public bool IsMastered => _isMastered;

        /// <summary>
        /// True if the game session has been started
        /// </summary>
        public bool IsLogin => _isLogin;

        /// <summary>
        /// List of all unlocked hardcore achievements
        /// </summary>
        public List<int> UnlockedAchs => _unlockedAchs;

        /// <summary>
        /// Active user
        /// </summary>
        public string User => _header.user;


        public override void OnModLoad()
        {
            if (Main.dedServ)
                return;

            _cachePath = $"{ModLoader.ModPath}/{Mod.Name}.nbt";
            _header = new(RetroAchievements.Host, RetroAchievements.GetGameId(), RetroAchievements.IsHardcore);

            // Subscribe to internal events
            _pingTimer.Elapsed += PingTimer_Elapsed;
            _retryTimer.Elapsed += RetryTimer_Elapsed;
            StartSessionCommand += NetworkSystem_StartSessionCommand;
            PingCommand += NetworkSystem_PingCommand;
            RetryCommand += NetworkSystem_RetryCommand;

            // Subscribe to external events
            ModContent.GetInstance<RaCommand>().LoginCommand += RaCommand_LoginCommand;
            ModContent.GetInstance<RaCommand>().LogoutCommand += RaCommand_LogoutCommand;
            ModContent.GetInstance<RuleSystem>().UnlockAchievementCommand += RuleSystem_UnlockAchievementCommand;

            // Ensure HTTP client has proper User Agent for RA requests
            SetupUserAgent();

            // If user credentials already in cache, start the game session
            if (TryGetCredentials())
                StartSessionCommand.Invoke(this, null);
        }

        public override void OnModUnload()
        {
            if (Main.dedServ)
                return;

            // Unsubscribe from internal events
            _pingTimer.Elapsed -= PingTimer_Elapsed;
            _retryTimer.Elapsed -= RetryTimer_Elapsed;
            StartSessionCommand -= NetworkSystem_StartSessionCommand;
            PingCommand -= NetworkSystem_PingCommand;
            RetryCommand -= NetworkSystem_RetryCommand;

            // Unsubscribe from external events
            ModContent.GetInstance<RaCommand>().LoginCommand -= RaCommand_LoginCommand;
            ModContent.GetInstance<RaCommand>().LogoutCommand -= RaCommand_LogoutCommand;
            ModContent.GetInstance<RuleSystem>().UnlockAchievementCommand -= RuleSystem_UnlockAchievementCommand;

            // Handle IDisposable objects
            _client.Dispose();
            _pingTimer.Dispose();
            _retryTimer.Dispose();
        }

        /// <summary>
        /// Cache the user credentials to a serialized file
        /// </summary>
        private void CacheCredentials()
        {
            TagCompound tag = new()
            {
                ["user"] = _header.user,
                ["token"] = _header.token
            };
            TagIO.ToFile(tag, _cachePath);

            MessageTool.ModLog($"Cached login credentials for {User}");
        }

        /// <summary>
        /// Setup the HTTP client's User Agent for making requests to RA
        /// </summary>
        private void SetupUserAgent()
        {
            // RA requires a user agent of {Standalone name}/{x.y.z Version}
            _client.DefaultRequestHeaders.Add("User-Agent", $"TerrariaRetroAchievements/{Mod.Version}");
        }
        
        /// <summary>
        /// Update mastery status<br/>
        /// Mastery occurs when all available achievements have been unlocked
        /// </summary>
        private void UpdateMastery(bool display = false)
        {
            _isMastered = UnlockedAchs.Count == RetroAchievements.GetAchievementCount();
            if (IsMastered && display)
            {
                if (RetroAchievements.IsHardcore)
                    MessageTool.Log($"{_header.user} has mastered {RetroAchievements.GetGameName()}!");
                else
                    MessageTool.Log($"{_header.user} has completed {RetroAchievements.GetGameName()}!");
            }
        }

        /// <summary>
        /// Try to get user credentials from a cache file
        /// </summary>
        /// <returns>True if credentials were retrieved from the cache file</returns>
        private bool TryGetCredentials()
        {
            if (!FileUtilities.Exists(_cachePath, false))
                return false;

            TagCompound tag = TagIO.FromFile(_cachePath);
            if (tag == null)
                return false;

            string user = tag.GetString("user");
            string token = tag.GetString("token");

            // Ensure credentials aren't empty
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(token))
                return false;

            _header.user = user;
            _header.token = token;
            MessageTool.ModLog($"Retrieved cached login credentials for {User}");
            return true;
        }

        /// <summary>
        /// Login a user
        /// </summary>
        /// <param name="user">User name</param>
        /// <param name="pass">User password</param>
        /// <returns>Asynchronous task</returns>
        private async Task Login(string user, string pass)
        {
            MessageTool.Log($"Logging in {user} to {_header.host}...");

            _header.user = user;
            ApiResponse<LoginResponse> api = await NetworkInterface.TryLogin(_client, _header, pass);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                MessageTool.Log($"Unable to login ({api.Failure})");
                return;
            }

            if (!api.Response.Success)
            {
                MessageTool.Log($"Unable to login ({api.Response.Error})");
                return;
            }
                
            _header.token = api.Response.Token;
            CacheCredentials();

            MessageTool.Log($"{user} has successfully logged in!");
            StartSessionCommand.Invoke(this, null);
        }

        /// <summary>
        /// LoginCommand event callback to login a user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void RaCommand_LoginCommand(object sender, LoginEventArgs args) => await Login(args.User, args.Password);

        /// <summary>
        /// LogoutCommand event callback to logout a user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void RaCommand_LogoutCommand(object sender, EventArgs e)
        {
            string oldUser = _header.user;
            _header.user = "";
            _header.token = "";

            // Remove cached credential file
            FileInfo cacheInfo = new(_cachePath);
            if (cacheInfo.Exists)
                cacheInfo.Delete();

            // Reset internal state
            _unlockedAchs = [];
            UpdateMastery();
            _pingTimer.Stop();
            _retryTimer.Stop();
            _isLogin = false;

            // Take the achievement buff from the player if in-game
            if (!Main.gameMenu)
                Main.LocalPlayer.GetModPlayer<RetroAchievementPlayer>().TakeAchievementBuff();

            MessageTool.Log($"{oldUser} has logged out");
        }

        /// <summary>
        /// Start a game session for the user
        /// </summary>
        /// <returns>Asynchronous task</returns>
        private async Task StartSession()
        {
            // Start game session for all associated sets
            _unlockedAchs = [];
            foreach (int id in RetroAchievements.GetSets())
            {
                MessageTool.ModLog($"Starting a game session for {id}...");
                ApiResponse<StartSessionResponse> api = await NetworkInterface.TryStartSession(_client, _header);

                if (!string.IsNullOrEmpty(api.Failure))
                {
                    MessageTool.ModLog($"Unable to start game session for {id} ({api.Failure})");
                    continue;
                }

                if (!api.Response.Success)
                {
                    MessageTool.ModLog($"Unable to start game session for {id} ({api.Response.Error})");
                    continue;
                }

                // Append existing achievement unlocks
                _unlockedAchs.AddRange(api.Response.GetUnlockedAchIds());
            }
            
            // Display existing unlocks and update mastery status
            MessageTool.Log($"{User} has earned {UnlockedAchs.Count}/{RetroAchievements.GetAchievementCount()} achievements for {RetroAchievements.GetGameName()}");
            UpdateMastery(display: false);

            // Start pinging and retrying failed unlocks
            _isLogin = true;
            PingCommand.Invoke(this, null);
            _pingTimer.Start();
            _retryTimer.Start();

            // Give the achievement buff to the player if in-game
            if (!Main.gameMenu)
                Main.LocalPlayer.GetModPlayer<RetroAchievementPlayer>().GiveAchievementBuff();
        }

        /// <summary>
        /// StartSessionCommand event callback to start a game session for the user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void NetworkSystem_StartSessionCommand(object sender, EventArgs args) => await StartSession();

        /// <summary>
        /// Send a game activity ping for the user
        /// </summary>
        /// <param name="rp">Rich presence</param>
        /// <returns>Asynchronous task</returns>
        private async Task Ping(string rp)
        {
            // Don't try to ping if not logged in
            if (!IsLogin)
                return;

            MessageTool.ModLog($"Sending a game activity ping for {RetroAchievements.GetGameId()}...");
            ApiResponse<BaseResponse> api = await NetworkInterface.TryPing(_client, _header, rp);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                MessageTool.ModLog($"Unable to send game activity ping ({api.Failure})");
                return;
            }

            if (!api.Response.Success)
            {
                MessageTool.ModLog($"Unable to send game activity ping ({api.Response.Error})");
                return;
            }      
        }

        /// <summary>
        /// Timer.Elapsed event callback to send a game activity ping for the user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private void PingTimer_Elapsed(object sender, ElapsedEventArgs args) => PingCommand.Invoke(this, null);

        /// <summary>
        /// PingCommand event callback to send a game activity ping for the user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void NetworkSystem_PingCommand(object sender, EventArgs args) => await Ping(RichPresenceSystem.GetRichPresence());

        /// <summary>
        /// Unlock an achievement for the user
        /// </summary>
        /// <param name="name">Achievement name</param>
        /// <param name="id">Achievement ID</param>
        /// <returns>Asynchronous task</returns>
        public async Task Unlock(string name, int id, bool retry=false)
        {
            // Don't try to unlock if not logged in
            if (!IsLogin)
                return;

            // Don't attempt to unlock achievements that are not a Core achievement
            if (!RetroAchievements.IsCoreAchievement(name))
                return;

            // Don't unlock achievements with an invalid ID
            // If this happens, ensure the achievement and its ID are in the JSON file
            if (id == 0)
                return;

            // Don't attempt to unlock achievements that are already unlocked on the RA server
            if (UnlockedAchs.Contains(id))
                return;

            MessageTool.ModLog($"Unlocking achievement {id}...");
            ApiResponse<AwardAchievementResponse> api = await NetworkInterface.TryAwardAchievement(_client, _header, id);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                if (!retry)
                {
                    _failedAchs.TryAdd(name, id);
                    MessageTool.ChatLog($"Unable to unlock [a:{name}] ({api.Failure}). Will retry periodically in the background...");
                }
                MessageTool.ModLog($"Unable to unlock achievement {id} ({api.Failure})");
                return;
            }

            if (!api.Response.Success)
            {
                if (!retry)
                {
                    _failedAchs.TryAdd(name, id);
                    MessageTool.ChatLog($"Unable to unlock [a:{name}] ({api.Response.Error}). Will retry periodically in the background...");
                }
                MessageTool.ModLog($"Unable to unlock achievement {id} ({api.Response.Error})");
                return;
            }
            
            _unlockedAchs.Add(id);
            UpdateMastery();
            _failedAchs.Remove(name);

            MessageTool.ChatLog($"{User} has unlocked [a:{name}]!");
        }

        /// <summary>
        /// UnlockAchievementCommand event callback to unlock an achievement for the user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void RuleSystem_UnlockAchievementCommand(object sender, UnlockAchievementEventArgs args) => await Unlock(args.Name, RetroAchievements.GetAchievementId(args.Name));

        /// <summary>
        /// Timer.Elapsed event callback to retry failed achievement unlocks
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private void RetryTimer_Elapsed(object sender, ElapsedEventArgs args) => RetryCommand.Invoke(this, null);

        /// <summary>
        /// RetryCommand event callback to retry failed achievement unlocks
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void NetworkSystem_RetryCommand(object sender, EventArgs args)
        {
            foreach (var ach in _failedAchs)
                await Unlock(ach.Key, ach.Value, retry:true);
        }
    }
}
