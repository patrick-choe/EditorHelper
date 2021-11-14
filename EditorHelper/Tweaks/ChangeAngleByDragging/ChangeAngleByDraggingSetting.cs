using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Tweaks.ChangeAngleByDragging {
    public class ChangeAngleByDraggingSetting : ITweakSetting {
        [Label(LangCode.English, "Change tile angle")]
        [Label(LangCode.Korean, "타일 각도 변경")]
        [Draw] public KeyMap ChangeTileAngle = new KeyMap(KeyCode.None, AdditionalKey.Ctrl);
        
        [Label(LangCode.English, "Tile rotation delta degree")]
        [Label(LangCode.Korean, "타일 회전 각도")]
        [Draw(0, 360)] public Fraction MeshDelta = new(15, 4);
        
        public void OnGUI() {
            this.Draw();
        }
    }
}