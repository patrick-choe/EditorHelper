using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class IncompatibleVersion : Alert {
        protected override (int, int) WindowSize => (960, 540);
        protected override (int, int) TargetRes => (1920, 1080);

        public override void Init() {
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "I'll use anyway"), (LangCode.Korean, "무시하고 사용하기")), Close
            );
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Disable EditorHelper"), (LangCode.Korean, "EditorHelper 비활성화")),
                () => {
                    Main.Unusable = true;
                    Close();
                }
            );
        }

        public override void Content() {
            GUILayout.Label(GUIEx.CheckLangCode(
                (LangCode.English, $"<color=#ff3333><size={MatchRes(65)}>EditorHelper Version is Incompatible</size></color>"),
                (LangCode.Korean, $"<color=#ff3333><size={MatchRes(65)}>EditorHelper 버전이 호환되지 않습니다</size></color>")), SettingTitle);
            GUILayout.Label(GUIEx.CheckLangCode(
                    (LangCode.English,
                        $"<color=#ffffff><size={MatchRes(40)}>Compatible ADOFAI version: r{Main.Target}\nCurrent version: r{Main.ReleaseNum}</size></color>"),
                    (LangCode.Korean,
                        $"<color=#ffffff><size={MatchRes(40)}>호환되는 ADOFAI 버전: r{Main.Target}\n현재 버전: r{Main.ReleaseNum}</size></color>")),
                SettingTitle);
        }
    }
}