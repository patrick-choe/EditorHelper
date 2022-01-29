using System;
using System.Collections.Generic;
using ADOFAI;
using EditorHelper.Core.Patch;
using EditorHelper.Core.Tweaks;
using EditorHelper.Tweaks.ChangeAngleByDragging;
using EditorHelper.Tweaks.RotateScreen;
using EditorHelper.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Object = UnityEngine.Object;

namespace EditorHelper.Tweaks.Miscellaneous {
    public class MiscellaneousPatch {
        private static MiscellaneousSetting Setting =>
            TweakManager.Setting<MiscellaneousTweak, MiscellaneousSetting>()!;

        [TweakPatch(nameof(RDColorPickerPopup), "Hide")]
        public static class RemoveFFLastPatch {
            public static void Postfix(RDColorPickerPopup __instance) {
                if (!Setting.RemoveFFOnRGBA) return;
                var colorPC = __instance.get<PropertyControl_Color>("colorPC")!;
                var text = colorPC.text;
                if (text.Length == 8 && text.EndsWith("ff")) {
                    colorPC.text = text.Substring(0, 6);
                }
            }
        }

        [TweakPatch(nameof(scnEditor), "UpdateSelectedFloor")]
        public static class ShowSelectedTile {
            public static readonly List<scrLetterPress> Texts = new();

            public static void Prefix() {
                if (!Setting.ShowDegree) return;
                foreach (var textsValue in Texts.ToArray()) {
                    try {
                        Object.Destroy(textsValue.gameObject);
                    } catch (Exception) {
                        // ignored
                    }

                    Texts.Remove(textsValue);
                }

                if (ChangeAngleByDraggingPatch.UpdateFloorAnglePatch.Pressing) return;
                foreach (var scrFloor in scnEditor.instance.selectedFloors) {
                    try {
                        var gameObject =
                            Object.Instantiate(scnEditor.instance.prefab_editorNum, scrFloor.transform);
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


        [TweakPatch(nameof(scnEditor), "Update")]
        public static class GraveKey15DegPatch {
            // ReSharper disable once InconsistentNaming
            public static void Postfix() {
                if (!Setting.GraveToSee15Degs) return;
                if (scnEditor.instance.SelectionIsSingle()) {
                    if (Input.GetKey(KeyCode.BackQuote)) {
                        scnEditor.instance.floorButtonExtraCanvas.gameObject.SetActive(true);
                        scnEditor.instance.floorButtonPrimaryCanvas.gameObject.SetActive(true);

                        var angle = Camera.current.transform.eulerAngles.z;
                        var editor = scnEditor.instance;
                        editor.buttonT.transform.eulerAngles = new Vector3(0, 0, 75 + angle);
                        editor.buttonJ.transform.eulerAngles = new Vector3(0, 0, 15 + angle);
                        editor.buttonM.transform.eulerAngles = new Vector3(0, 0, -15 + angle);
                        editor.buttonB.transform.eulerAngles = new Vector3(0, 0, -75 + angle);
                        editor.buttonF.transform.eulerAngles = new Vector3(0, 0, -105 + angle);
                        editor.buttonN.transform.eulerAngles = new Vector3(0, 0, -165 + angle);
                        editor.buttonH.transform.eulerAngles = new Vector3(0, 0, 165 + angle);
                        editor.buttonG.transform.eulerAngles = new Vector3(0, 0, 105 + angle);
                        editor.AddListener(editor.buttonT, 'o');
                        editor.AddListener(editor.buttonJ, 'p');
                        editor.AddListener(editor.buttonM, 'A');
                        editor.AddListener(editor.buttonB, 'Y');
                        editor.AddListener(editor.buttonF, 'V');
                        editor.AddListener(editor.buttonN, 'x');
                        editor.AddListener(editor.buttonH, 'W');
                        editor.AddListener(editor.buttonG, 'q');
                        scnEditor.instance.invoke("UpdateFloorDirectionButtons")(true);
                        editor.floorButtonPrimaryCanvas.transform.localScale = Vector3.one * 0.8f;
                        editor.buttonD.gameObject.SetActive(false);
                        editor.buttonA.gameObject.SetActive(false);
                        editor.buttonW.gameObject.SetActive(false);
                        editor.buttonS.gameObject.SetActive(false);
                        editor.buttonQ.gameObject.SetActive(true);
                        editor.buttonE.gameObject.SetActive(true);
                        editor.buttonZ.gameObject.SetActive(true);
                        editor.buttonC.gameObject.SetActive(true);
                        editor.buttonQ.transform.localPosition =
                            Quaternion.AngleAxis(angle, new Vector3(-1, 1)) * new Vector3(-20, 20);
                        editor.buttonE.transform.localPosition =
                            Quaternion.AngleAxis(angle, new Vector3(1, 1)) * new Vector3(20, 20);
                        editor.buttonZ.transform.localPosition =
                            Quaternion.AngleAxis(angle, new Vector3(-1, -1)) * new Vector3(-20, -20);
                        editor.buttonC.transform.localPosition =
                            Quaternion.AngleAxis(angle, new Vector3(1, -1)) * new Vector3(20, -20);
                    }

                    if (Input.GetKeyUp(KeyCode.BackQuote)) {
                        var angle = Camera.current.transform.eulerAngles.z;
                        var quart = Quaternion.AngleAxis(angle, Vector3.up);
                        var editor = scnEditor.instance;
                        editor.buttonT.transform.eulerAngles = new Vector3(0, 0, 60 + angle);
                        editor.buttonJ.transform.eulerAngles = new Vector3(0, 0, 30 + angle);
                        editor.buttonM.transform.eulerAngles = new Vector3(0, 0, -30 + angle);
                        editor.buttonB.transform.eulerAngles = new Vector3(0, 0, -60 + angle);
                        editor.buttonF.transform.eulerAngles = new Vector3(0, 0, -120 + angle);
                        editor.buttonN.transform.eulerAngles = new Vector3(0, 0, -150 + angle);
                        editor.buttonH.transform.eulerAngles = new Vector3(0, 0, 150 + angle);
                        editor.buttonG.transform.eulerAngles = new Vector3(0, 0, 120 + angle);

                        editor.AddListener(editor.buttonT, 'T');
                        editor.AddListener(editor.buttonJ, 'J');
                        editor.AddListener(editor.buttonM, 'M');
                        editor.AddListener(editor.buttonB, 'B');
                        editor.AddListener(editor.buttonF, 'F');
                        editor.AddListener(editor.buttonN, 'N');
                        editor.AddListener(editor.buttonH, 'H');
                        editor.AddListener(editor.buttonG, 'G');
                        scnEditor.instance.invoke("UpdateFloorDirectionButtons")(true);
                        editor.buttonD.gameObject.SetActive(true);
                        editor.buttonA.gameObject.SetActive(true);
                        editor.floorButtonPrimaryCanvas.transform.localScale = Vector3.one;
                        editor.buttonQ.transform.localPosition = Vector3.zero;
                        editor.buttonE.transform.localPosition = Vector3.zero;
                        editor.buttonZ.transform.localPosition = Vector3.zero;
                        editor.buttonC.transform.localPosition = Vector3.zero;
                    }
                }
            }
        }
    }
}