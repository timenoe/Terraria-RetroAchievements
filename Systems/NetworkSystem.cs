﻿using System;
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
        public const int PingInterval = 5;

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
        /// Active user
        /// </summary>
        public string User => _header.user;


        public override void OnModLoad()
        {
            if (Main.dedServ)
                return;

            _cachePath = $"{ModLoader.ModPath}/RetroAchievements.nbt";
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

            try
            {
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

            // Take the achievement buff from the player if in-game
            if (!Main.gameMenu)
                Main.LocalPlayer.GetModPlayer<RetroAchievementPlayer>().TakeAchievementBuff();

            // Reset internal state
            _unlockedAchs = [];
            UpdateMastery();
            _pingTimer.Stop();
            _retryTimer.Stop();
            _isLogin = false;

            MessageTool.Log($"Logged out {oldUser}");
        }

        /// <summary>
        /// Start a game session for the user
        /// </summary>
        /// <returns>Asynchronous task</returns>
        private async Task StartSession()
        {
            MessageTool.ModLog($"Starting a game session for {RetroAchievements.GetGameId()}...");
            ApiResponse<StartSessionResponse> api = await NetworkInterface.TryStartSession(_client, _header);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                MessageTool.Log($"Unable to start game session ({api.Failure})");
                return;
            }

            if (!api.Response.Success)
            {
                MessageTool.Log($"Unable to start game session ({api.Response.Error})");
                return;
            }

            // Give the achievement buff to the player if in-game
            if (!Main.gameMenu)
                Main.LocalPlayer.GetModPlayer<RetroAchievementPlayer>().GiveAchievementBuff();

            // Get existing achievement unlocks and update mastery status
            _unlockedAchs = api.Response.GetUnlockedAchIds();
            UpdateMastery();

            // Start sending activity pings
            PingCommand.Invoke(this, null);
            _pingTimer.Start();

            // Start retrying failed unlocks when they occur
            _retryTimer.Start();

            _isLogin = true;
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

            // Don't unlock achievements with an invalid ID
            // If this happens, ensure the achievement and its ID are in the JSON file
            if (id == 0)
                return;

            // Don't attempt to unlock achievements that are already unlocked on the RA server
            if (_unlockedAchs.Contains(id))
                return;

            // Don't unlock achievements that are not in core
            if (!RetroAchievements.IsCoreAchievement(name))
                return;

            MessageTool.ModLog($"Unlocking achievement {id}...");
            ApiResponse<AwardAchievementResponse> api = await NetworkInterface.TryAwardAchievement(_client, _header, id);

            if (!string.IsNullOrEmpty(api.Failure))
            {
                if (!retry)
                {
                    _failedAchs.TryAdd(name, id);
                    MessageTool.ChatLog($"Unable to unlock [a:{name}] ({api.Failure})");
                }
                MessageTool.ModLog($"Unable to unlock achievement {id} ({api.Failure})");
                return;
            }

            if (!api.Response.Success)
            {
                if (!retry)
                {
                    _failedAchs.TryAdd(name, id);
                    MessageTool.ChatLog($"Unable to unlock [a:{name}] ({api.Response.Error})");
                }
                MessageTool.ModLog($"Unable to unlock achievement {id} ({api.Response.Error})");
                return;
            }
            
            _unlockedAchs.Add(id);
            UpdateMastery();
            _failedAchs.Remove(name);

            MessageTool.ChatLog($"{_header.user} has unlocked [a:{name}]!");
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
