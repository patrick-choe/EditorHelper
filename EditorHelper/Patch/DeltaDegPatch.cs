using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreEditorOptions.Util;
using UnityEngine;

namespace EditorHelper.Patch {
    [HarmonyPatch(typeof(scrLevelMaker), "GetRotDirection", typeof(float), typeof(bool))]
    internal static class RotateDirectionPatch {
        public static bool Prefix(float direction, bool CW, ref float __result) {
            if (direction == 999f) {
                __result = direction;
                return false;
            }

            __result = direction + (float) (CW ? -1 : 1) * 90f;
            return false;
        }
    }

    [HarmonyPatch(typeof(scnEditor), "RotateFloor")]
    internal static class RotateFloorPatch {
        private static readonly MethodInfo SelectFloor = typeof(scnEditor).GetMethod("SelectFloor", AccessTools.all);

        private static void Postfix(scnEditor __instance, scrFloor floor, bool CW, bool remakePath) {
            if (!Main.Settings.SmallerDeltaDeg || !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) {
                return;
            }

            int seqID = floor.seqID;
            if (seqID == 0) {
                return;
            }

            if (__instance.get<bool>("isOldLevel")) {
                char rotDirection = GetSmallRotDirection(floor.stringDirection, CW);
                __instance.levelData.pathData = __instance.levelData.pathData.Remove(floor.seqID - 1, 1);
                __instance.levelData.pathData =
                    __instance.levelData.pathData.Insert(floor.seqID - 1, rotDirection.ToString());
            } else {
                float rotDirection2 = GetSmallRotDirection(floor.floatDirection, CW, 15f);
                __instance.levelData.angleData.RemoveAt(floor.seqID - 1);
                __instance.levelData.angleData.Insert(floor.seqID - 1, rotDirection2);
            }

            if (remakePath) {
                __instance.RemakePath(true);
                SelectFloor.Invoke(__instance, new object[] {__instance.get<List<scrFloor>>("floors")[seqID], true});
            }
        }

        private static float GetSmallRotDirection(float direction, bool CW, float rotation) {
            if (direction == 999f) {
                return direction;
            }

            return direction + (float) (CW ? -1 : 1) * rotation;
        }

private static char GetSmallRotDirection(char direction, bool CW) {
            var ch = direction;
            if (direction == Main.AngleChar[0]) {
                ch = CW ? Main.AngleChar[345] : Main.AngleChar[15];
            } else if (direction == Main.AngleChar[15]) {
                ch = CW ? Main.AngleChar[0] : Main.AngleChar[30];
            } else if (direction == Main.AngleChar[30]) {
                ch = CW ? Main.AngleChar[15] : Main.AngleChar[45];
            } else if (direction == Main.AngleChar[45]) {
                ch = CW ? Main.AngleChar[30] : Main.AngleChar[60];
            } else if (direction == Main.AngleChar[60]) {
                ch = CW ? Main.AngleChar[45] : Main.AngleChar[75];
            } else if (direction == Main.AngleChar[75]) {
                ch = CW ? Main.AngleChar[60] : Main.AngleChar[90];
            } else if (direction == Main.AngleChar[90]) {
                ch = CW ? Main.AngleChar[75] : Main.AngleChar[105];
            } else if (direction == Main.AngleChar[105]) {
                ch = CW ? Main.AngleChar[90] : Main.AngleChar[150];
            } else if (direction == Main.AngleChar[120]) {
                ch = CW ? Main.AngleChar[135] : Main.AngleChar[165];
            } else if (direction == Main.AngleChar[135]) {
                ch = CW ? Main.AngleChar[150] : Main.AngleChar[120];
            } else if (direction == Main.AngleChar[150]) {
                ch = CW ? Main.AngleChar[105] : Main.AngleChar[135];
            } else if (direction == Main.AngleChar[165]) {
                ch = CW ? Main.AngleChar[120] : Main.AngleChar[180];
            } else if (direction == Main.AngleChar[180]) {
                ch = CW ? Main.AngleChar[165] : Main.AngleChar[195];
            } else if (direction == Main.AngleChar[195]) {
                ch = CW ? Main.AngleChar[180] : Main.AngleChar[210];
            } else if (direction == Main.AngleChar[210]) {
                ch = CW ? Main.AngleChar[195] : Main.AngleChar[225];
            } else if (direction == Main.AngleChar[225]) {
                ch = CW ? Main.AngleChar[210] : Main.AngleChar[240];
            } else if (direction == Main.AngleChar[240]) {
                ch = CW ? Main.AngleChar[225] : Main.AngleChar[255];
            } else if (direction == Main.AngleChar[255]) {
                ch = CW ? Main.AngleChar[240] : Main.AngleChar[270];
            } else if (direction == Main.AngleChar[270]) {
                ch = CW ? Main.AngleChar[255] : Main.AngleChar[285];
            } else if (direction == Main.AngleChar[285]) {
                ch = CW ? Main.AngleChar[270] : Main.AngleChar[330];
            } else if (direction == Main.AngleChar[300]) {
                ch = CW ? Main.AngleChar[315] : Main.AngleChar[345];
            } else if (direction == Main.AngleChar[315]) {
                ch = CW ? Main.AngleChar[330] : Main.AngleChar[300];
            } else if (direction == Main.AngleChar[330]) {
                ch = CW ? Main.AngleChar[285] : Main.AngleChar[315];
            } else if (direction == Main.AngleChar[345]) {
                ch = CW ? Main.AngleChar[300] : Main.AngleChar[0];
            }

            return ch;
        }
    }
}