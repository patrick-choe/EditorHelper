using System;
using System.Collections.Generic;
using ADOFAI;
using EditorHelper.Core.Components;
using EditorHelper.Core.Patch;
using HarmonyLib;

namespace EditorHelper.Tweaks {
    public abstract class SamplePatch {
        [TweakPatch("PropertyExportPatch", nameof(PropertyControl_Export), "Awake")]
        public static class PropertyExportPatch {
            public static void Postfix(PropertyControl_Export __instance) {
                __instance.gameObject.AddComponent<FloorMeshConverter>().Control = __instance;
            }
        }

        [TweakPatch("PropertyUpdatePatch" , nameof(PropertyControl_Toggle), "Update")]
        public static class PropertyUpdatePatch {
            public static void Postfix(PropertyControl_Toggle __instance) {
                if (CustomLevel.instance == null) return;
                if (__instance.propertyInfo.name == "useLegacyFlash") {
                    __instance.selected = CustomLevel.instance.levelData.legacyFlash ? "Enabled" : "Disabled";
                }
                if (__instance.propertyInfo.name == "useLegacyTiles") {
                    __instance.selected = CustomLevel.instance.levelData.isOldLevel ? "Enabled" : "Disabled";
                }
            }
        }

        [TweakPatch("PropertySetPatch" , nameof(PropertyControl_Toggle), "SelectVar")]
        public static class PropertySetPatch {
            public static bool Prefix(PropertyControl_Toggle __instance, string var) {
                if (__instance.settingText) {
                    return false;
                }
                
                if (__instance.propertyInfo.name == "useLegacyTiles") {
                    if (!FloorMeshConverter.Convert()) {
                        return false;
                    }
                }

                __instance.editor.SaveState();
                __instance.editor.changingState++;
                if (__instance.buttons != null && __instance.buttons.Count > 0) {
                    foreach (string text in __instance.buttons.Keys) {
                        __instance.SetButtonEnabled(__instance.buttons[text], text == var);
                    }
                }

                var selectedEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
                __instance.selected = var;
                var enumType = __instance.propertyInfo.enumType;
                if (__instance.propertyInfo.type == PropertyType.Tile) {
                    var tuple =
                        selectedEvent[__instance.propertyInfo.name] as Tuple<int, TileRelativeTo>;
                    selectedEvent[__instance.propertyInfo.name] =
                        new Tuple<int, TileRelativeTo>(tuple.Item1, (TileRelativeTo) Enum.Parse(enumType, var));
                } else {
                    selectedEvent[__instance.propertyInfo.name] = Enum.Parse(enumType, var);
                }

                if (__instance.propertyInfo.name == "useLegacyFlash") {
                    CustomLevel.instance.levelData.legacyFlash = __instance.selected == "Enabled";
                }

                __instance.ToggleOthersEnabled();
                if (selectedEvent.eventType == LevelEventType.BackgroundSettings) {
                    __instance.customLevel.SetBackground();
                } else if (selectedEvent.eventType == LevelEventType.AddDecoration ||
                           selectedEvent.eventType == LevelEventType.AddText) {
                    __instance.editor.UpdateDecorationSprites();
                }

                __instance.editor.ApplyEventsToFloors();
                __instance.editor.changingState--;

                return false;
            }
        }
        
        [TweakPatch("MoreEditorSettingsInit", nameof(scnEditor), "Awake")]
        public static class MoreEditorSettingsInit {
            public static void Prefix() {
                if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyFlash"))
                    GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");
                if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyTiles"))
                    GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyTiles");

                GCS.settingsInfo["MiscSettings"].propertiesInfo.Add("useLegacyFlash",
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "useLegacyFlash"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.settingsInfo["MiscSettings"]));
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Add("useLegacyTiles",
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "useLegacyTiles"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.settingsInfo["MiscSettings"]));
            }
        }
    }
}