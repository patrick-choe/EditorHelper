using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace EditorHelper.Tweaks.RotateSmallerDeg {
    public class RotateSmallerDegSetting : ITweakSetting {
        [Label(LangCode.English, "Tile rotation delta degree<size=13>\n(Only mesh model)</size>")]
        [Label(LangCode.Korean, "타일 회전 각도 <size=13>(자유각도 레벨에서)</size>")]
        [Draw(0, 360)] public Fraction DeltaDeg = 15;
        
        [Label(LangCode.English, "Rotate tiles clockwise")]
        [Label(LangCode.Korean, "타일 시계방향 회전")]
        [Draw] public KeyMap RotateCW = new KeyMap(KeyCode.Period, AdditionalKey.Ctrl | AdditionalKey.Alt);
        
        [Label(LangCode.English, "Rotate tiles counter-clockwise")]
        [Label(LangCode.Korean, "타일 반시계방향 회전")]
        [Draw] public KeyMap RotateCCW = new KeyMap(KeyCode.Comma, AdditionalKey.Ctrl | AdditionalKey.Alt);

        public void OnGUI() {
            this.Draw();
        }
    }
}