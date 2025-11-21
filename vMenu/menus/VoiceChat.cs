using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using static vMenuClient.CommonFunctions;
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
        public List<string> channels = new()
        {
            "채널 1 (기본)",
            "채널 2",
            "채널 3",
            "채널 4",
        };
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
            currentChannel = channels[0];
            if (IsAllowed(Permission.VCStaffChannel))
            {
                channels.Add("스태프 채널");
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "음성 채팅 설정");

            var voiceChatEnabled = new MenuCheckboxItem("음성 채팅 활성화", "음성 채팅을 활성화하거나 비활성화합니다.", EnableVoicechat);
            var showCurrentSpeaker = new MenuCheckboxItem("현재 대화 중인 사람 표시", "현재 누가 말하고 있는지 표시합니다.", ShowCurrentSpeaker);
            var showVoiceStatus = new MenuCheckboxItem("마이크 상태 표시", "마이크가 열려 있는지 음소거 상태인지 표시합니다.", ShowVoiceStatus);

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
                "전역",
            };
            var voiceChatProximity = new MenuItem("음성 채팅 범위 (" + ConvertToMetric(currentProximity) + ")", "음성 채팅 수신 범위를 미터 단위로 설정합니다. 전역은 0으로 설정하세요.");
            var voiceChatChannel = new MenuListItem("음성 채팅 채널", channels, channels.IndexOf(currentChannel), "음성 채팅 채널을 설정합니다.");

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
                    Subtitle.Custom($"새로운 음성 채팅 채널: ~b~{channels[newIndex]}~s~.");
                }
            };
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == voiceChatProximity)
                {
                    var result = await GetUserInput(windowTitle: $"미터 단위로 범위 입력. 현재: ({ConvertToMetric(currentProximity)})", maxInputLength: 6);

                    if (float.TryParse(result, out var resultfloat))
                    {
                        currentProximity = resultfloat;
                        Subtitle.Custom($"새로운 음성 채팅 범위: ~b~{ConvertToMetric(currentProximity)}~s~.");
                        voiceChatProximity.Text = ("음성 채팅 범위 (" + ConvertToMetric(currentProximity) + ")");
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
                val = "전역";
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
