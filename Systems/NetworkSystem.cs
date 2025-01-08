using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using RASharpIntegration.Network;
using RetroAchievements.Commands;
using RetroAchievements.Utils;
using RetroAchievements.Players;
using Terraria;

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
        public const int PingInterval = 5;


        /// <summary>
        /// HTTP client to send requests with
        /// </summary>
        private readonly HttpClient _client = new();

        /// <summary>
        /// Timer to periodically send a game activity ping for the user
        /// </summary>
        private readonly Timer _pingTimer = new(TimeSpan.FromMinutes(PingInterval).TotalMilliseconds);

        /// <summary>
        /// True if the game has been mastered
        /// </summary>
        private bool _isMastered;

        /// <summary>
        /// True if the game session has been started
        /// </summary>
        private bool _isStarted;

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
        /// True if the game has been mastered
        /// </summary>
        public bool IsMastered
        {
            get { return _isMastered; }
        }

        /// <summary>
        /// True if the game session has been started
        /// </summary>
        public bool IsStarted
        {
            get { return _isStarted; }
        }

        /// <summary>
        /// Active user
        /// </summary>
        public string User
        {
            get { return _header.user; }
        }


        public override void OnModLoad()
        {
            if (!RetroAchievements.IsEnabled)
                return;
            
            _cachePath = $"{ModLoader.ModPath}/RetroAchievements.nbt";
            _header = new(
                RetroAchievements.Host, 
                game: RetroAchievements.GetGameId(),
                hardcore: RetroAchievements.IsHardcore);

            // Subscribe to internal events
            _pingTimer.Elapsed += PingTimer_Elapsed;
            StartSessionCommand += NetworkSystem_StartSessionCommand;
            PingCommand += NetworkSystem_PingCommand;

            // Subscribe to external events
            ModContent.GetInstance<RaCommand>().LoginCommand += RaCommand_LoginCommand;
            ModContent.GetInstance<AchievementSystem>().UnlockAchievementCommand += NetworkSystem_UnlockAchievementCommand;

            // Ensure HTTP client has proper User Agent for RA requests
            SetupUserAgent();

            // If user credentials already in cache, start the game session
            if (TryGetCredentials())
                StartSessionCommand.Invoke(this, null);
        }

        public override void OnModUnload()
        {
            if (!RetroAchievements.IsEnabled)
                return;

            // Unsubscribe from internal events
            StartSessionCommand -= NetworkSystem_StartSessionCommand;
            PingCommand -= NetworkSystem_PingCommand;
            _pingTimer.Elapsed -= PingTimer_Elapsed;

            // Unsubscribe from external events
            ModContent.GetInstance<RaCommand>().LoginCommand -= RaCommand_LoginCommand;
            ModContent.GetInstance<AchievementSystem>().UnlockAchievementCommand -= NetworkSystem_UnlockAchievementCommand;

            // Handle IDisposable objects
            _client.Dispose();
            _pingTimer.Dispose();
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

            MessageUtil.ModLog($"Cached login credentials for {User}");
        }

        /// <summary>
        /// Setup the HTTP client's User Agent for making requests to RA
        /// </summary>
        private void SetupUserAgent()
        {
            // RA requires a user agent of {Standalone name}/{x.y.z Version}
            Version version = ModContent.GetInstance<RetroAchievements>().Version;
            _client.DefaultRequestHeaders.Add("User-Agent", $"TerrariaRetroAchievements/{version}");
        }
        
        /// <summary>
        /// Update mastery status<br/>
        /// Mastery occurs when all available achievements have been unlocked
        /// </summary>
        private void UpdateMastery()
        {
            _isMastered = _unlockedAchs.Count == RetroAchievements.GetAchievementCount();
            if (IsMastered)
            {
                if (RetroAchievements.IsHardcore)
                    MessageUtil.Log($"{_header.user} has mastered {RetroAchievements.GetGameName()}!");
                else
                    MessageUtil.Log($"{_header.user} has completed {RetroAchievements.GetGameName()}!");
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

            try
            {
                string user = tag.GetString("user");
                string token = tag.GetString("token");

                // Ensure credentials aren't empty
                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(token))
                    return false;

                _header.user = user;
                _header.token = token;
                MessageUtil.ModLog($"Retrieved cached login credentials for {User}");
                return true;              
            }

            // Will throw exception is the key type is not a string
            // Can only happen if the file has been tampered with
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Login a user
        /// </summary>
        /// <param name="user">User name</param>
        /// <param name="pass">User password</param>
        /// <returns>Asynchronous task</returns>
        private async Task Login(string user, string pass)
        {
            _header.user = user;
            MessageUtil.Log($"Logging in {user} to {_header.host}...");

            ApiResponse<LoginResponse> api = await NetworkInterface.TryLogin(_client, _header, pass);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                MessageUtil.Log($"Unable to login due to exception: {api.Failure}");
                return;
            }

            if (!api.Response.Success)
            {
                MessageUtil.Log($"Unable to login due to error: {api.Response.Error}");
                return;
            }
                
            _header.token = api.Response.Token;
            CacheCredentials();
            MessageUtil.Log($"{user} has successfully logged in!");

            StartSessionCommand.Invoke(this, null);
        }

        /// <summary>
        /// LoginCommand event callback to login a user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void RaCommand_LoginCommand(object sender, LoginEventArgs args)
        {
            await Login(args.User, args.Password);
        }

        /// <summary>
        /// Start a game session for the user
        /// </summary>
        /// <returns>Asynchronous task</returns>
        private async Task StartSession()
        {
            MessageUtil.ModLog($"Starting a game session for {RetroAchievements.GetGameId()}...");

            ApiResponse<StartSessionResponse> api = await NetworkInterface.TryStartSession(_client, _header);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                MessageUtil.Log($"Unable to start game session due to exception: {api.Failure}");
                return;
            }

            if (!api.Response.Success)
            {
                MessageUtil.Log($"Unable to start game session due to error: {api.Response.Error}");
                return;
            }

            // Get existing achievement unlocks and update mastery status
            _unlockedAchs = api.Response.GetUnlockedAchIds();
            UpdateMastery();

            // Apply the achievement buff to the player if in-game
            if (!Main.gameMenu)
                Main.LocalPlayer.GetModPlayer<AchievementPlayer>().GiveAchievementBuff();

            // Start sending activity pings
            PingCommand.Invoke(this, null);
            _pingTimer.Start();

            _isStarted = true;
            MessageUtil.ChatLog($"{_header.user} has started a game session for {RetroAchievements.GetGameName()}!");
        }

        /// <summary>
        /// StartSessionCommand event callback to start a game session for the user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void NetworkSystem_StartSessionCommand(object sender, EventArgs args)
        {
            await StartSession();
        }

        /// <summary>
        /// Send a game activity ping for the user
        /// </summary>
        /// <param name="rp">Rich presence</param>
        /// <returns>Asynchronous task</returns>
        private async Task Ping(string rp)
        {
            MessageUtil.ModLog($"Sending a game activity ping for {RetroAchievements.GetGameId()}...");
            ApiResponse<BaseResponse> api = await NetworkInterface.TryPing(_client, _header, rp);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                MessageUtil.ModLog($"Unable to send game activity ping due to exception: {api.Failure}");
                return;
            }

            if (!api.Response.Success)
            {
                MessageUtil.ModLog($"Unable to send game activity ping due to error: {api.Response.Error}");
                return;
            }      
        }

        /// <summary>
        /// Timer.Elapsed event callback to send a game activity ping for the user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private void PingTimer_Elapsed(object sender, ElapsedEventArgs args)
        {
            PingCommand.Invoke(this, null);
        }

        /// <summary>
        /// PingCommand event callback to send a game activity ping for the user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void NetworkSystem_PingCommand(object sender, EventArgs args)
        {
            await Ping(RichPresenceSystem.GetRichPresence());
        }

        /// <summary>
        /// Unlock an achievement for the user
        /// </summary>
        /// <param name="name">Achievement name</param>
        /// <param name="id">Achievement ID</param>
        /// <returns>Asynchronous task</returns>
        public async Task Unlock(string name, int id)
        {
            // Do no unlock achievements that are already unlocked on the server
            if (_unlockedAchs.Contains(id))
                return;

            MessageUtil.ModLog($"Unlocking achievement {id}...");
            ApiResponse<AwardAchievementResponse> api = await NetworkInterface.TryAwardAchievement(_client, _header, id);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                MessageUtil.ChatLog($"Unable to unlock [a:{name}] due to exception: {api.Failure}");
                MessageUtil.ModLog($"Unable to unlock achievement {id} due to exception: {api.Failure}");
                return;
            }

            if (!api.Response.Success)
            {
                MessageUtil.ChatLog($"Unable to unlock [a:{name}] due to error: {api.Response.Error}");
                MessageUtil.ModLog($"Unable to unlock achievement {id} due to error: {api.Response.Error}");
                return;
            }
            
            _unlockedAchs.Add(id);
            MessageUtil.ChatLog($"{_header.user} has unlocked [a:{name}]!");

            UpdateMastery();
            
            // TODO: If an achievement request fails, keep trying periodically in the background
        }

        /// <summary>
        /// UnlockAchievementCommand event callback to unlock an achievement for the user
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event args</param>
        private async void NetworkSystem_UnlockAchievementCommand(object sender, UnlockAchievementEventArgs args)
        {
            await Unlock(args.Name, RetroAchievements.GetAchievementId(args.Name));
        }
    }
}
