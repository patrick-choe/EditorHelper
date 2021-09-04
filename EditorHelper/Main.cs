using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using EditorHelper.Settings;
using GDMiniJSON;
using HarmonyLib;
using MoreEditorOptions.Util;
using SA.GoogleDoc;
using UnityEngine;
using UnityModManagerNet;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace EditorHelper {
    internal static class Main {
        private static Harmony _harmony;
        private static UnityModManager.ModEntry _mod;
        internal static MainSettings Settings { get; private set; }
        internal static bool FirstLoaded;
        internal static bool IsEnabled;

        internal static bool highlightEnabled;
        
        public static readonly Dictionary<int, char> AngleChar = new Dictionary<int, char> {
            {0, 'R'},
            {15, 'p'},
            {30, 'J'},
            {45, 'E'},
            {60, 'T'},
            {75, 'o'},
            {90, 'U'},
            {105, 'q'},
            {120, 'G'},
            {135, 'Q'},
            {150, 'H'},
            {165, 'W'},
            {180, 'L'},
            {195, 'x'},
            {210, 'N'},
            {225, 'Z'},
            {240, 'F'},
            {255, 'V'},
            {270, 'D'},
            {285, 'Y'},
            {300, 'B'},
            {315, 'C'},
            {330, 'M'},
            {345, 'A'},
        };
        // internal static UnityModManager.ModEntry.ModLogger Logger => _mod?.Logger;

        private static bool Load(UnityModManager.ModEntry modEntry) {
            var version = AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);

            if (version == null || (int) version != 76) {
                return false;
            }

            Settings = UnityModManager.ModSettings.Load<MainSettings>(modEntry);
            Settings.moreEditorSettings_prev = Settings.moreEditorSettings;

            _mod = modEntry;
            _mod.OnToggle = OnToggle;
            _mod.OnGUI = OnGUI;
            _mod.OnSaveGUI = OnSaveGUI; 
            
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

            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            _mod = modEntry;
            IsEnabled = value;

            if (value) {
                StartTweaks();
                ApplyConfig();
            } else {
                StopTweaks();
                ApplyConfig(true);
            }

            return true;
        }

        private static void StartTweaks() {
            _harmony = new Harmony(_mod.Info.Id);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            CheckMoreEditorSettings();
        }

        internal static void CheckMoreEditorSettings() {
            if (Settings.moreEditorSettings) {
                try {
                    GCS.settingsInfo["MiscSettings"].propertiesInfo["useLegacyFlash"] =
                        new PropertyInfo(new Dictionary<string, object> {
                            {"name", "useLegacyFlash"},
                            {"type", "Enum:Toggle"},
                            {"default", "Disabled"}
                        }, GCS.settingsInfo["MiscSettings"]);
                    GCS.settingsInfo["MiscSettings"].propertiesInfo["convertFloorMesh"] =
                        new PropertyInfo(new Dictionary<string, object> {
                            {"name", "convertFloorMesh"},
                            {"type", "Export"}
                        }, GCS.settingsInfo["MiscSettings"]);
                } catch (NullReferenceException) { }
            } else {
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("convertFloorMesh");
            }
        }
        
        private static void StopTweaks() {
            TargetPatch.UntargetFloor();
            _harmony.UnpatchAll(_harmony.Id);
            _harmony = null;
            GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");
            GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("convertFloorMesh");
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry) {
            Settings.Draw(modEntry);
            if (Settings.highlightTargetedTiles && !highlightEnabled) {
                highlightEnabled = true;
                if (scnEditor.instance?.selectedFloors.Count == 1)
                    if (scnEditor.instance.levelEventsPanel.selectedEventType == LevelEventType.MoveTrack ||
                        scnEditor.instance.levelEventsPanel.selectedEventType == LevelEventType.RecolorTrack)
                        TargetPatch.TargetFloor();
            }

            if (!Settings.highlightTargetedTiles && highlightEnabled) {
                highlightEnabled = false;
                if (TargetPatch.targets.Count != 0)
                    TargetPatch.UntargetFloor();
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            Settings.Save(modEntry);

            ApplyConfig();
        }

        private static void ApplyConfig(bool forceRemove = false) {
            if (GCS.settingsInfo == null) {
                return;
            }

            if (!FirstLoaded) {
                FirstLoaded = true;
            }

            if (Settings.RemoveLimits && !forceRemove) {
                foreach (var propertyInfo in GCS.levelEventsInfo.SelectMany(eventPair =>
                    eventPair.Value.propertiesInfo.Select(propertiesPair => propertiesPair.Value))) {
                    switch (propertyInfo.type) {
                        case PropertyType.Color:
                            propertyInfo.color_usesAlpha = true;
                            break;
                        case PropertyType.Int:
                            propertyInfo.int_min = int.MinValue;
                            propertyInfo.int_max = int.MaxValue;
                            break;
                        case PropertyType.Float:
                            propertyInfo.float_min = float.NegativeInfinity;
                            propertyInfo.float_max = float.PositiveInfinity;
                            break;
                        case PropertyType.Vector2:
                            propertyInfo.maxVec = Vector2.positiveInfinity;
                            propertyInfo.minVec = Vector2.negativeInfinity;
                            break;
                    }
                }

                foreach (var propertyInfo in GCS.settingsInfo.SelectMany(eventPair =>
                    eventPair.Value.propertiesInfo.Select(propertiesPair => propertiesPair.Value))) {
                    switch (propertyInfo.type) {
                        case PropertyType.Color:
                            propertyInfo.color_usesAlpha = true;
                            break;
                        case PropertyType.Int:
                            propertyInfo.int_min = int.MinValue;
                            propertyInfo.int_max = int.MaxValue;
                            break;
                        case PropertyType.Float:
                            propertyInfo.float_min = float.NegativeInfinity;
                            propertyInfo.float_max = float.PositiveInfinity;
                            break;
                        case PropertyType.Vector2:
                            propertyInfo.maxVec = Vector2.positiveInfinity;
                            propertyInfo.minVec = Vector2.negativeInfinity;
                            break;
                    }
                }
            } else {
                if (!(Json.Deserialize(Resources.Load<TextAsset>("LevelEditorProperties").text) is
                    Dictionary<string, object> dictionary)) {
                    return;
                }

                var levelEventsInfo = Utils.Decode(dictionary["levelEvents"] as IEnumerable<object>);
                var settingsInfo = Utils.Decode(dictionary["settings"] as IEnumerable<object>);

                foreach (var (key, value) in GCS.levelEventsInfo) {
                    var levelEventInfo = levelEventsInfo[key];

                    foreach (var (property, propertyInfo) in value.propertiesInfo) {
                        var originalPropertyInfo = levelEventInfo.propertiesInfo[property];

                        switch (propertyInfo.type) {
                            case PropertyType.Color:
                                propertyInfo.color_usesAlpha = originalPropertyInfo.color_usesAlpha;
                                break;
                            case PropertyType.Int:
                                propertyInfo.int_min = originalPropertyInfo.int_min;
                                propertyInfo.int_max = originalPropertyInfo.int_max;
                                break;
                            case PropertyType.Float:
                                propertyInfo.float_min = originalPropertyInfo.float_min;
                                propertyInfo.float_max = originalPropertyInfo.float_max;
                                break;
                            case PropertyType.Vector2:
                                propertyInfo.maxVec = originalPropertyInfo.maxVec;
                                propertyInfo.minVec = originalPropertyInfo.minVec;
                                break;
                        }
                    }
                }

                foreach (var (key, value) in GCS.settingsInfo) {
                    var levelEventInfo = settingsInfo[key];

                    foreach (var (property, propertyInfo) in value.propertiesInfo) {
                        var originalPropertyInfo = levelEventInfo.propertiesInfo[property];

                        switch (propertyInfo.type) {
                            case PropertyType.Color:
                                propertyInfo.color_usesAlpha = originalPropertyInfo.color_usesAlpha;
                                break;
                            case PropertyType.Int:
                                propertyInfo.int_min = originalPropertyInfo.int_min;
                                propertyInfo.int_max = originalPropertyInfo.int_max;
                                break;
                            case PropertyType.Float:
                                propertyInfo.float_min = originalPropertyInfo.float_min;
                                propertyInfo.float_max = originalPropertyInfo.float_max;
                                break;
                            case PropertyType.Vector2:
                                propertyInfo.maxVec = originalPropertyInfo.maxVec;
                                propertyInfo.minVec = originalPropertyInfo.minVec;
                                break;
                        }
                    }
                }
            }
        }
    }
}