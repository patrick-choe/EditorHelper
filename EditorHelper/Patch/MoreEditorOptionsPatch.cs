using System;
using System.Collections.Generic;
using ADOFAI;
using EditorHelper.Components;
using HarmonyLib;
using UnityEngine;

namespace EditorHelper.Patch {

    public class Patch {
        [HarmonyPatch(typeof(ADOStartup), "SetupLevelEventsInfo")]
        public static class SetupPatch {
            public static void Postfix() {
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
            }
        }

        //[HarmonyPatch(typeof(PropertiesPanel), "SetProperties")]
        public static class InspectorSettingsPatch {
            public static bool Prefix(PropertiesPanel __instance, LevelEvent levelEvent,
                bool checkIfEnabled = true) {
                foreach (string text in levelEvent.data.Keys) {
                    if (__instance.properties.ContainsKey(text)) {
                        var control = __instance.properties[text].control;
                        if (!(control == null) && control.propertyInfo.type != PropertyType.Export) {
                            if (control.propertyInfo.type == PropertyType.Vector2) {
                                control.text = ((Vector2) levelEvent[text]).ToString("f6");
                                if (control.propertyInfo.hasRandomValue) {
                                    control.randomControl.text =
                                        ((Vector2) levelEvent[control.propertyInfo.randValueKey]).ToString("f6");
                                    control.SetRandomLayout();
                                }
                            } else {
                                if (levelEvent.data.ContainsKey(text))
                                    control.text = levelEvent[text].ToString();
                                else
                                    control.text = "True";
                                if (control.propertyInfo.hasRandomValue) {
                                    control.randomControl.text =
                                        levelEvent[control.propertyInfo.randValueKey].ToString();
                                    control.SetRandomLayout();
                                }
                            }

                            if (checkIfEnabled) {
                                control.ToggleOthersEnabled();
                            }
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Export), "Awake")]
        public static class PropertyExportPatch {
            public static void Postfix(PropertyControl_Export __instance) {
                __instance.gameObject.AddComponent<FloorMeshConverter>().Control = __instance;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Toggle), "Update")]
        public static class PropertyUpdatePatch {
            public static void Postfix(PropertyControl_Toggle __instance) {
                if (CustomLevel.instance == null) return;
                if (__instance.propertyInfo.name == "useLegacyFlash") {
                    __instance.selected = CustomLevel.instance.levelData.legacyFlash ? "Enabled" : "Disabled";
                }
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Toggle), "SelectVar")]
        public static class PropertySetPatch {
            public static bool Prefix(PropertyControl_Toggle __instance, string var) {
                if (__instance.settingText) {
                    return false;
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
    }
}