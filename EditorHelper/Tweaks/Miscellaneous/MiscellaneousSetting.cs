using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.Miscellaneous {
    public class MiscellaneousSetting : ITweakSetting {
        [Label(LangCode.English, "Remove 'ff' end of the RGBA color")]
        [Label(LangCode.Korean, "RGBA 코드 마지막에 오는 'ff' 제거")]
        [Draw] public bool RemoveFFOnRGBA = true;
        
        [Label(LangCode.English, "Show degree on tile")]
        [Label(LangCode.Korean, "타일에 각도 표시")]
        [Draw] public bool ShowDegree = true;
        
        [Label(LangCode.English, "Show 15 degs in editor while pressing ~ key")]
        [Label(LangCode.Korean, "에디터에서 ~키를 누르는 동안 15도 단위 각도 표시")]
        [Draw] public bool GraveToSee15Degs = true;
        
        public void OnGUI() {
            this.Draw();
        }
    }
}