using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class PatchNote_2_0_alpha_1 : Alert {
        protected override (int, int) WindowSize => (960, 540);
        protected override (int, int) TargetRes => (1920, 1080);

        private GUIStyle _updateList = null!;
        
        public const string PatchNoteKR = 
@" - 내부 구조 변경
 - 각각의 기능을 트윅으로 세분화
 - EventBundle, BPM 측정 기능 임시 삭제
 - 각종 버그 수정";
        public const string PatchNoteEN =
@" - Refactored code
 - Split each functions to tweak
 - EventBundle, BPM Detect temporary disabled
 - Fixed some bugs";

        public override void Init() {
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Close"), (LangCode.Korean, "닫기")),
                () => {
                    Main.Settings.PatchNote_2_0_alpha_1 = true;
                    Close();
                }
            );

            _updateList = new GUIStyle {
                font = Assets.SettingFontRegular
            };
        }

        public override void Content() {
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(65)}>EditorHelper 2.0 alpha 1</size></color>", SettingTitle);
            GUIEx.BeginIndent(20);
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(22)}>" + GUIEx.CheckLangCode((LangCode.English, PatchNoteEN), (LangCode.Korean, PatchNoteKR)) + "</size></color>", _updateList);
            GUIEx.EndIndent();
        }
    }
}