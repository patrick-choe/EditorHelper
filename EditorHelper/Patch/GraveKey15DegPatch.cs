using System;
using ADOFAI;
using HarmonyLib;
using MoreEditorOptions.Util;
using UnityEngine;
using UnityEngine.UI;

namespace EditorHelper.Patch {
    [HarmonyPatch(typeof(scnEditor), "Update")]
    public static class GraveKey15DegPatch {
        // ReSharper disable once InconsistentNaming
        public static void Postfix() {
            if (!Main.Settings.GraveToSee15Degs) return;
            if (scnEditor.instance.SelectionIsSingle()) {
                if (Input.GetKey(KeyCode.BackQuote)) {
                    scnEditor.instance.floorButtonExtraCanvas.gameObject.SetActive(true);
                    scnEditor.instance.floorButtonPrimaryCanvas.gameObject.SetActive(true);

                    var editor = scnEditor.instance;
                    editor.buttonT.transform.eulerAngles = new Vector3(0, 0, 75);
                    editor.buttonJ.transform.eulerAngles = new Vector3(0, 0, 15);
                    editor.buttonM.transform.eulerAngles = new Vector3(0, 0, -15);
                    editor.buttonB.transform.eulerAngles = new Vector3(0, 0, -75);
                    editor.buttonF.transform.eulerAngles = new Vector3(0, 0, -105);
                    editor.buttonN.transform.eulerAngles = new Vector3(0, 0, -165);
                    editor.buttonH.transform.eulerAngles = new Vector3(0, 0, 165);
                    editor.buttonG.transform.eulerAngles = new Vector3(0, 0, 105);
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
                    editor.buttonQ.transform.localPosition = new Vector3(-20, 20);
                    editor.buttonE.transform.localPosition = new Vector3(20, 20);
                    editor.buttonZ.transform.localPosition = new Vector3(-20, -20);
                    editor.buttonC.transform.localPosition = new Vector3(20, -20);
                }

                if (Input.GetKeyUp(KeyCode.BackQuote)) {
                    var editor = scnEditor.instance;
                    editor.buttonT.transform.eulerAngles = new Vector3(0, 0, 60);
                    editor.buttonJ.transform.eulerAngles = new Vector3(0, 0, 30);
                    editor.buttonM.transform.eulerAngles = new Vector3(0, 0, -30);
                    editor.buttonB.transform.eulerAngles = new Vector3(0, 0, -60);
                    editor.buttonF.transform.eulerAngles = new Vector3(0, 0, -120);
                    editor.buttonN.transform.eulerAngles = new Vector3(0, 0, -150);
                    editor.buttonH.transform.eulerAngles = new Vector3(0, 0, 150);
                    editor.buttonG.transform.eulerAngles = new Vector3(0, 0, 120);
                    
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