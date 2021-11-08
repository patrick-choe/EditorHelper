using System.Collections.Generic;
using EditorHelper.Core.Initializer;
using EditorHelper.Core.Patch;
using SA.GoogleDoc;

namespace EditorHelper.Utils {
    public static class PatchRDString {
        public static readonly Dictionary<string, Dictionary<LangCode, string>> Translations = new();
        
        [TweakPatchId(nameof(RDString), "GetWithCheck")]
        public static class RDStringPatch {
            public static bool Prefix(string key, out bool exists, ref string __result) {
                RDString.Setup();
                exists = false;
                var lang = Localization.CurrentLanguage;
                if (Translations.TryGetValue(key, out var val1)) {
                    if (val1.TryGetValue(lang, out var val2)) {
                        exists = true;
                        __result = val2;
                        return false;
                    }
                    if (val1.TryGetValue(LangCode.English, out var val4)) {
                        exists = true;
                        __result = val4;
                        return false;
                    }
                }
                
                string result = "";
                string token = key + ".mobile";
                string token2 = key + ".nx";
                if (ADOBase.isSwitch && Localization.ExistsLocalizedString(token2)) {
                    result = Localization.GetLocalizedString(token2);
                    exists = true;
                } else if (ADOBase.isMobile && Localization.ExistsLocalizedString(token)) {
                    result = Localization.GetLocalizedString(token);
                    exists = true;
                } else if (Localization.ExistsLocalizedString(key)) {
                    result = Localization.GetLocalizedString(key);
                    exists = true;
                }

                __result = result;
                return false;
            }

            [Init] public static void Patch() {
                Main.Harmony!.TweakPatch(typeof(RDStringPatch));
            }
        }
    }
}