using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EditorHelper.Patch
{
    [HarmonyPatch(typeof(scnEditor), "RotateFloor")]
    internal static class RotateFloorPatch
    {
        private static readonly MethodInfo SelectFloor = typeof(scnEditor).GetMethod("SelectFloor", AccessTools.all);

        private static void Postfix(scnEditor __instance, scrFloor floor, bool CW, bool remakePath)
        {
            if (!Main.IsEnabled || !Main.Settings.SmallerDeltaDeg)
            {
                return;
            }
            
            var seqId = floor.seqID;
            if (seqId == 0)
                return;
            var rotDirection = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)
                ? GetSmallRotDirection(floor.stringDirection, CW)
                : __instance.lm.GetRotDirection(floor.stringDirection, CW);
            __instance.levelData.pathData = __instance.levelData.pathData.Remove(floor.seqID - 1, 1);
            __instance.levelData.pathData = __instance.levelData.pathData.Insert(floor.seqID - 1, rotDirection.ToString());
            if (!remakePath)
                return;
            __instance.RemakePath();

            SelectFloor.Invoke(__instance, new object[] { __instance.customLevel.levelMaker.listFloors[seqId], true });
        }

        private static char GetSmallRotDirection(char direction, bool CW)
        {
            var ch = direction;
            switch (direction)
            {
                case scrLevelMaker.Angle0:
                    ch = CW ? scrLevelMaker.Angle345 : scrLevelMaker.Angle15;
                    break;
                case scrLevelMaker.Angle15:
                    ch = CW ? scrLevelMaker.Angle0 : scrLevelMaker.Angle30;
                    break;
                case scrLevelMaker.Angle30:
                    ch = CW ? scrLevelMaker.Angle15 : scrLevelMaker.Angle45;
                    break;
                case scrLevelMaker.Angle45:
                    ch = CW ? scrLevelMaker.Angle30 : scrLevelMaker.Angle60;
                    break;
                case scrLevelMaker.Angle60:
                    ch = CW ? scrLevelMaker.Angle45 : scrLevelMaker.Angle75;
                    break;
                case scrLevelMaker.Angle75:
                    ch = CW ? scrLevelMaker.Angle60 : scrLevelMaker.Angle90;
                    break;
                case scrLevelMaker.Angle90:
                    ch = CW ? scrLevelMaker.Angle75 : scrLevelMaker.Angle105;
                    break;
                case scrLevelMaker.Angle105:
                    ch = CW ? scrLevelMaker.Angle90 : scrLevelMaker.Angle150;
                    break;
                case scrLevelMaker.Angle120:
                    ch = CW ? scrLevelMaker.Angle135 : scrLevelMaker.Angle165;
                    break;
                case scrLevelMaker.Angle135:
                    ch = CW ? scrLevelMaker.Angle150 : scrLevelMaker.Angle120;
                    break;
                case scrLevelMaker.Angle150:
                    ch = CW ? scrLevelMaker.Angle105 : scrLevelMaker.Angle135;
                    break;
                case scrLevelMaker.Angle165:
                    ch = CW ? scrLevelMaker.Angle120 : scrLevelMaker.Angle180;
                    break;
                case scrLevelMaker.Angle180:
                    ch = CW ? scrLevelMaker.Angle165 : scrLevelMaker.Angle195;
                    break;
                case scrLevelMaker.Angle195:
                    ch = CW ? scrLevelMaker.Angle180 : scrLevelMaker.Angle210;
                    break;
                case scrLevelMaker.Angle210:
                    ch = CW ? scrLevelMaker.Angle195 : scrLevelMaker.Angle225;
                    break;
                case scrLevelMaker.Angle225:
                    ch = CW ? scrLevelMaker.Angle210 : scrLevelMaker.Angle240;
                    break;
                case scrLevelMaker.Angle240:
                    ch = CW ? scrLevelMaker.Angle225 : scrLevelMaker.Angle255;
                    break;
                case scrLevelMaker.Angle255:
                    ch = CW ? scrLevelMaker.Angle240 : scrLevelMaker.Angle270;
                    break;
                case scrLevelMaker.Angle270:
                    ch = CW ? scrLevelMaker.Angle255 : scrLevelMaker.Angle285;
                    break;
                case scrLevelMaker.Angle285:
                    ch = CW ? scrLevelMaker.Angle270 : scrLevelMaker.Angle330;
                    break;
                case scrLevelMaker.Angle300:
                    ch = CW ? scrLevelMaker.Angle315 : scrLevelMaker.Angle345;
                    break;
                case scrLevelMaker.Angle315:
                    ch = CW ? scrLevelMaker.Angle330 : scrLevelMaker.Angle300;
                    break;
                case scrLevelMaker.Angle330:
                    ch = CW ? scrLevelMaker.Angle285 : scrLevelMaker.Angle315;
                    break;
                case scrLevelMaker.Angle345:
                    ch = CW ? scrLevelMaker.Angle300 : scrLevelMaker.Angle0;
                    break;
            }
            return ch;
        }
    }
}