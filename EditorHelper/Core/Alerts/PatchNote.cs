using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class PatchNote: Alert {
        protected override (int, int) WindowSize => (960, 540);
        protected override (int, int) TargetRes => (1920, 1080);

        private GUIStyle _updateList = null!;

        public const string PatchNoteKR = " - r78(2) 대응\n - EventBundle 재추가";
/*@" - 내부 구조 변경
 - 각각의 기능을 트윅으로 세분화
 - 각종 버그 수정
 - R78 지원";*/
        public const string PatchNoteEN = " - r78(2) support\n - EventBundle re-added";
/*@" - Refactored code
 - Split each functions to tweak
 - Fixed some bugs
 - R78 support";*/

        public override void Init() {
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Close"), (LangCode.Korean, "닫기")),
                () => {
                    Main.Settings.PatchNote_2_0_beta_3 = true;
                    Close();
                }
            );

            _updateList = new GUIStyle {
                font = Assets.SettingFontRegular
            };
        }

        public override void Content() {
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(65)}>EditorHelper 2.0 Beta 3</size></color>", SettingTitle);
            GUIEx.BeginIndent(20);
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(22)}>" + GUIEx.CheckLangCode((LangCode.English, PatchNoteEN), (LangCode.Korean, PatchNoteKR)) + "</size></color>", _updateList);
            GUIEx.EndIndent();
        }
    }
}