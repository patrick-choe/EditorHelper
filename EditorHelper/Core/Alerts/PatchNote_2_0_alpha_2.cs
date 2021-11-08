using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class PatchNote_2_0_alpha_2 : Alert {
        protected override (int, int) WindowSize => (960, 540);
        protected override (int, int) TargetRes => (1920, 1080);

        private GUIStyle _updateList = null!;
        
        public const string PatchNoteKR = 
@" - 트윅이 랜덤하게 작동하지 않는 버그 수정";
        public const string PatchNoteEN =
@" - Fixed a bug that tweak doesn't work randomly";

        public override void Init() {
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Close"), (LangCode.Korean, "닫기")),
                () => {
                    Main.Settings.PatchNote_2_0_alpha_2 = true;
                    Close();
                }
            );

            _updateList = new GUIStyle {
                font = Assets.SettingFontRegular
            };
        }

        public override void Content() {
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(65)}>EditorHelper 2.0 alpha 2</size></color>", SettingTitle);
            GUIEx.BeginIndent(20);
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(22)}>" + GUIEx.CheckLangCode((LangCode.English, PatchNoteEN), (LangCode.Korean, PatchNoteKR)) + "</size></color>", _updateList);
            GUIEx.EndIndent();
        }
    }
}