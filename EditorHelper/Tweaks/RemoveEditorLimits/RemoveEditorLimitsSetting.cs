using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.RemoveEditorLimits {
    public class RemoveEditorLimitsSetting : ITweakSetting {
        [Label(LangCode.English, "Remove input value limit")]
        [Label(LangCode.Korean, "입력값 제한 해제")]
        [Draw] public bool RemoveInputValueLimit = true;
        
        [Label(LangCode.English, "Enable first floor events")]
        [Label(LangCode.Korean, "첫 타일 이벤트 허용")]
        [Draw] public bool EnableFirstFloorEvents = true;

        [Label(LangCode.English, "Allow Mp3 file")]
        [Label(LangCode.Korean, "Mp3 형식 허용")]
        [Draw] public bool AllowMp3 = false;

        public void OnGUI() {
            this.Draw();
        }
    }
}