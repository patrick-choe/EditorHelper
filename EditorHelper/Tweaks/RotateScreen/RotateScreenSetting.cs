using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace EditorHelper.Tweaks.RotateScreen {
    public class RotateScreenSetting : ITweakSetting {
        [Label(LangCode.English, "Screen rotation delta degree <size=13>(Only mesh model)</size>")]
        [Label(LangCode.Korean, "화면 회전 각도 <size=13>(자유각도 레벨에서)</size>")]
        [Draw(0, 360)] public Fraction DeltaDeg = 15;
        
        [Label(LangCode.English, "Rotate screen clockwise")]
        [Label(LangCode.Korean, "화면 시계방향 회전")]
        [Draw] public KeyMap RotateScreenCW = new KeyMap(KeyCode.Period, AdditionalKey.Ctrl | AdditionalKey.BackQuote);

        [Label(LangCode.English, "Rotate screen counter-clockwise")]
        [Label(LangCode.Korean, "화면 반시계방향 회전")]
        [Draw] public KeyMap RotateScreenCCW = new KeyMap(KeyCode.Comma, AdditionalKey.Ctrl | AdditionalKey.BackQuote);

        public void OnGUI() {
            this.Draw();
        }
    }
}