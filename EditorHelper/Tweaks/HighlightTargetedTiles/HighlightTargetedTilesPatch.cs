using System;
using ADOFAI;
using DG.Tweening;
using EditorHelper.Core.Patch;
using EditorHelper.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Tweaks.HighlightTargetedTiles {
    public abstract class HighlightTargetedTilesPatch {

        [TweakPatchId(nameof(scnEditor), "DeselectFloors")]
        public static class DeselectPatch {
            public static void Postfix() {
                DOTween.Kill("EHselectedColorEventTween", false);
                DOTween.Kill("selectedColorTween", false);
            }
        }
        
        [TweakPatchId(nameof(scnEditor), "SelectFloor")]
        public static class SelectPatch {
            public static void Prefix() {
                DOTween.Kill("EHselectedColorEventTween", true);
            }
        }

        [TweakPatchId(nameof(InspectorPanel), "ShowPanel")]
        public static class UpdateCurrentSelectedTiles {
            public static void Postfix(InspectorPanel __instance) {
                DOTween.Kill("selectedColorTween", false);
                if (__instance.get<bool>("showingPanel")) UpdateSelectedEvent(__instance.selectedEvent);
            }
        }

        [TweakPatchId(nameof(PropertyControl_Tile), "Setup")]
        public static class UpdateOnEventUpdate {
            public static void Postfix(PropertyControl_Tile __instance) {
                __instance.buttonFirstTile.onClick ??= new Button.ButtonClickedEvent();
                __instance.buttonThisTile.onClick ??= new Button.ButtonClickedEvent();
                __instance.buttonLastTile.onClick ??= new Button.ButtonClickedEvent();
                
                __instance.buttonFirstTile.onClick.AddListener(() => UpdateSelectedEvent(scnEditor.instance.levelEventsPanel.selectedEvent));
                __instance.buttonThisTile.onClick.AddListener(() => UpdateSelectedEvent(scnEditor.instance.levelEventsPanel.selectedEvent));
                __instance.buttonLastTile.onClick.AddListener(() => UpdateSelectedEvent(scnEditor.instance.levelEventsPanel.selectedEvent));
                __instance.inputField.get<UnityEvent<string>>("onEndEdit")!.AddListener(_ => UpdateSelectedEvent(scnEditor.instance.levelEventsPanel.selectedEvent));
                
            }
        }

        public static void UpdateSelectedEvent(LevelEvent? evnt) {
            DOTween.Kill("EHselectedColorEventTween");
            if (evnt?.eventType is not LevelEventType.MoveTrack or LevelEventType.RecolorTrack) return;
            foreach (var floor in scnEditor.instance.selectedFloors) {
                floor.SetColor(floor.floorRenderer.deselectedColor);
            }
            
            var startTileOff = evnt.data["startTile"] as Tuple<int, TileRelativeTo>;
            var endTileOff = evnt.data["endTile"] as Tuple<int, TileRelativeTo>;
            var length = scrLevelMaker.instance.listFloors.Count;
            int startTile = Misc.GetAbsoluteSeqId(startTileOff!.Item1, startTileOff.Item2, evnt.floor, length - 1);
            int endTile = Misc.GetAbsoluteSeqId(endTileOff!.Item1, endTileOff.Item2, evnt.floor, length - 1);

            if (startTile < 0) startTile = 0;
            if (endTile < 0) endTile = 0;
            if (startTile >= length) startTile = length - 1;
            if (endTile >= length) endTile = length - 1;
            if (startTile > endTile) {
                (startTile, endTile) = (endTile, startTile);
            }

            UnityModManager.Logger.Log($"{(startTile, endTile)}");

            var currFloor = scrLevelMaker.instance.listFloors[startTile];
            while (currFloor.seqID <= endTile) {
                UpdateColorFloor(currFloor, Color.blue);
                currFloor = currFloor.nextfloor;
            }
        }

        public static void UpdateColorFloor(scrFloor floor, Color color) {
            var track = floor.GetComponent<ffxChangeTrack>();
            DOTween.To(() => 0f, delegate(float x) { floor.floorRenderer.color = Color.Lerp(floor.floorRenderer.deselectedColor, color, x); },
                    0.5f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true)
                .SetId($"EHselectedColorEventTween")
                .onKill += () => floor.SetColor(floor.floorRenderer.deselectedColor);
        }
    }
}