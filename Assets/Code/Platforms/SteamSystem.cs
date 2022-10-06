#if STEAM_SDK
using System;
using UnitySystemFramework.Core;
using UnitySystemFramework.Utility;
using Steamworks;
using UnityEngine;

namespace UnitySystemFramework.Platforms
{
    public class SteamSystem : UpdateSystem, IPlatformSystem
    {
        protected SteamAPIWarningMessageHook_t _warningHook;

        public bool IsInitialized { get; private set; }

        public ulong SteamID { get; private set; } = 1;
        public string DisplayName { get; private set; } = "Local Player";
        public Texture2D Avatar { get; private set; }

        public PlatformSDK SDK => PlatformSDK.Steam;

        protected override void OnInit()
        {
        }

        protected override void OnStart()
        {
            if (GetGlobal("IsServer"))
            {
                DisplayName = "Server";
                SteamID = 9999999999;
                return;
            }

#if STEAMWORKS
            if (!Packsize.Test())
            {
                LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }

            if (!DllCheck.Test())
            {
                LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }

#if STEAM_DRM
            try
            {
                // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
                // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(STEAM_APP_ID)))
                {
                    Game.Exit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            { // We catch this exception here, as it will be the first occurrence of it.
                LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);

                Game.Exit();
                return;
            }
#endif

            // Initializes the Steamworks API.
            // If this returns false then this indicates one of the following conditions:
            // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
            // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
            // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
            // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
            // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
            // Valve's documentation for this is located here:
            // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            try
            {
                IsInitialized = SteamAPI.Init();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            if (!IsInitialized)
            {
                LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
                return;
            }

            if (_warningHook == null)
            {
                // Set up our callback to receive warning messages from Steam.
                // You must launch with "-debug_steamapi" in the launch args to receive warnings.
                _warningHook = SteamDebugMessageHook;
                try
                {
                    SteamClient.SetWarningMessageHook(_warningHook);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }

            SteamID = SteamUser.GetSteamID().m_SteamID;
            DisplayName = SteamFriends.GetPersonaName();
            Avatar = GetUserAvatar(SteamID);

            SteamApps.GetLaunchCommandLine(out string commandLine, 0);
            if (!string.IsNullOrWhiteSpace(commandLine))
            {
                var props = EnvironmentUtil.ParseCL(commandLine);
                if (EnvironmentUtil.TryGetCL("ip", out string ip, props))
                    SetGlobal("IP", ip);
                if (EnvironmentUtil.TryGetCL("port", out ushort port, props))
                    SetGlobal("Port", port);
                if (EnvironmentUtil.TryGetCL("lport", out ushort lport, props))
                    SetGlobal("LobbyPort", lport);
                if (EnvironmentUtil.TryGetCL("lobbyport", out ushort lobbyport, props))
                    SetGlobal("LobbyPort", lobbyport);
                if (EnvironmentUtil.HasCL("join", props))
                    SetGlobal("IsAutoJoin", true);
            }
#endif
        }

        protected override void OnUpdate()
        {
            if (!IsInitialized)
                return;

            // Run Steam client callbacks
            SteamAPI.RunCallbacks();
        }

        protected override void OnEnd()
        {
            if (IsInitialized)
                SteamAPI.Shutdown();
            IsInitialized = false;
        }

        protected void SteamDebugMessageHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            LogWarning(pchDebugText);
        }

        public Texture2D GetUserAvatar(ulong steamID)
        {
            var avatarID = SteamFriends.GetSmallFriendAvatar(new CSteamID(steamID));
            if (SteamUtils.GetImageSize(avatarID, out var width, out var height))
            {
                var buffer = new byte[4 * width * height * sizeof(char)];
                if (SteamUtils.GetImageRGBA(avatarID, buffer, buffer.Length))
                {
                    var texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    texture.SetPixelData(buffer, 0);
                    texture.Apply(false);
                    return texture;
                }
            }

            return default;
        }
    }
}
#endif