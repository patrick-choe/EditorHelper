using System.Collections.Generic;
using System.Linq;

namespace EditorHelper {
    public static class FloorPatch {
        public static int floors = 0;

        public static bool SetFloors(scnEditor __instance) {
            floors = __instance.customLevel.levelMaker.listFloors.Count - 1;
            return true;
        }

        public static void AfterCreateFloor(scnEditor __instance, float angle, char chara,
            bool pulseFloorButtons = true, bool fullSpin = false) {
            if (__instance.selectedFloors.Count == 0)
                return;
            if (__instance.selectedFloors[0].seqID == 0)
                return;
            int now = __instance.customLevel.levelMaker.listFloors.Count - 1;
            if (floors >= now)
                return;
            EventTileChanger.Change(__instance, now - floors, __instance.selectedFloors[0].seqID);
        }

        public static void AfterDeleteFloor(scnEditor __instance, int sequenceIndex, bool remakePath = true) {
            int pathLength = __instance.levelData.pathData.Length;
            int angleCount = __instance.levelData.angleData.Count;
            bool oldLevel = __instance.customLevel.levelData.isOldLevel;
            int now = oldLevel ? pathLength : angleCount;
            if (floors > now && remakePath) {
                EventTileChanger.Change(__instance, now - floors, sequenceIndex);
            }
        }

        public static void AfterPasteFloors(scnEditor __instance) {
            List<scrFloor> selectedFloors = __instance.selectedFloors;
            if (!__instance.clipboardCharsEvents.Any())
                return;
            int now = __instance.customLevel.levelMaker.listFloors.Count - 1;
            if (floors == now)
                return;
            int num = selectedFloors[0].seqID - __instance.clipboardCharsEvents.Count + 1;
            EventTileChanger.Change(__instance, now - floors, num, num, selectedFloors[0].seqID);
        }
    }
}