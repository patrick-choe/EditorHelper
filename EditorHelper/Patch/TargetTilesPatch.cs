using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Patch {
    [HarmonyPatch(typeof(PropertyControl_Tile), "Setup")]
    public static class UpdatePropertiesPatch {
        public static bool Prefix(PropertyControl_Tile __instance, bool addListener) {
            __instance.inputField.propertyInfo = __instance.propertyInfo;
            __instance.inputField.propertiesPanel = __instance.propertiesPanel;
            __instance.buttonsToggle.propertyInfo = __instance.propertyInfo;
            __instance.buttonsToggle.propertiesPanel = __instance.propertiesPanel;
            __instance.inputField.Setup(addListener);
            List<string> list = new List<string>();
            foreach (object obj in Enum.GetValues(typeof(TileRelativeTo))) {
                list.Add(((TileRelativeTo) obj).ToString());
            }

            List<Button> list2 = new List<Button>();
            list2.Add(__instance.buttonThisTile);
            list2.Add(__instance.buttonFirstTile);
            list2.Add(__instance.buttonLastTile);
            Dictionary<string, Button> dictionary = new Dictionary<string, Button>();

            if (Main.Settings.ChangeIndexWhenToggle) {
                __instance.buttonFirstTile.onClick.AddListener(() => {
                    if (!Main.Settings.ChangeIndexWhenToggle) return;
                    UnityModManager.Logger.Log("Start");
                    var selectedEvent = scnEditor.instance.levelEventsPanel.selectedEvent;
                    var name = __instance.propertyInfo.name;
                    var tuple = selectedEvent.data[name] as Tuple<int, TileRelativeTo>;
                    selectedEvent.data[name] = new Tuple<int, TileRelativeTo>(
                        ChangeIndex(tuple.Item1, selectedEvent.floor, tuple.Item2, TileRelativeTo.Start),
                        TileRelativeTo.Start
                    );
                    UnityModManager.Logger.Log($"{tuple.Item1}");
                    UnityModManager.Logger.Log(
                        $"{ChangeIndex(tuple.Item1, selectedEvent.floor, tuple.Item2, TileRelativeTo.Start)}");
                });
                __instance.buttonLastTile.onClick.AddListener(() => {
                    if (!Main.Settings.ChangeIndexWhenToggle) return;
                    UnityModManager.Logger.Log("End");
                    var selectedEvent = scnEditor.instance.levelEventsPanel.selectedEvent;
                    var name = __instance.propertyInfo.name;
                    var tuple = selectedEvent.data[name] as Tuple<int, TileRelativeTo>;
                    selectedEvent.data[name] = new Tuple<int, TileRelativeTo>(
                        ChangeIndex(tuple.Item1, selectedEvent.floor, tuple.Item2, TileRelativeTo.End),
                        TileRelativeTo.End
                    );
                });
                __instance.buttonThisTile.onClick.AddListener(() => {
                    if (!Main.Settings.ChangeIndexWhenToggle) return;
                    UnityModManager.Logger.Log("ThisTile");
                    var selectedEvent = scnEditor.instance.levelEventsPanel.selectedEvent;
                    var name = __instance.propertyInfo.name;
                    var tuple = selectedEvent.data[name] as Tuple<int, TileRelativeTo>;
                    selectedEvent.data[name] = new Tuple<int, TileRelativeTo>(
                        ChangeIndex(tuple.Item1, selectedEvent.floor, tuple.Item2, TileRelativeTo.ThisTile),
                        TileRelativeTo.ThisTile
                    );
                });

                for (int i = 0; i < 3; i++) {
                    string enumVal = list[i];
                    Button button = list2[i];
                    dictionary.Add(enumVal, button);
                    button.GetComponentInChildren<Text>().text = RDString.Get("enum.TileRelativeTo." + list[i]);
                    button.GetComponent<Button>().onClick.AddListener(delegate() {
                        __instance.buttonsToggle.SelectVar(enumVal);
                        var tuple = scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] as Tuple<int, TileRelativeTo>;
                        __instance.inputField.text = tuple.Item1.ToString();
                    });
                }

                if (Main.Settings.HighlightTargetedTiles) {
                    __instance.buttonFirstTile.onClick.AddListener(() =>
                        UpdateCurrentSelectedTiles.Postfix(scnEditor.instance.levelEventsPanel));
                    __instance.buttonLastTile.onClick.AddListener(() =>
                        UpdateCurrentSelectedTiles.Postfix(scnEditor.instance.levelEventsPanel));
                    __instance.buttonLastTile.onClick.AddListener(() =>
                        UpdateCurrentSelectedTiles.Postfix(scnEditor.instance.levelEventsPanel));
                    __instance.inputField.onEndEdit.AddListener((val) =>
                        UpdateCurrentSelectedTiles.Postfix(scnEditor.instance.levelEventsPanel));
                }
            }

            __instance.buttonsToggle.buttons = dictionary;
            return false;
        }
        public static int ChangeIndex(int orig, int seqID, TileRelativeTo from, TileRelativeTo to) {
            int start;
            switch (from) {
                case TileRelativeTo.ThisTile:
                    start = orig + seqID;
                    break;
                case TileRelativeTo.End:
                    start = orig + scrLevelMaker.instance.listFloors.Count - 1;
                    break;
                default:
                    start = orig;
                    break;
            }

            int result;
            switch (to) {
                case TileRelativeTo.ThisTile:
                    result = start - seqID;
                    break;
                case TileRelativeTo.End:
                    result = start - scrLevelMaker.instance.listFloors.Count + 1;
                    break;
                default:
                    result = start;
                    break;
            }

            return result;
        }
        public static T AddListenerAtFirst<T>(this T clickedEvent, UnityAction call) where T : UnityEvent, new() {
            var result = new T();
            result.AddListener(call);
            result.AddListener(clickedEvent.Invoke);
            return result;
        }
    }

    [HarmonyPatch(typeof(scnEditor), "CreateFloorWithCharOrAngle")]
    public static class ShiftEventCreatedFloorPatch {
        public static void Postfix() {
            if (!Main.Settings.ChangeIndexWhenCreateTile) return;
            var seqID = scnEditor.instance.selectedFloors[0].seqID;
            foreach (var evnt in scnEditor.instance.events) {
                if (evnt.eventType is LevelEventType.MoveTrack or LevelEventType.RecolorTrack) {
                    var tuple = evnt.data["startTile"] as Tuple<int, TileRelativeTo>;
                    if (tuple.Item2 == TileRelativeTo.Start && evnt.floor > seqID) {
                        evnt.data["startTile"] = new Tuple<int, TileRelativeTo>(tuple.Item1 + 1, tuple.Item2);
                    }
                    if (tuple.Item2 == TileRelativeTo.End && evnt.floor < seqID) {
                        evnt.data["startTile"] = new Tuple<int, TileRelativeTo>(tuple.Item1 - 1, tuple.Item2);
                    }
                    tuple = evnt.data["endTile"] as Tuple<int, TileRelativeTo>;
                    if (tuple.Item2 == TileRelativeTo.Start && evnt.floor > seqID) {
                        evnt.data["endTile"] = new Tuple<int, TileRelativeTo>(tuple.Item1 + 1, tuple.Item2);
                    }
                    if (tuple.Item2 == TileRelativeTo.End && evnt.floor < seqID) {
                        evnt.data["endTile"] = new Tuple<int, TileRelativeTo>(tuple.Item1 - 1, tuple.Item2);
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(scnEditor), "DeleteFloor")]
    public static class ShiftEventDeletedFloorPatch {
        public static void Postfix() {
            if (!Main.Settings.ChangeIndexWhenCreateTile) return;
            var seqID = scnEditor.instance.selectedFloors[0].seqID;
            foreach (var evnt in scnEditor.instance.events) {
                if (evnt.eventType is LevelEventType.MoveTrack or LevelEventType.RecolorTrack) {
                    var tuple = evnt.data["startTile"] as Tuple<int, TileRelativeTo>;
                    if (tuple.Item2 == TileRelativeTo.Start && evnt.floor > seqID) {
                        evnt.data["startTile"] = new Tuple<int, TileRelativeTo>(tuple.Item1 - 2, tuple.Item2);
                    }
                    if (tuple.Item2 == TileRelativeTo.End && evnt.floor < seqID) {
                        evnt.data["startTile"] = new Tuple<int, TileRelativeTo>(tuple.Item1 + 2, tuple.Item2);
                    }
                    tuple = evnt.data["endTile"] as Tuple<int, TileRelativeTo>;
                    if (tuple.Item2 == TileRelativeTo.Start && evnt.floor > seqID) {
                        evnt.data["endTile"] = new Tuple<int, TileRelativeTo>(tuple.Item1 - 2, tuple.Item2);
                    }
                    if (tuple.Item2 == TileRelativeTo.End && evnt.floor < seqID) {
                        evnt.data["endTile"] = new Tuple<int, TileRelativeTo>(tuple.Item1 + 2, tuple.Item2);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(scnEditor), "DeselectFloors")]
    public static class DeselectPatch {
        public static void Prefix() {
            foreach (var floor in scnEditor.instance.selectedFloors) {
                var track = floor.GetComponent<ffxChangeTrack>();
                if (track == null)
                    floor.floorRenderer.deselectedColor = floor.floorRenderer.color;
                else
                    floor.floorRenderer.deselectedColor =
                        track.colorType == TrackColorType.Rainbow ? Color.white : track.color1;
            }
        }
    }

    [HarmonyPatch(typeof(InspectorPanel), "ShowPanel")]
    public static class UpdateCurrentSelectedTiles {
        public static void Postfix(InspectorPanel __instance) {
            if (!Main.Settings.HighlightTargetedTiles) return;
            DOTween.Kill("selectedColorTween", false);
            DOTween.Kill($"EHselectedColorEventTween");
            switch (__instance.selectedEventType) {
                case LevelEventType.MoveTrack:
                case LevelEventType.RecolorTrack:
                    goto UpdateSelectedColor;
                default:
                    return;
            }

            UpdateSelectedColor:
            var startTileOff = __instance.selectedEvent.data["startTile"] as Tuple<int, TileRelativeTo>;
            var endTileOff = __instance.selectedEvent.data["endTile"] as Tuple<int, TileRelativeTo>;
            int startTile;
            var length = scrLevelMaker.instance.listFloors.Count;
            switch (startTileOff.Item2) {
                case TileRelativeTo.ThisTile:
                    startTile = startTileOff.Item1 + __instance.selectedEvent.floor;
                    break;
                case TileRelativeTo.End:
                    startTile = startTileOff.Item1 + length - 1;
                    break;
                default:
                    startTile = startTileOff.Item1;
                    break;
            }

            int endTile;
            switch (endTileOff.Item2) {
                case TileRelativeTo.ThisTile:
                    endTile = endTileOff.Item1 + __instance.selectedEvent.floor;
                    break;
                case TileRelativeTo.End:
                    endTile = endTileOff.Item1 + length - 1;
                    break;
                default:
                    endTile = endTileOff.Item1;
                    break;
            }

            if (startTile < 0) startTile = 0;
            if (endTile < 0) endTile = 0;
            if (startTile >= length) startTile = length - 1;
            if (endTile >= length) endTile = length - 1;
            if (startTile > endTile) {
                (startTile, endTile) = (endTile, startTile);
            }

            UnityModManager.Logger.Log($"{(startTile, endTile)}");

            var currFloor = scrLevelMaker.instance.listFloors[startTile];
            UpdateColorFloor(currFloor, Color.blue);
            while (currFloor.seqID < endTile) {
                currFloor = currFloor.nextfloor;
                UpdateColorFloor(currFloor, Color.blue);
            }
        }

        public static void UpdateColorFloor(scrFloor floor, Color color) {
            var track = floor.GetComponent<ffxChangeTrack>();
            var colorOrig = track == null ? floor.floorRenderer.color :
                track.colorType == TrackColorType.Rainbow ? Color.white : track.color1;
            DOTween.To(() => 0f, delegate(float x) { floor.floorRenderer.color = Color.Lerp(colorOrig, color, x); },
                    0.5f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true)
                .SetId($"EHselectedColorEventTween")
                .onKill += () => floor.floorRenderer.color = colorOrig;
        }
    }
}