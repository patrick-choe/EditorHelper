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
            PatchRDString.Translations["editor.EditorHelperEventBundles"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "<size=20>EditorHelper 이벤트 번들</size>"},
                {LangCode.English, "<size=20>EditorHelper Event Bundles</size>"},
            };

            PatchRDString.Translations["editor.EditorHelperAssetPacks"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "<size=20>EditorHelper 에셋 팩</size>"},
                {LangCode.English, "<size=20>EditorHelper Asset Packs</size>"},
            };
            
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
            if (GCS.levelEventsInfo == null) return;
            foreach (LevelEventTypeEx key in Enum.GetValues(typeof(LevelEventTypeEx))) {
                GCS.levelEventsInfo.Remove(key.ToString());
                if ((int) key >= 200) {
                    GCS.settingsInfo.Remove(key.ToString());
                }
            }
        }
    }
}