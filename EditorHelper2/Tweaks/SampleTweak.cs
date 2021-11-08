using System.Collections.Generic;
using ADOFAI;
using EditorHelper.Core.Patch;
using EditorHelper.Core.Tweaks;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityModManagerNet;

namespace EditorHelper.Tweaks {
    [RegisterTweak]
    public class SampleTweak : Tweak, IPatchClass<SamplePatch> {
        public override void OnRegister(bool enabled) {
            base.OnRegister(enabled);
            
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
                {LangCode.English, "Use Legacy Tiles"},
            };
            PatchRDString.Translations["editor.EditorHelperEventBundles"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "<size=20>EditorHelper 이벤트 번들</size>"},
                {LangCode.English, "<size=20>EditorHelper Event Bundles</size>"},
            };

            PatchRDString.Translations["editor.EditorHelperAssetPacks"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "<size=20>EditorHelper 에셋 팩</size>"},
                {LangCode.English, "<size=20>EditorHelper Asset Packs</size>"},
            };
        }

        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyFlash"))
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");
            if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyTiles"))
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyTiles");
            UnpatchTweak();
        }
    }
}