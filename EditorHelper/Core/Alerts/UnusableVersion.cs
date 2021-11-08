using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class UnusableVersion : Alert {
        protected override (int, int) WindowSize => (960, 540);
        protected override (int, int) TargetRes => (1920, 1080);

        public override void Init() {
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Disable EditorHelper"), (LangCode.Korean, "EditorHelper 비활성화")),
                () => {
                    Main._mod!.Active = false;
                    Main.Unusable = true;
                    Close();
                }
            );
        }

        public override void Content() {
            GUILayout.Label(GUIEx.CheckLangCode(
                (LangCode.English, $"<color=#ff3333><size={MatchRes(65)}>You cannot use EditorHelper currently</size></color>"),
                (LangCode.Korean, $"<color=#ff3333><size={MatchRes(65)}>EditorHelper를 현재 사용할 수 없습니다.</size></color>")), SettingTitle);
            GUILayout.Label(GUIEx.CheckLangCode((LangCode.English, $"<color=#ffffff><size={MatchRes(40)}>Compatible ADOFAI version: r{Main.Target}\nCurrent version: r{Main.ReleaseNum}</size></color>"),
                (LangCode.Korean, $"<color=#ffffff><size={MatchRes(40)}>호환되는 ADOFAI 버전: r{Main.Target}\n현재 버전: r{Main.ReleaseNum}</size></color>")), SettingTitle);
            GUILayout.Label(GUIEx.CheckLangCode((LangCode.English, $"<color=#ffffff><size={MatchRes(40)}>Use another version of EditorHelper.</size></color>"),
                (LangCode.Korean, $"<color=#ffffff><size={MatchRes(40)}>다른 버전의 EditorHelper을 사용하십시오.</size></color>")), SettingTitle);
        }
    }
}