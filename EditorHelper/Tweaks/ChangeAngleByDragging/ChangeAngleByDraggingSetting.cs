using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Tweaks.ChangeAngleByDragging {
    public class ChangeAngleByDraggingSetting : ITweakSetting {
        [Label(LangCode.English, "Use delta degree")]
        [Label(LangCode.Korean, "기준 각도 사용")]
        [Draw] public bool UseDelta = false;
        
        [Label(LangCode.English, "Tile rotation delta degree")]
        [Label(LangCode.Korean, "타일 회전 각도")]
        [Draw(0, 360)] public Fraction MeshDelta = new(15, 4);
        
        public void OnGUI() {
            this.Draw();
        }
    }
}