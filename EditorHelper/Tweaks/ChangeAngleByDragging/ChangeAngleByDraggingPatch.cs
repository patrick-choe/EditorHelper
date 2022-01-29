using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using DG.Tweening;
using EditorHelper.Core.Patch;
using EditorHelper.Core.Tweaks;
using EditorHelper.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Tweaks.ChangeAngleByDragging {
    public abstract class ChangeAngleByDraggingPatch {
        public static ChangeAngleByDraggingSetting Setting => TweakManager.Setting<ChangeAngleByDraggingTweak, ChangeAngleByDraggingSetting>()!;
        
        [TweakPatch(nameof(scnEditor), "UpdateSelectedFloor")]
        public static class UpdateFloorAnglePatch {
            public static scrLetterPress? LetterPress = null;
            public static bool Pressing = false;
            public static float angle = 0;
            public static scrFloor Floor = null!;
            
            public static Vector2 Rotate(Vector2 v, float delta) {
                delta = Mathf.Deg2Rad * delta;
                return new Vector2(
                    v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                    v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
                );
            }

            public static void Postfix() {
                if (!scnEditor.instance.SelectionIsSingle()) return;
                //TODO: Rework this
                if (!CustomLevel.instance.levelData.isOldLevel &&/* Setting.ChangeTileAngle.Check && */Input.GetMouseButton(0)) {
                    var objects = scnEditor.instance.invoke<GameObject[]>("ObjectsAtMouse")();
                    if (scnEditor.instance.SelectionIsSingle() && (Pressing || objects.Any(o => o.GetComponent<scrFloor>() == scnEditor.instance.selectedFloors[0]))) {
                        var obj = scnEditor.instance.selectedFloors[0];
                        if (obj == null || obj.seqID == 0) return;
                        Floor = obj;
                        var prev = scrLevelMaker.instance.listFloors[obj.seqID - 1];
                        var pos = (Vector2) prev.transform.position;
                        var mousePos = (Vector2) Camera.current.ScreenToWorldPoint(Input.mousePosition);
                        var posDiff = mousePos - pos;
                        var changePos = posDiff.normalized * 1.5f + pos;
                        posDiff = changePos - pos;
                        if (posDiff.y >= 0)
                            angle = Vector2.Angle(new Vector2(1.5f, 0), pos - changePos);
                        else
                            angle = 180 + Vector2.Angle(new Vector2(1.5f, 0), changePos - pos);
                        float delta = (float) Setting.MeshDelta;
                        if (delta != 0)
                            angle = Mathf.Round(angle / delta) * delta.NormalizeAngle(true);
                        else
                            angle = angle.NormalizeAngle(true);
                        angle = (float) Math.Round(angle, 4);
                        changePos = Rotate(new Vector2(1.5f, 0), 180-angle) + pos;
                        var transform = obj.transform;
                        transform.position = changePos;
                        transform.eulerAngles = new Vector3(0, 0, angle);
                        Pressing = true;
                        if (LetterPress == null) {
                            var gameObject2 =
                                UnityEngine.Object.Instantiate<GameObject>(scnEditor.instance.prefab_editorNum, obj.transform);
                            gameObject2.GetComponent<scrLetterPress>().letterText.text = $"{angle}";
                            LetterPress = gameObject2.GetComponent<scrLetterPress>();
                            gameObject2.transform.eulerAngles = Vector3.zero;
                        }
                        LetterPress.letterText.text = $"{((prev.floatDirection + angle) * (obj.isCCW ? -1 : 1)).NormalizeAngle(true)}";
                        prev.UpdateAngleTo(angle, 0, 180);
                        obj.UpdateAngleTo(180, -angle * 2, -angle * 2, true);
                        return;
                    }
                }

                if (Pressing) {
                    UnityModManager.Logger.Log($"{angle}");
                    CustomLevel.instance.levelData.angleData[Floor.seqID - 1] = (float) Math.Round(180 - angle, 4);
                    scnEditor.instance.RemakePath();
                    scnEditor.instance.selectedFloors = new List<scrFloor>();
                
                    LetterPress = null;
                    Pressing = false;
                }
            }
        }
    }
}