using ADOFAI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EditorHelper
{
    public static class TargetPatch
    {

        public static List<scrFloor> targets = new List<scrFloor>();
        public static Dictionary<PropertyControl_Tile, TileRelativeTo> lastTileRelativeTo = new Dictionary<PropertyControl_Tile, TileRelativeTo>();

        public static void AfterShowPanel(LevelEventType eventType)
        {
            if (!Main.Settings.HighlightTargetedTiles)
                return;
            if (eventType == LevelEventType.MoveTrack || eventType == LevelEventType.RecolorTrack)
            {
                TargetFloor();
            }
            else
            {
                UntargetFloor();
            }
        }

        public static bool BeforeSelectFloor(scrFloor floorToSelect)
        {
            if (Main.Settings.SelectTileWithShortcutKeys)
            {
                if (scnEditor.instance.levelEventsPanel != null)
                {
                    LevelEventType type = scnEditor.instance.levelEventsPanel.selectedEventType;
                    if (type == LevelEventType.MoveTrack || type == LevelEventType.RecolorTrack)
                        if (Input.GetKey(KeyCode.Semicolon))
                        {
                            string text = "copyFlashTween" + floorToSelect.seqID;
                            DOTween.Kill(text, false);
                            floorToSelect.floorRenderer.SetFlash(1f);
                            floorToSelect.floorRenderer.TweenFlash(0f, 0.3f, text);

                            int floor = scnEditor.instance.selectedFloors[0].seqID;
                            Tuple<int, TileRelativeTo> tuple = scnEditor.instance.levelEventsPanel.selectedEvent.data["startTile"] as Tuple<int, TileRelativeTo>;
                            switch (tuple.Item2)
                            {
                                case TileRelativeTo.ThisTile:
                                    scnEditor.instance.levelEventsPanel.selectedEvent.data["startTile"] =
                                        Tuple.Create(floorToSelect.seqID - floor, tuple.Item2);
                                    break;
                                case TileRelativeTo.Start:
                                    scnEditor.instance.levelEventsPanel.selectedEvent.data["startTile"] =
                                        Tuple.Create(floorToSelect.seqID, tuple.Item2);
                                    break;
                                case TileRelativeTo.End:
                                    scnEditor.instance.levelEventsPanel.selectedEvent.data["startTile"] =
                                        Tuple.Create(1 + floorToSelect.seqID - scnEditor.instance.customLevel.levelMaker.listFloors.Count, tuple.Item2);
                                    break;
                            }
                            scnEditor.instance.levelEventsPanel.ShowPanelOfEvent(scnEditor.instance.levelEventsPanel.selectedEvent);
                            return false;
                        }
                        else if (Input.GetKey(KeyCode.Quote))
                        {
                            string text = "copyFlashTween" + floorToSelect.seqID;
                            DOTween.Kill(text, false);
                            floorToSelect.floorRenderer.SetFlash(1f);
                            floorToSelect.floorRenderer.TweenFlash(0f, 0.3f, text);

                            int floor = scnEditor.instance.selectedFloors[0].seqID;
                            Tuple<int, TileRelativeTo> tuple = scnEditor.instance.levelEventsPanel.selectedEvent.data["endTile"] as Tuple<int, TileRelativeTo>;
                            switch (tuple.Item2)
                            {
                                case TileRelativeTo.ThisTile:
                                    scnEditor.instance.levelEventsPanel.selectedEvent.data["endTile"] =
                                        Tuple.Create(floorToSelect.seqID - floor, tuple.Item2);
                                    break;
                                case TileRelativeTo.Start:
                                    scnEditor.instance.levelEventsPanel.selectedEvent.data["endTile"] =
                                        Tuple.Create(floorToSelect.seqID, tuple.Item2);
                                    break;
                                case TileRelativeTo.End:
                                    scnEditor.instance.levelEventsPanel.selectedEvent.data["endTile"] =
                                        Tuple.Create(1 + floorToSelect.seqID - scnEditor.instance.customLevel.levelMaker.listFloors.Count, tuple.Item2);
                                    break;
                            }
                            scnEditor.instance.levelEventsPanel.ShowPanelOfEvent(scnEditor.instance.levelEventsPanel.selectedEvent);
                            return false;
                        }
                }
            }
            if (!Main.Settings.HighlightTargetedTiles)
                return true;
            if (scnEditor.instance.selectedFloors.Count == 0)
                return true;
            scrFloor lastFloor = scnEditor.instance.selectedFloors[0];
            if (scnEditor.instance.events.
                FindAll(e => e.floor == lastFloor.seqID).
                FindAll(e => e.eventType == LevelEventType.MoveTrack || e.eventType == LevelEventType.RecolorTrack).Count == 0)
                return true;
            else if ((lastFloor is null || lastFloor.seqID == floorToSelect.seqID) && scnEditor.instance.events.
                FindAll(e => e.floor == floorToSelect.seqID).
                FindAll(e => e.eventType == LevelEventType.MoveTrack || e.eventType == LevelEventType.RecolorTrack).Count != 0)
                TargetFloor();
            else
                UntargetFloor();
            return true;
        }

        public static void AfterRemoveEventAtSelected()
        {
            if (scnEditor.instance.levelEventsPanel == null)
                return;
            LevelEventType type = scnEditor.instance.levelEventsPanel.selectedEventType;
            if (type == LevelEventType.MoveTrack || type == LevelEventType.RecolorTrack)
            {
                TargetFloor();
            }
        }

        public static void TargetFloor()
        {
            if (!Main.Settings.HighlightTargetedTiles)
                return;
            if (scnEditor.instance.selectedFloors.Count != 1)
                return;
            UntargetFloor();
            scrFloor floor = scnEditor.instance.selectedFloors[0];
            if (floor.editor.levelEventsPanel is null)
                return;
            LevelEvent selected = floor.editor.levelEventsPanel.selectedEvent;
            if (selected is null)
                return;
            if (selected.eventType != LevelEventType.MoveTrack && selected.eventType != LevelEventType.RecolorTrack)
                return;
            Tuple<int, TileRelativeTo> first = selected.data["startTile"] as Tuple<int, TileRelativeTo>;
            Tuple<int, TileRelativeTo> end = selected.data["endTile"] as Tuple<int, TileRelativeTo>;
            int firstTile, endTile;
            List<scrFloor> floors = floor.customLevel.levelMaker.listFloors;
            switch (first.Item2)
            {
                case TileRelativeTo.ThisTile:
                    firstTile = floor.seqID + first.Item1;
                    break;
                case TileRelativeTo.Start:
                    firstTile = first.Item1;
                    break;
                case TileRelativeTo.End:
                    firstTile = floors.Count - 1 + first.Item1;
                    break;
                default:
                    firstTile = 0;
                    break;
            }
            switch (end.Item2)
            {
                case TileRelativeTo.ThisTile:
                    endTile = floor.seqID + end.Item1;
                    break;
                case TileRelativeTo.Start:
                    endTile = end.Item1;
                    break;
                case TileRelativeTo.End:
                    endTile = floors.Count - 1 + end.Item1;
                    break;
                default:
                    endTile = 0;
                    break;
            }
            if (firstTile > endTile)
            {
                int temp = firstTile;
                firstTile = endTile;
                endTile = temp;
            }
            for (int i = firstTile; i <= endTile; i++)
            {
                if (i < 0)
                    continue;
                if (i > floors.Count - 1)
                    continue;
                if (i == floor.seqID)
                    continue;
                FloorRenderer floorRenderer = floors[i].floorRenderer;
                Color color = floorRenderer.color;
                Color[] array = new Color[2];
                _ = new Color(1f - color.r, 1f - color.g, 1f - color.b);
                Color.RGBToHSV(color, out float num, out float num2, out float num3);
                num = (num + 0.5f) % 1f;
                num3 = Mathf.Clamp01(num3 * 2f);
                float s = Mathf.Clamp01(num2 + 0.5f);
                float v = (num3 > 0.75f) ? (num3 - 0.25f) : (num3 + 0.25f);
                array[0] = Color.HSVToRGB(num, num2, num3);
                array[1] = Color.HSVToRGB(num, s, v);
                floorRenderer.deselectedColor = color;
                floorRenderer.color = array[0];
                _ = DOTween.To(() => floorRenderer.color, delegate (Color x)
                {
                    floorRenderer.color = x;
                }, array[1], 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetId("targetColorTween");
                targets.Add(floors[i]);
            }
        }

        public static void UntargetFloor()
        {
            int _ = DOTween.Kill("targetColorTween", false);
            foreach (scrFloor floor in targets)
            {
                if (floor == null)
                    return;
                FloorRenderer floorRenderer = floor.floorRenderer;
                floorRenderer.color = floorRenderer.deselectedColor;
            }
            targets.Clear();
        }

        public static void AfterTextSetup(PropertyControl_Text __instance)
        {
            if (__instance.propertyInfo.name.Equals("startTile") || __instance.propertyInfo.name.Equals("endTile"))
                __instance.inputField.onEndEdit.AddListener(delegate (string s)
                {
                    if (!Main.IsEnabled)
                        return;
                    if (__instance.editor.selectedFloors.Count != 0)
                        TargetFloor();
                });
        }

        public static void AfterTileSetup(PropertyControl_Tile __instance)
        {
            lastTileRelativeTo.Add(__instance, TileRelativeTo.ThisTile);
            __instance.buttonThisTile.onClick.AddListener(delegate ()
            {
                if (!Main.IsEnabled)
                    return;
                lastTileRelativeTo.TryGetValue(__instance, out TileRelativeTo trt);
                Tuple<int, TileRelativeTo> tuple = scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] as Tuple<int, TileRelativeTo>;
                switch (trt)
                {
                    case TileRelativeTo.ThisTile:
                        return;
                    case TileRelativeTo.Start:
                        if (!Main.Settings.ChangeIndexWhenToggle)
                            break;
                        int selectedFloor = scnEditor.instance.selectedFloors[0].seqID;
                        int floor = tuple.Item1;
                        scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] = Tuple.Create(floor - selectedFloor, tuple.Item2);
                        break;
                    case TileRelativeTo.End:
                        if (!Main.Settings.ChangeIndexWhenToggle)
                            break;
                        selectedFloor = scnEditor.instance.selectedFloors[0].seqID;
                        floor = scnEditor.instance.customLevel.levelMaker.listFloors.Count + tuple.Item1 - 1;
                        scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] = Tuple.Create(floor - selectedFloor, tuple.Item2);
                        break;
                }
                lastTileRelativeTo.Remove(__instance);
                lastTileRelativeTo.Add(__instance, tuple.Item2);
                scnEditor.instance.levelEventsPanel.ShowPanelOfEvent(scnEditor.instance.levelEventsPanel.selectedEvent);
            });
            __instance.buttonFirstTile.onClick.AddListener(delegate ()
            {
                if (!Main.IsEnabled)
                    return;
                lastTileRelativeTo.TryGetValue(__instance, out TileRelativeTo trt);
                Tuple<int, TileRelativeTo> tuple = scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] as Tuple<int, TileRelativeTo>;
                switch (trt)
                {
                    case TileRelativeTo.ThisTile:
                        if (!Main.Settings.ChangeIndexWhenToggle)
                            break;
                        int selectedFloor = scnEditor.instance.selectedFloors[0].seqID;
                        int floor = tuple.Item1 + selectedFloor;
                        scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] = Tuple.Create(floor, tuple.Item2);
                        break;
                    case TileRelativeTo.Start:
                        return;
                    case TileRelativeTo.End:
                        if (!Main.Settings.ChangeIndexWhenToggle)
                            break;
                        selectedFloor = scnEditor.instance.selectedFloors[0].seqID;
                        floor = scnEditor.instance.customLevel.levelMaker.listFloors.Count + tuple.Item1 - 1;
                        scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] = Tuple.Create(floor, tuple.Item2);
                        break;
                }
                lastTileRelativeTo.Remove(__instance);
                lastTileRelativeTo.Add(__instance, tuple.Item2);
                scnEditor.instance.levelEventsPanel.ShowPanelOfEvent(scnEditor.instance.levelEventsPanel.selectedEvent);
            });
            __instance.buttonLastTile.onClick.AddListener(delegate ()
            {
                if (!Main.IsEnabled)
                    return;
                lastTileRelativeTo.TryGetValue(__instance, out TileRelativeTo trt);
                Tuple<int, TileRelativeTo> tuple = scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] as Tuple<int, TileRelativeTo>;
                switch (trt)
                {
                    case TileRelativeTo.ThisTile:
                        if (!Main.Settings.ChangeIndexWhenToggle)
                            break;
                        int selectedFloor = scnEditor.instance.selectedFloors[0].seqID;
                        int floor = tuple.Item1 + selectedFloor;
                        scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] = Tuple.Create(1 + floor - scnEditor.instance.customLevel.levelMaker.listFloors.Count, tuple.Item2);
                        break;
                    case TileRelativeTo.Start:
                        if (!Main.Settings.ChangeIndexWhenToggle)
                            break;
                        selectedFloor = scnEditor.instance.selectedFloors[0].seqID;
                        floor = tuple.Item1;
                        scnEditor.instance.levelEventsPanel.selectedEvent.data[__instance.propertyInfo.name] = Tuple.Create(1 + floor - scnEditor.instance.customLevel.levelMaker.listFloors.Count, tuple.Item2);
                        break;
                    case TileRelativeTo.End:
                        return;
                }
                lastTileRelativeTo.Remove(__instance);
                lastTileRelativeTo.Add(__instance, tuple.Item2);
                scnEditor.instance.levelEventsPanel.ShowPanelOfEvent(scnEditor.instance.levelEventsPanel.selectedEvent);
            });
        }

        public static bool BeforeRemakePath()
        {
            UntargetFloor();
            return true;
        }

        public static void AfterRemakePath(CustomLevel __instance)
        {
            if (__instance.editor.selectedFloors.Count != 0 && scnEditor.instance.events.
                FindAll(e => e.floor == __instance.editor.selectedFloors[0].seqID).
                FindAll(e => e.eventType == LevelEventType.MoveTrack || e.eventType == LevelEventType.RecolorTrack).Count != 0)
            {
                TargetFloor();
            }
        }

        public static void AfterQuitToMenu()
        {
            lastTileRelativeTo.Clear();
        }
    }
}
