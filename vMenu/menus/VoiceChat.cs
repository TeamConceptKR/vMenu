using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using static vMenuClient.CommonFunctions;
using static vMenuClient.Localization;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class VoiceChat
    {
        // Variables
        private Menu menu;
        public bool EnableVoicechat = UserDefaults.VoiceChatEnabled;
        public bool ShowCurrentSpeaker = UserDefaults.ShowCurrentSpeaker;
        public bool ShowVoiceStatus = UserDefaults.ShowVoiceStatus;
        public float currentProximity = (GetSettingsFloat(Setting.vmenu_override_voicechat_default_range) != 0.0) ? GetSettingsFloat(Setting.vmenu_override_voicechat_default_range) : UserDefaults.VoiceChatProximity;
        public List<string> channels;
        public string currentChannel;
        private readonly List<float> proximityRange = new()
        {
            5f, // 5m
            10f, // 10m
            15f, // 15m
            20f, // 20m
            100f, // 100m
            300f, // 300m
            1000f, // 1.000km
            2000f, // 2.000km
            0f, // global
        };


        private void CreateMenu()
        {
            // Initialize channels with localized strings
            channels = new List<string>()
            {
                GetString("VoiceChat_Channel1"),
                GetString("VoiceChat_Channel2"),
                GetString("VoiceChat_Channel3"),
                GetString("VoiceChat_Channel4"),
            };
            
            currentChannel = channels[0];
            if (IsAllowed(Permission.VCStaffChannel))
            {
                channels.Add(GetString("VoiceChat_StaffChannel"));
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, GetString("VoiceChat_Title"));

            var voiceChatEnabled = new MenuCheckboxItem(GetString("VoiceChat_Enable"), GetString("VoiceChat_Enable_Desc"), EnableVoicechat);
            var showCurrentSpeaker = new MenuCheckboxItem(GetString("VoiceChat_ShowSpeaker"), GetString("VoiceChat_ShowSpeaker_Desc"), ShowCurrentSpeaker);
            var showVoiceStatus = new MenuCheckboxItem(GetString("VoiceChat_ShowMicStatus"), GetString("VoiceChat_ShowMicStatus_Desc"), ShowVoiceStatus);

            var proximity = new List<string>()
            {
                "5 m",
                "10 m",
                "15 m",
                "20 m",
                "100 m",
                "300 m",
                "1 km",
                "2 km",
                "Global",
            };
            var voiceChatProximity = new MenuItem(GetString("VoiceChat_Proximity", ConvertToMetric(currentProximity)), GetString("VoiceChat_Proximity_Desc"));
            var voiceChatChannel = new MenuListItem(GetString("VoiceChat_Channel"), channels, channels.IndexOf(currentChannel), GetString("VoiceChat_Channel_Desc"));

            if (IsAllowed(Permission.VCEnable))
            {
                menu.AddMenuItem(voiceChatEnabled);

                // Nested permissions because without voice chat enabled, you wouldn't be able to use these settings anyway.
                if (IsAllowed(Permission.VCShowSpeaker))
                {
                    menu.AddMenuItem(showCurrentSpeaker);
                }

                menu.AddMenuItem(voiceChatProximity);
                menu.AddMenuItem(voiceChatChannel);
                menu.AddMenuItem(showVoiceStatus);
            }

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == voiceChatEnabled)
                {
                    EnableVoicechat = _checked;
                }
                else if (item == showCurrentSpeaker)
                {
                    ShowCurrentSpeaker = _checked;
                }
                else if (item == showVoiceStatus)
                {
                    ShowVoiceStatus = _checked;
                }
            };

            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == voiceChatChannel)
                {
                    currentChannel = channels[newIndex];
                    Subtitle.Custom(GetString("VoiceChat_ChannelSet", channels[newIndex]));
                }
            };
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == voiceChatProximity)
                {
                    var result = await GetUserInput(windowTitle: GetString("VoiceChat_ProximityPrompt", ConvertToMetric(currentProximity)), maxInputLength: 6);

                    if (float.TryParse(result, out var resultfloat))
                    {
                        currentProximity = resultfloat;
                        Subtitle.Custom(GetString("VoiceChat_ProximitySet", ConvertToMetric(currentProximity)));
                        voiceChatProximity.Text = GetString("VoiceChat_Proximity", ConvertToMetric(currentProximity));
                    }
                }
            };

        }
        static string ConvertToMetric(float input)
        {
            string val = "0m";
            if (input < 1.0)
            {
                val = (input * 100) + "cm";
            }
            else if (input >= 1.0)
            {
                if (input < 1000)
                {
                    val = input + "m";
                }
                else
                {
                    val = (input / 1000) + "km";
                }
            }
            if (input == 0)
            {
                val = "global";
            }
            return val;
        }
        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
