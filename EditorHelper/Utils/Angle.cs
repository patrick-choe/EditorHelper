using System;
using System.Collections.Generic;
using MoreEditorOptions.Util;
using UnityEngine;

namespace EditorHelper.Utils {
    public static class Angle {
        public static readonly List<char> Specials = new List<char> {'!', '5', '6', '7', '8', '9'};

        public static readonly Dictionary<int, char> AngleChar = new Dictionary<int, char> {
            {0, 'R'},
            {15, 'p'},
            {30, 'J'},
            {45, 'E'},
            {60, 'T'},
            {75, 'o'},
            {90, 'U'},
            {105, 'q'},
            {120, 'G'},
            {135, 'Q'},
            {150, 'H'},
            {165, 'W'},
            {180, 'L'},
            {195, 'x'},
            {210, 'N'},
            {225, 'Z'},
            {240, 'F'},
            {255, 'V'},
            {270, 'D'},
            {285, 'Y'},
            {300, 'B'},
            {315, 'C'},
            {330, 'M'},
            {345, 'A'},
        };

        public static char RotateFloor(char chara, float rotation) {
            if (Specials.Contains(chara)) return chara;
            var angle = scrLevelMaker.GetAngleFromFloorCharDirectionWithCheck(chara, out _);
            var rot = Mathf.RoundToInt(angle + rotation).NormalizeAngle();
            return AngleChar[rot];
        }

        public static double NormalizeAngle(this double rot, bool zeroTo360 = false) {
            do {
                rot = (rot + 360) % 360;
            } while (rot < 0 || rot >= 360);

            if (zeroTo360 && rot == 0) rot = 360;

            return rot;
        }

        public static float GetAngleDiff(this scrFloor floor, bool checkCW = true) {
            if (floor.midSpin) return 0;
            if (floor.seqID == 0) return 180;
            double diff = (floor.exitangle - floor.entryangle) * Mathf.Rad2Deg * (checkCW && floor.isCCW ? -1 : 1);
            float angleDiff = (float) diff.NormalizeAngle(true);
            return angleDiff;
        }

        public static float NormalizeAngle(this float rot, bool zeroTo360 = false) {
            return (float) ((double) rot).NormalizeAngle(zeroTo360);
        }

        public static int NormalizeAngle(this int rot, bool zeroTo360 = false) {
            return (int) ((double) rot).NormalizeAngle(zeroTo360);
        }

        public static void UpdateAngleTo(this scrFloor floor, float angle, float offsetEnter = 0,  float offsetExit = 0,  bool reset = false, bool resetExit = true,
            bool rotate = true, bool setUTurnIfAnglesMatch = false) {
            FloorMeshRenderer floorMeshRenderer = floor.floorRenderer as FloorMeshRenderer;
            if (floorMeshRenderer != null) {
                float num = (1.5707964f - (float) floor.entryangle) % 6.2831855f;
                if (reset) num = 0;
                angle = 90 + angle;

                floor.exitangle = angle * Mathf.Deg2Rad;
                float num2 = (1.5707964f - (float) floor.exitangle) % 6.2831855f;
                if (resetExit) num2 = (90-angle).NormalizeAngle() * Mathf.Deg2Rad;
                floorMeshRenderer.SetAngle(num + offsetEnter * Mathf.Deg2Rad, num2 + offsetExit * Mathf.Deg2Rad);
                if (setUTurnIfAnglesMatch) {
                    double num3 = Math.Abs(scrMisc.GetAngleMoved((double) num, (double) num2, !floor.isCCW));
                    floor.noChange = (num3 <= 9.999999974752427E-07 || num3 >= 6.28318452835083);
                    FloorMesh floorMesh = floorMeshRenderer.floorMesh;
                    if (floor.noChange) {
                        floorMeshRenderer.SetLength(floorMesh.length - 0.25f);
                        floorMeshRenderer.SetUturnOrMidspin(true);
                    }

                    if (floor.midSpin) {
                        floorMesh.curvaturePoints = 3;
                        floorMeshRenderer.shadowMesh.curvaturePoints = 3;
                        return;
                    }
                }
            } else {
                floor.invoke("SetSpriteFromChar")(rotate);
            }
        }
    }
}