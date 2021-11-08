using System.Linq;
using EditorHelper.Core.TweakFunctions;
using EditorHelper.Core.Tweaks;
using EditorHelper.Utils;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.RotateSmallerDeg {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Enable Smaller Delta Degree")]
    [TweakDescription(LangCode.Korean, "더 작은 각도로 타일 회전")]
    public class RotateSmallerDegTweak : Tweak, ISettingClass<RotateSmallerDegSetting> {

        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
            if (GCS.settingsInfo == null) return;
            if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyFlash"))
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");
            if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyTiles"))
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyTiles");
        }
        
        [TweakFunction("RotateCCW", Require.SelectionIsSingle | Require.SelectionIsMultiple)]
        public void RotateCCW() {
            DebugUtils.Log("RotateCCW");
            RotateSelection(false);
        }

        [TweakFunction("RotateCW", Require.SelectionIsSingle | Require.SelectionIsMultiple)]
        public void RotateCW() {
            DebugUtils.Log("RotateCW");
            RotateSelection(true);
        }

        private void RotateSelection(bool CW) {
            int seqID = scnEditor.instance.selectedFloors[0].seqID;
            int seqID2 = scnEditor.instance.selectedFloors.Last<scrFloor>().seqID;
            int seqID3 = scnEditor.instance.multiSelectPoint.seqID;
            if (seqID == 0) {
                return;
            }

            foreach (scrFloor floor in scnEditor.instance.selectedFloors) {
                Rotate(floor, CW, false);
            }

            scnEditor.instance.RemakePath(true);
            scnEditor.instance.MultiSelectFloors(scrLevelMaker.instance.listFloors[seqID], scrLevelMaker.instance.listFloors[seqID2], false);
            scnEditor.instance.multiSelectPoint = scrLevelMaker.instance.listFloors[seqID3];
        }

        private void Rotate(scrFloor floor, bool CW, bool remakePath) {
            if (scnEditor.instance.get<bool>("lockPathEditing")) {
                return;
            }

            int seqID = floor.seqID;
            if (seqID == 0) {
                return;
            }

            if (scnEditor.instance.levelData.isOldLevel) {
                char rotDirection = GetSmallRotDirection(floor.stringDirection, CW);
                scnEditor.instance.levelData.pathData = scnEditor.instance.levelData.pathData.Remove(floor.seqID - 1, 1);
                scnEditor.instance.levelData.pathData = scnEditor.instance.levelData.pathData.Insert(floor.seqID - 1, rotDirection.ToString());
            } else {
                float rotDirection2 = GetSmallRotDirection(floor.floatDirection, CW, (float) this.Setting().DeltaDeg);
                scnEditor.instance.levelData.angleData.RemoveAt(floor.seqID - 1);
                scnEditor.instance.levelData.angleData.Insert(floor.seqID - 1, rotDirection2);
            }

            if (remakePath) {
                scnEditor.instance.RemakePath(true);
                scnEditor.instance.SelectFloor(scrLevelMaker.instance.listFloors[seqID], true);
            }
        }

        private static float GetSmallRotDirection(float direction, bool CW, float rotation) {
            if (direction == 999f) {
                return direction;
            }

            return direction + (CW ? -1 : 1) * rotation;
        }

        private static char GetSmallRotDirection(char direction, bool CW) =>
            CW ? Angle.RotateFloor(direction, -15) : Angle.RotateFloor(direction, 15);
    }
}