using System.Collections.Generic;
using System.Reflection;
using EditorHelper.Utils;
using HarmonyLib;
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

        private static char GetSmallRotDirection(char direction, bool CW) =>
            CW ? Angle.RotateFloor(direction, -15) : Angle.RotateFloor(direction, 15);
    }
}