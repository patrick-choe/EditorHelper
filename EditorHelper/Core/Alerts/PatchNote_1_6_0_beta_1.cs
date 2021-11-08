using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class PatchNote_1_6_0_beta_1 : Alert {
        protected override (int, int) WindowSize => (960, 540);
        protected override (int, int) TargetRes => (1920, 1080);

        private GUIStyle _updateList = null!;
        
        public const string PatchNoteKR =
            @" - R77 지원
      - 수식 입력 기능 삭제 (공식 지원)
      - 스크롤 위치 유지 기능 삭제 (공식 지원)
      - WAV 파일 입력 기능 삭제 (공식 지원)
 - 오프셋 자동 입력 기능 임시 삭제 (버그로 인한)
 - 이벤트 번들 다중 타일에 입력 추가
";
        public const string PatchNoteEN =
            @" - Supports R77
      - Remove using formulas in input fields (Official support)
      - Remove static scroll position (Official support)
      - Remove WAV file support (Official support)
 - Temporary Remove auto-offset (Bug)
 - Apply Event bundle to multiple tile 
";

        public override void Init() {
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Close"), (LangCode.Korean, "닫기")),
                () => {
                    Main.Settings.PatchNote_1_6_0_beta_1 = true;
                    Close();
                }
            );

            _updateList = new GUIStyle {
                font = Assets.SettingFontRegular
            };
        }

        public override void Content() {
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(65)}>EditorHelper 1.6.0 Beta 1</size></color>", SettingTitle);
            GUIEx.BeginIndent(20);
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(22)}>" + GUIEx.CheckLangCode((LangCode.English, PatchNoteEN), (LangCode.Korean, PatchNoteKR)) + "</size></color>", _updateList);
            GUIEx.EndIndent();
        }
    }
}