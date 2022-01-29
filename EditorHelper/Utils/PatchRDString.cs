using System.Collections.Generic;
using EditorHelper.Core.Initializer;
using EditorHelper.Core.Patch;
using SA.GoogleDoc;

namespace EditorHelper.Utils {
    public static class PatchRDString {
        public static readonly Dictionary<string, Dictionary<LangCode, string>> Translations = new();
        
        [TweakPatch(nameof(RDString), "GetWithCheck")]
        public static class RDStringPatch {
            public static bool Prefix(string key, out bool exists, Dictionary<string, object> parameters, out string __result) {
                RDString.Setup();
                exists = false;
                string text = "";
                string token = key + ".mobile";
                string token2 = key + ".nx";
                if (ADOBase.isSwitch && ExistsLocalizedString(token2)) {
                    text = GetLocalizedString(token2);
                    exists = true;
                } else if (ADOBase.isMobile && ExistsLocalizedString(token)) {
                    text = GetLocalizedString(token);
                    exists = true;
                } else if (ExistsLocalizedString(key)) {
                    text = GetLocalizedString(key);
                    exists = true;
                }

                if (text == "null") {
                    text = Localization.GetLocalizedString(key, LangSection.Translations, LangCode.English);
                    exists = true;
                }

                if (exists) {
                    text = RDString.ReplaceParameters(text, parameters);
                }

                __result = text;
                return false;
            }
            
            public static bool ExistsLocalizedString(string token) => Localization.ExistsLocalizedString(token) || Translations.ContainsKey(token);
            public static string GetLocalizedString(string token) =>
                !Translations.TryGetValue(token, out var translations) ? Localization.GetLocalizedString(token) :
                translations.TryGetValue(Localization.CurrentLanguage, out var translation) ? translation :
                translations.TryGetValue(LangCode.English, out var english) ? english :
                Localization.GetLocalizedString(token);

            [Init] public static void Patch() {
                Main.Harmony!.TweakPatch(typeof(RDStringPatch));
            }
        }
    }
}