using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace EditorHelper.Tweaks.BetterArtists {
    public class BetterArtistsSetting : ITweakSetting {
        [Label(LangCode.English, "Insert artist URL Automatically")]
        [Label(LangCode.Korean, "작곡가 URL 자동 입력")]
        [Draw(0, 360)] public bool AutoArtistURL = true;
        
        [Label(LangCode.English, "Better Artists Check")]
        [Label(LangCode.Korean, "더 나은 작곡가 확인")]
        [Draw] public bool BetterArtistCheck = true;

        public void OnGUI() {
            this.Draw();
        }
    }
}