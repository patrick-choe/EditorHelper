using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using EditorHelper.Core.TweakFunctions;
using EditorHelper.Core.Tweaks;
using EditorHelper.Tweaks.RotateSmallerDeg;
using EditorHelper.Utils;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.EventBundles {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Enable Event Bundles")]
    [TweakDescription(LangCode.Korean, "이벤트 번들 사용")]
    public class EventBundlesTweak : Tweak, IPatchClass<EventBundlesPatch> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
            if (GCS.settingsInfo == null) return;
            if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("EH:useLegacyFlash"))
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("EH:useLegacyFlash");
            if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("EH:useLegacyTiles"))
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("EH:useLegacyTiles");
        }
    }
}