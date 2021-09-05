using System;
using System.Collections.Generic;

namespace EditorHelper
{
    public static class EventTileChanger
    {
        public static void Change(scnEditor editor, int change, int at, int exceptFrom = 0, int exceptTo = 0)
        {
            List<ADOFAI.LevelEvent> events = editor.events;
            foreach (ADOFAI.LevelEvent e in events.
                FindAll(e => e.eventType == ADOFAI.LevelEventType.MoveTrack || e.eventType == ADOFAI.LevelEventType.RecolorTrack).
                FindAll(e => e.floor < exceptFrom || e.floor > exceptTo))
            {
                if (!e.data.TryGetValue("startTile", out var value))
                    continue;
                Tuple<int, TileRelativeTo> tuple = value as Tuple<int, TileRelativeTo>;
                int i = tuple.Item1;
                TileRelativeTo trt = tuple.Item2;
                switch (trt)
                {
                    case TileRelativeTo.ThisTile:
                        if (!Main.Settings.ThisTile)
                            return;
                        if (e.floor >= at)
                        {
                            if (e.floor + i < at)
                            {
                                e.data["startTile"] = Tuple.Create(i - change, trt);
                            }
                        }
                        else
                        {
                            if (e.floor + i >= at)
                            {
                                e.data["startTile"] = Tuple.Create(i + change, trt);
                            }
                        }
                        break;
                    case TileRelativeTo.Start:
                        if (!Main.Settings.FirstTile)
                            return;
                        if (i >= at)
                            e.data["startTile"] = Tuple.Create(i + change, trt);
                        break;
                    case TileRelativeTo.End:
                        if (!Main.Settings.LastTile)
                            return;
                        if (editor.customLevel.levelMaker.listFloors.Count + i < at)
                            e.data["startTile"] = Tuple.Create(i - change, trt);
                        break;
                }
                if (!e.data.TryGetValue("endTile", out value))
                    continue;
                tuple = value as Tuple<int, TileRelativeTo>;
                i = tuple.Item1;
                trt = tuple.Item2;
                switch (trt)
                {
                    case TileRelativeTo.ThisTile:
                        if (!Main.Settings.ThisTile)
                            return;
                        if (e.floor >= at)
                        {
                            if (e.floor + i < at)
                            {
                                e.data["endTile"] = Tuple.Create(i - change, trt);
                            }
                        }
                        else
                        {
                            if (e.floor + i >= at)
                            {
                                e.data["endTile"] = Tuple.Create(i + change, trt);
                            }
                        }
                        break;
                    case TileRelativeTo.Start:
                        if (!Main.Settings.FirstTile)
                            return;
                        if (i >= at)
                            e.data["endTile"] = Tuple.Create(i + change, trt);
                        break;
                    case TileRelativeTo.End:
                        if (!Main.Settings.LastTile)
                            return;
                        if (editor.customLevel.levelMaker.listFloors.Count + i < at)
                            e.data["endTile"] = Tuple.Create(i - change, trt);
                        break;
                }
            }
        }
    }
}
