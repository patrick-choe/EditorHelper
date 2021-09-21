using System;
using System.Collections.Generic;
using System.Linq;
using EditorHelper.Utils;
using HarmonyLib;
using MoreEditorOptions.Util;
using OggVorbisEncoder.Setup;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Object = UnityEngine.Object;

namespace EditorHelper.Patch {
    public class ShowSelectedTilePatch {
        [HarmonyPatch(typeof(scnEditor), "UpdateSelectedFloor")]
        public static class UpdateFloorAnglePatch {
            public static scrLetterPress LetterPress = null;
            public static scrLetterPress LetterPress2 = null;
            public static bool Pressing = false;
            public static float angle = 0;
            public static scrFloor Floor = null;
            
            public static Vector2 Rotate(Vector2 v, float delta) {
                delta = Mathf.Deg2Rad * delta;
                return new Vector2(
                    v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                    v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
                );
            }

            public static void Postfix() {
                if (!scnEditor.instance.SelectionIsSingle()) return;
                if (!CustomLevel.instance.levelData.isOldLevel && Main.Settings.ChangeTileAngle.Check && Input.GetMouseButton(0)) {
                    if (scnEditor.instance.selectedFloors.Count == 1) {
                        CorrectRotationPatch.LastPos2 = Camera.current.transform.position;
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
                        var delta = (float) Main.Settings.MeshDelta;
                        if (delta != 0)
                            angle = Mathf.Round(angle / delta) * delta.NormalizeAngle(true);
                        else
                            angle = angle.NormalizeAngle(true);
                        angle = (float) Math.Round(angle, 4);
                        changePos = Rotate(new Vector2(1.5f, 0), 180-angle) + pos;
                        obj.transform.position = changePos;
                        obj.transform.eulerAngles = new Vector3(0, 0, angle);
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
                    LetterPress2 = null;
                    Pressing = false;
                }
            }
        }


        [HarmonyPatch(typeof(scnEditor), "UpdateSelectedFloor")]
        public static class ShowSelectedTile {
            public static List<scrLetterPress> Texts = new();
            public static scrFloor floorMoreSelected = null;

            public static void Prefix() {
                if (!Main.Settings.EnableSelectedTileShowAngle) return;
                foreach (var textsValue in Texts.ToArray()) {
                    try {
                        Object.Destroy(textsValue.gameObject);
                    } catch (Exception) {
                        // ignored
                    }

                    Texts.Remove(textsValue);
                }

                if (UpdateFloorAnglePatch.Pressing) return;
                foreach (var scrFloor in scnEditor.instance.selectedFloors) {
                    try {
                        if (scrFloor.seqID >= scrLevelMaker.instance.listFloors.Count - 1 || scrFloor.seqID == 0) {
                            var gameObject2 =
                                UnityEngine.Object.Instantiate<GameObject>(scnEditor.instance.prefab_editorNum,
                                    scrFloor.transform);
                            gameObject2.GetComponent<scrLetterPress>().letterText.text = "180";
                            Texts.Add(gameObject2.GetComponent<scrLetterPress>());
                            gameObject2.transform.eulerAngles = Vector3.zero;
                            continue;
                        }

                        var gameObject = UnityEngine.Object.Instantiate<GameObject>(scnEditor.instance.prefab_editorNum,
                            scrFloor.transform);
                        if (scrFloor.seqID >= scrLevelMaker.instance.listFloors.Count - 1) continue;
                        var nextFloor = scrLevelMaker.instance.listFloors[scrFloor.seqID + 1].floatDirection;
                        var angle = scrFloor.GetAngleDiff();
                        var letterPress = gameObject.GetComponent<scrLetterPress>();
                        var field = letterPress.letterText.gameObject.AddComponent<InputField>();
                        field.textComponent = letterPress.letterText;
                        field.text = $"{Math.Round(angle, 4)}";
                        gameObject.transform.eulerAngles = Vector3.zero;
                        Texts.Add(letterPress);
                    } catch (Exception) {
                        UnityModManager.Logger.Log($"Error: {scrFloor.seqID}");
                    }
                }
            }
        }
    }
}