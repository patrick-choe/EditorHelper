using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using EditorHelper.Core.Patch;
using EditorHelper.Core.Tweaks;
using EditorHelper.Utils;
using UnityEngine;

namespace EditorHelper.Tweaks.RemoveEditorLimits {
    public abstract class RemoveEditorLimitsPatch {
        [TweakPatchId(nameof(scnEditor), "Awake")]
        public static class RemoveEditorLimitsInit {
            public static void Prefix() {
                if (!TweakManager.Setting<RemoveEditorLimitsTweak, RemoveEditorLimitsSetting>()!.RemoveInputValueLimit) return;
                var infos = new[] {GCS.levelEventsInfo, GCS.settingsInfo};
                foreach (var info in infos.SelectMany(info => info.Values)) {
                    #if DEBUG
                    info.pro = false;
                    #endif
                    foreach (var propertyInfo in info.propertiesInfo.Values) {
                        #if DEBUG
                        propertyInfo.pro = false;
                        #endif
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
                            case PropertyType.String:
                                propertyInfo.string_maxLength = int.MaxValue;
                                break;
                        }
                    }
                }
            }
        }

        [TweakPatchId(nameof(scnEditor), "OnSelectedFloorChange")]
        public static class FirstTileEventPatch {
            public static bool Prefix(scnEditor __instance) {
                if (!TweakManager.Setting<RemoveEditorLimitsTweak, RemoveEditorLimitsSetting>()!.EnableFirstFloorEvents) return true;
                
                if (!__instance.SelectionIsEmpty()) {
                    __instance.levelEventsPanel.ShowTabsForFloor(__instance.selectedFloors[0].seqID);
                    __instance.invoke("UpdateFloorDirectionButtons")(true);
                    __instance.invoke("ShowEventPicker")(true);
                    __instance.ShowEventIndicators(__instance.selectedFloors[0]);
                    __instance.HidePopupBlocker();
                    return false;
                }

                return true;
            }
        }

        [TweakPatchId(nameof(scnEditor), "UpdateFloorDirectionButtons")]
        public static class UpdateDirBtnPatch {
            private static bool Prefix(scnEditor __instance, bool active) {
                if (!TweakManager.Setting<RemoveEditorLimitsTweak, RemoveEditorLimitsSetting>()!.EnableFirstFloorEvents) return true;
                
                if (active) {
                    __instance.PreviousFloor(__instance.selectedFloors[0]);
                    double num = __instance.selectedFloors[0].entryangle * 57.295780181884766;
                    float oppositeAngle = Mathf.Abs(450f - (float) num) % 360f;
                    foreach (FloorDirectionButton btn in __instance.floorDirectionButtons) {
                        __instance.invoke("UpdateDirectionButton")(btn, oppositeAngle);
                    }

                    __instance.floorButtonCanvas.transform.position =
                        __instance.selectedFloors[0].transform.position;
                }

                __instance.floorButtonCanvas.gameObject.SetActive(active);

                return false;
            }
        }

        [TweakPatchId(nameof(scnEditor), "AddEvent")]
        internal static class AddEventPatch {
            private static bool Prefix(scnEditor __instance, int floorID, LevelEventType eventType,
                ref bool ___refreshDecSprites) {
                if (!TweakManager.Setting<RemoveEditorLimitsTweak, RemoveEditorLimitsSetting>()!.EnableFirstFloorEvents) return true;
                
                LevelEvent levelEvent = new LevelEvent(floorID, eventType);
                LevelEvent selectedEvent = __instance.levelEventsPanel.selectedEvent;
                if (selectedEvent != null && selectedEvent.data.ContainsKey("angleOffset") &&
                    levelEvent.data.ContainsKey("angleOffset")) {
                    levelEvent["angleOffset"] = selectedEvent["angleOffset"];
                }

                __instance.events.Add(levelEvent);
                if (eventType == LevelEventType.AddDecoration || eventType == LevelEventType.AddText) {
                    ___refreshDecSprites = true;
                }

                return false;
            }
        }

        [TweakPatchId(nameof(scnEditor), "AddEventAtSelected")]
        internal static class AddEventAtSelectedPatch {
            private static bool Prefix(scnEditor __instance, LevelEventType eventType) {
                if (!TweakManager.Setting<RemoveEditorLimitsTweak, RemoveEditorLimitsSetting>()!.EnableFirstFloorEvents) return true;
                
                if (__instance.SelectionIsEmpty()) {
                    return false;
                }

                __instance.SaveState();
                __instance.changingState++;
                var floors = __instance.selectedFloors.Select(f => f.seqID).ToArray();

                foreach (var sequenceID in floors.Except(new[] {floors[0]})) {
                    LevelEvent levelEvent =
                        __instance.events.Find(x => x.eventType == eventType && x.floor == sequenceID);
                    bool flag = Array.Exists(EditorConstants.toggleableTypes, element => element == eventType);
                    if (levelEvent != null &&
                        Array.Exists(EditorConstants.soloTypes, element => element == eventType) && !flag) {
                        __instance.changingState--;
                        return false;
                    }

                    if (flag && levelEvent != null) {
                        __instance.RemoveEvent(levelEvent);
                        __instance.DecideInspectorTabsAtSelected();
                    } else {
                        __instance.AddEvent(sequenceID, eventType);
                        __instance.levelEventsPanel.selectedEventType = eventType;
                    }

                }

                var seqID = __instance.selectedFloors[0].seqID;

                LevelEvent levelEvent2 =
                    __instance.events.Find(x => x.eventType == eventType && x.floor == seqID);
                bool flag2 = Array.Exists(EditorConstants.toggleableTypes, element => element == eventType);
                if (levelEvent2 != null &&
                    Array.Exists(EditorConstants.soloTypes, element => element == eventType) && !flag2) {
                    __instance.changingState--;
                    return false;
                }

                if (flag2 && levelEvent2 != null) {
                    __instance.RemoveEvent(levelEvent2);
                    __instance.DecideInspectorTabsAtSelected();
                } else {
                    __instance.AddEvent(seqID, eventType);
                    __instance.levelEventsPanel.selectedEventType = eventType;

                    int count = __instance.events
                        .FindAll(x => x.eventType == eventType && x.floor == seqID).Count;
                    if (count == 1) {
                        __instance.DecideInspectorTabsAtSelected();
                        __instance.levelEventsPanel.ShowPanel(eventType);
                    } else {
                        __instance.levelEventsPanel.ShowPanel(eventType, count - 1);
                    }
                }

                __instance.ApplyEventsToFloors();
                __instance.ShowEventIndicators(__instance.selectedFloors[0]);
                __instance.changingState--;

                return false;
            }
        }

        [TweakPatchId(nameof(scnEditor), "Update")]
        public static class UpdateAddEventPatch {
            public static void Postfix(scnEditor __instance) {
                if (!TweakManager.Setting<RemoveEditorLimitsTweak, RemoveEditorLimitsSetting>()!.EnableFirstFloorEvents) return;
                
                if (__instance.get<bool>("userIsEditingAnInputField") || __instance.get<bool>("showingPopup")) return;
                if (!__instance.SelectionIsSingle() && !__instance.SelectionIsEmpty()) {
                    string inputString = Input.inputString;
                    if (inputString != null && inputString.Length == 1 && inputString[0] >= '1' && inputString[0] <= '9') {
                        int num = inputString[0] - '0';
                        __instance.printe("number: " + num);
                        foreach (var levelEventButton in __instance.get<Dictionary<LevelEventCategory, List<LevelEventButton>>>("eventButtons")![__instance.get<LevelEventCategory>("currentCategory")]) {
                            if (levelEventButton.keyCode == num && levelEventButton.page == __instance.get<int>("currentPage")) {
                                __instance.AddEventAtSelected(levelEventButton.type);
                                break;
                            }
                        }

                        return;
                    }
                }
            }
        }
    }
}