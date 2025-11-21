using CitizenFX.Core;

using MenuAPI;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;

namespace vMenuClient.menus
{
    public class Recording
    {
        // Variables
        private Menu menu;

        private void CreateMenu()
        {
            AddTextEntryByHash(0x86F10CE6, "Cfx.re 포럼에 업로드"); // Replace the "Upload To Social Club" button in gallery
            AddTextEntry("ERROR_UPLOAD", "이 사진을 Cfx.re 포럼에 업로드하시겠습니까?"); // Replace the warning message text for uploading

            // Create the menu.
            menu = new Menu("녹화", "녹화 옵션");

            var takePic = new MenuItem("사진 촬영", "사진을 촬영하여 일시정지 메뉴 갤러리에 저장합니다.");
            var openPmGallery = new MenuItem("갤러리 열기", "일시정지 메뉴 갤러리를 엽니다.");
            var startRec = new MenuItem("녹화 시작", "GTA V 내장 녹화 기능을 사용하여 새 게임 녹화를 시작합니다.");
            var stopRec = new MenuItem("녹화 중지", "현재 녹화를 중지하고 저장합니다.");
            var openEditor = new MenuItem("록스타 에디터", "록스타 에디터를 엽니다. 일부 문제를 방지하려면 먼저 세션을 종료하는 것이 좋습니다.");

            menu.AddMenuItem(takePic);
            menu.AddMenuItem(openPmGallery);
            menu.AddMenuItem(startRec);
            menu.AddMenuItem(stopRec);
            menu.AddMenuItem(openEditor);

            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == startRec)
                {
                    if (IsRecording())
                    {
                        Notify.Alert("이미 클립을 녹화 중입니다. 다시 녹화를 시작하려면 먼저 녹화를 중지해야 합니다!");
                    }
                    else
                    {
                        StartRecording(1);
                    }
                }
                else if (item == openPmGallery)
                {
                    ActivateFrontendMenu((uint)GetHashKey("FE_MENU_VERSION_MP_PAUSE"), true, 3);
                }
                else if (item == takePic)
                {
                    BeginTakeHighQualityPhoto();
                    SaveHighQualityPhoto(-1);
                    FreeMemoryForHighQualityPhoto();
                }
                else if (item == stopRec)
                {
                    if (!IsRecording())
                    {
                        Notify.Alert("현재 클립을 녹화 중이 아닙니다. 클립을 중지하고 저장하려면 먼저 녹화를 시작해야 합니다.");
                    }
                    else
                    {
                        StopRecordingAndSaveClip();
                    }
                }
                else if (item == openEditor)
                {
                    if (GetSettingsBool(Setting.vmenu_quit_session_in_rockstar_editor))
                    {
                        QuitSession();
                    }
                    ActivateRockstarEditor();
                    // wait for the editor to be closed again.
                    while (IsPauseMenuActive())
                    {
                        await BaseScript.Delay(0);
                    }
                    // then fade in the screen.
                    DoScreenFadeIn(1);
                    Notify.Alert("록스타 에디터에 들어가기 전에 이전 세션을 종료했습니다. 서버의 메인 세션에 다시 참가하려면 게임을 다시 시작하세요.", true, true);
                }
            };

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
