using System;
using ADOFAI;
using EditorHelper.Core.Patch;
using EditorHelper.Utils;
using HarmonyLib;

namespace EditorHelper.Tweaks.PreserveTileIndex {
    public abstract class PreserveTileIndexPatch {
        [TweakPatchId(nameof(scnEditor), "InsertFloatFloor")]
        public static class FloatCreatePreserveIndexPatch {
            public static void Prefix(int sequenceID) {
                if (scnEditor.instance.get<bool>("lockPathEditing")) return;
                foreach (var evnt in scnEditor.instance.events) {
                    if (evnt.data.GetValueSafe("startTile") is Tuple<int, TileRelativeTo> startTile &&
                        evnt.data.GetValueSafe("endTile") is Tuple<int, TileRelativeTo> endTile) {
                        var (startTileNew, endTileNew) = Misc.CreateShiftedTileIndex(startTile, endTile, sequenceID,
                            evnt.floor, scrLevelMaker.instance.listFloors.Count - 1);
                        evnt.data["startTile"] = startTileNew;
                        evnt.data["endTile"] = endTileNew;
                    }
                }
            }
        }
        
        [TweakPatchId(nameof(scnEditor), "InsertCharFloor")]
        public static class CharCreatePreserveIndexPatch {
            public static void Prefix(int sequenceID) {
                if (scnEditor.instance.get<bool>("lockPathEditing")) return;
                foreach (var evnt in scnEditor.instance.events) {
                    if (evnt.data.GetValueSafe("startTile") is Tuple<int, TileRelativeTo> startTile &&
                        evnt.data.GetValueSafe("endTile") is Tuple<int, TileRelativeTo> endTile) {
                        var (startTileNew, endTileNew) = Misc.CreateShiftedTileIndex(startTile, endTile, sequenceID,
                            evnt.floor, scrLevelMaker.instance.listFloors.Count - 1);
                        evnt.data["startTile"] = startTileNew;
                        evnt.data["endTile"] = endTileNew;
                    }
                }
            }
        }
        
        [TweakPatchId(nameof(scnEditor), "DeleteFloor")]
        public static class DeletePreserveIndexPatch {
            public static void Prefix(int sequenceIndex) {
                DebugUtils.Log("asdsddsfsdafd");
                if (scnEditor.instance.get<bool>("lockPathEditing")) return;
                foreach (var evnt in scnEditor.instance.events) {
                    if (evnt.data.GetValueSafe("startTile") is Tuple<int, TileRelativeTo> startTile &&
                        evnt.data.GetValueSafe("endTile") is Tuple<int, TileRelativeTo> endTile) {
                        var (startTileNew, endTileNew) = Misc.CreateDeletedTileIndex(startTile, endTile, sequenceIndex,
                            evnt.floor, scrLevelMaker.instance.listFloors.Count - 1);
                        evnt.data["startTile"] = startTileNew;
                        evnt.data["endTile"] = endTileNew;
                    }
                }
            }
        }

        [TweakPatchId(nameof(PropertyControl_Tile), "Setup")]
        public static class UpdatePropertiesPatch {
            public static void Prefix(PropertyControl_Tile __instance, bool addListener) {
                __instance.buttonFirstTile.onClick.AddListener(() => {
                    var selectedEvent = scnEditor.instance.levelEventsPanel.selectedEvent;
                    var name = __instance.propertyInfo.name;
                    var tuple = (Tuple<int, TileRelativeTo>) selectedEvent.data[name];
                    selectedEvent.data[name] = Misc.ChangeRelativeTo(tuple.Item1, tuple.Item2, TileRelativeTo.Start,
                        selectedEvent.floor, scrLevelMaker.instance.listFloors.Count - 1);
                    __instance.inputField.text =
                        ((Tuple<int, TileRelativeTo>) selectedEvent.data[name]).Item1.ToString();
                });
                __instance.buttonThisTile.onClick.AddListener(() => {
                    var selectedEvent = scnEditor.instance.levelEventsPanel.selectedEvent;
                    var name = __instance.propertyInfo.name;
                    var tuple = (Tuple<int, TileRelativeTo>) selectedEvent.data[name];
                    selectedEvent.data[name] = Misc.ChangeRelativeTo(tuple.Item1, tuple.Item2, TileRelativeTo.ThisTile,
                        selectedEvent.floor, scrLevelMaker.instance.listFloors.Count - 1);
                    __instance.inputField.text =
                        ((Tuple<int, TileRelativeTo>) selectedEvent.data[name]).Item1.ToString();
                });
                __instance.buttonLastTile.onClick.AddListener(() => {
                    var selectedEvent = scnEditor.instance.levelEventsPanel.selectedEvent;
                    var name = __instance.propertyInfo.name;
                    var tuple = (Tuple<int, TileRelativeTo>) selectedEvent.data[name];
                    selectedEvent.data[name] = Misc.ChangeRelativeTo(tuple.Item1, tuple.Item2, TileRelativeTo.End,
                        selectedEvent.floor, scrLevelMaker.instance.listFloors.Count - 1);
                    __instance.inputField.text =
                        ((Tuple<int, TileRelativeTo>) selectedEvent.data[name]).Item1.ToString();
                });
                return;
            }
        }
    }
}