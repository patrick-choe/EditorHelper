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
            PatchRDString.Translations["editor.EH:useLegacyFlash"] = new Dictionary<LangCode, string> {
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
            PatchRDString.Translations["editor.EH:useLegacyTiles"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "레거시 타일 사용"},
                {LangCode.English, "Use legacy tiles"},
            };

            PatchRDString.Translations["editor.EH:lockToCameraPos"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "화면 위치에 고정"},
                {LangCode.English, "Lock to screen position"},
            };

            PatchRDString.Translations["editor.EH:lockToCameraRot"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "화면 회전에 고정"},
                {LangCode.English, "Lock to screen rotation"},
            };

            PatchRDString.Translations["editor.EH:lockToCameraScale"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "화면 크기에 고정"},
                {LangCode.English, "Lock to screen scale"},
            };

            PatchRDString.Translations["editor.EH:disableIfMinimumVFX"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "시각 효과 '낮음' 시 비활성화"},
                {LangCode.English, "Disable if minimum visual effects"},
            };
            
            base.OnRegister(enabled);
        }

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
            
            if (GCS.levelEventsInfo["AddDecoration"].propertiesInfo.ContainsKey("EH:lockToCameraPos"))
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Remove("EH:lockToCameraPos");
                
            if (GCS.levelEventsInfo["AddDecoration"].propertiesInfo.ContainsKey("EH:lockToCameraRot"))
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Remove("EH:lockToCameraRot");
                
            if (GCS.levelEventsInfo["AddDecoration"].propertiesInfo.ContainsKey("EH:lockToCameraScale"))
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Remove("EH:lockToCameraScale");
            
            if (GCS.levelEventsInfo["AddDecoration"].propertiesInfo.ContainsKey("EH:disableIfMinimumVFX"))
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Remove("EH:disableIfMinimumVFX");
        }
    }
}