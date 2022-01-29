using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class PatchNote: Alert {
        protected override (int, int) WindowSize => (960, 540);
        protected override (int, int) TargetRes => (1920, 1080);

        private GUIStyle _updateList = null!;

        public const string PatchNoteKR = " - R80 지원\n - 드래그로 각도 변경 기능 임시 삭제";
/*@" - 내부 구조 변경
 - 각각의 기능을 트윅으로 세분화
 - 각종 버그 수정
 - R78 지원";*/
        public const string PatchNoteEN = " - R80 support \n - Temporary removed Change angle by dragging feature";
/*@" - Refactored code
 - Split each functions to tweak
 - Fixed some bugs
 - R78 support";*/

        public override void Init() {
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Close"), (LangCode.Korean, "닫기")),
                () => {
                    Main.Settings.PatchNote_2_1_alpha_1 = true;
                    Close();
                }
            );

            _updateList = new GUIStyle {
                font = Assets.SettingFontRegular
            };
        }

        public override void Content() {
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(65)}>EditorHelper 2.1.0.1</size></color>", SettingTitle);
            GUIEx.BeginIndent(20);
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(22)}>" + GUIEx.CheckLangCode((LangCode.English, PatchNoteEN), (LangCode.Korean, PatchNoteKR)) + "</size></color>", _updateList);
            GUIEx.EndIndent();
        }
    }
}