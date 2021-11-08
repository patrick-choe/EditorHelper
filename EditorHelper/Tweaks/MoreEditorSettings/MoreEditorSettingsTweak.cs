using System.Collections.Generic;
using EditorHelper.Core.Tweaks;
using EditorHelper.Utils;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.MoreEditorSettings {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "More Editor Settings")]
    [TweakDescription(LangCode.Korean,  "더 많은 에디터 설정")]
    public class MoreEditorSettingsTweak : Tweak, IPatchClass<MoreEditorSettingsPatch> {
        public override void OnRegister(bool enabled) {
            PatchRDString.Translations["editor.useLegacyFlash"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "기존 플래시 사용"},
                {LangCode.English, "Use legacy flash"},
            };
            PatchRDString.Translations["editor.convertFloorMesh"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "레벨 변환"},
                {LangCode.English, "Convert level"},
            };
            PatchRDString.Translations["editor.convertFloorMesh.toLegacy"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "기존 타일로 변환"},
                {LangCode.English, "Convert to legacy tiles"},
            };
            PatchRDString.Translations["editor.convertFloorMesh.toMesh"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "자유 각도로 변환"},
                {LangCode.English, "Convert to mesh models"},
            };
            PatchRDString.Translations["editor.useLegacyTiles"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "레거시 타일 사용"},
                {LangCode.English, "Use legacy tiles"},
            };
            PatchRDString.Translations["editor.EditorHelperEventBundles"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "<size=20>EditorHelper 이벤트 번들</size>"},
                {LangCode.English, "<size=20>EditorHelper Event Bundles</size>"},
            };

            PatchRDString.Translations["editor.EditorHelperAssetPacks"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "<size=20>EditorHelper 에셋 팩</size>"},
                {LangCode.English, "<size=20>EditorHelper Asset Packs</size>"},
            };
            
            base.OnRegister(enabled);
        }

        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
            if (GCS.settingsInfo == null) return;
            if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyFlash"))
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");
            if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyTiles"))
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyTiles");
        }
    }
}