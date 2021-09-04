using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using DG.Tweening;
using HarmonyLib;
using MoreEditorOptions.Util;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Patch {
	[HarmonyPatch(typeof(scnEditor), "CreateFloorWithCharOrAngle")]
	public static class CreateFloorPatch {
		public static bool Prefix(scnEditor __instance, float angle, char chara, bool pulseFloorButtons, bool fullSpin) {
			__instance.CreateFloorWithShiftedCharOrAngle(angle, chara, pulseFloorButtons, fullSpin);
			return false;
		}
	}
	
	[HarmonyPatch(typeof(scnEditor), "Update")]
	internal static class UpdateRotationPatch {
		internal static float CurrentRot => Camera.current.transform.eulerAngles.z;

		private static void Prefix(scnEditor __instance, ref bool ___refreshBgSprites, ref bool ___refreshDecSprites) {
			if (!Main.Settings.EnableScreenRot ||
			    !scrController.instance.paused ||
			    GCS.standaloneLevelMode ||
			    StandaloneFileBrowser.lastFrameCount == Time.frameCount) {
				return;
			}

			var selectedObj = __instance.eventSystem.currentSelectedGameObject;

			if (selectedObj != null && selectedObj.GetComponent<InputField>() != null) {
				return;
			}

			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
			    Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) {
				return;
			}

			if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt)) {
				return;
			}

			if (Input.GetKeyDown(KeyCode.Comma)) {
				scrCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, CurrentRot - 15f);
			}

			if (Input.GetKeyDown(KeyCode.Period)) {

				scrCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, CurrentRot + 15f);
			}

			if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.Comma)) {
				scnEditor.instance.buttonA.transform.parent.parent.rotation = Quaternion.Euler(0f, 0f, CurrentRot);
				__instance.AddListener(__instance.buttonD, 'R');
				__instance.AddListener(__instance.buttonE, 'E');
				__instance.AddListener(__instance.buttonW, 'U');
				__instance.AddListener(__instance.buttonQ, 'Q');
				__instance.AddListener(__instance.buttonA, 'L');
				__instance.AddListener(__instance.buttonZ, 'Z');
				__instance.AddListener(__instance.buttonS, 'D');
				__instance.AddListener(__instance.buttonC, 'C');
				__instance.AddListener(__instance.buttonT, 'T');
				__instance.AddListener(__instance.buttonG, 'G');
				__instance.AddListener(__instance.buttonF, 'F');
				__instance.AddListener(__instance.buttonB, 'B');
				__instance.AddListener(__instance.buttonJ, 'J');
				__instance.AddListener(__instance.buttonH, 'H');
				__instance.AddListener(__instance.buttonN, 'N');
				__instance.AddListener(__instance.buttonM, 'M');
				__instance.invoke("UpdateFloorDirectionButtons")(true);
			}
		}

		public static Dictionary<int, Button> Buttons = new Dictionary<int, Button> { };

		public static void AddListener(this scnEditor instance, Button button, char angle) {
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => instance.CreateFloorWithShiftedCharOrAngle(0f, angle));
			//UnityModManager.Logger.Log($"{button.name} => {RotateFloor(angle, CurrentRot)}");
		}

		public static void CreateFloor(this scnEditor instance, char floorType, bool pulseFloorButtons = true,
			bool fullSpin = false) {
			instance.invoke("CreateFloor")(floorType, pulseFloorButtons, fullSpin);
		}

		[HarmonyPatch(typeof(scnEditor), "UpdateDirectionButton")]
		public static class UpdateDirectionPath {
			public static bool Prefix(scnEditor __instance, FloorDirectionButton btn, float oppositeAngle) {
				if (btn == null) {
					return false;
				}

				int num = 0;
				bool flag = false;
				switch (btn.btnType) {
					case FloorDirectionButtonType.D:
						num = 0;
						break;
					case FloorDirectionButtonType.W:
						num = 90;
						break;
					case FloorDirectionButtonType.A:
						num = 180;
						break;
					case FloorDirectionButtonType.S:
						num = 270;
						break;
					case FloorDirectionButtonType.E:
						num = 45;
						break;
					case FloorDirectionButtonType.Q:
						num = 135;
						break;
					case FloorDirectionButtonType.Z:
						num = 225;
						break;
					case FloorDirectionButtonType.C:
						num = 315;
						break;
					case FloorDirectionButtonType.Y:
						num = 60;
						break;
					case FloorDirectionButtonType.V:
						num = 240;
						break;
					case FloorDirectionButtonType.T:
						num = 120;
						break;
					case FloorDirectionButtonType.B:
						num = 300;
						break;
					case FloorDirectionButtonType.H:
						num = 150;
						break;
					case FloorDirectionButtonType.J:
						num = 30;
						break;
					case FloorDirectionButtonType.N:
						num = 210;
						break;
					case FloorDirectionButtonType.M:
						num = 330;
						break;
					case FloorDirectionButtonType.Space:
						flag = true;
						break;
					case FloorDirectionButtonType.Tab:
						flag = true;
						break;
				}

				num = Mathf.RoundToInt(num + CurrentRot).NormalizeAngle();
				btn.delete = (Mathf.Approximately(oppositeAngle, (float) num) && !flag);
				btn.gameObject.SetActive(!btn.delete || __instance.selectedFloors[0].seqID != 0);
				btn.Init();
				var transform = btn.text.transform;
				var euler = transform.eulerAngles;
				transform.eulerAngles = new Vector3(euler.x, euler.y, CurrentRot);
				btn.textShifted.transform.eulerAngles = new Vector3(euler.x, euler.y, CurrentRot);
				return false;
			}
		}

		public static void CreateFloor(this scnEditor instance, float floorAngle, bool pulseFloorButtons = true,
			bool fullSpin = false) {
			instance.invoke("CreateFloor")(floorAngle, pulseFloorButtons, fullSpin);
		}

		public static void CreateFloorWithShiftedCharOrAngle(this scnEditor instance, float angle, char chara,
			bool pulseFloorButtons = true, bool fullSpin = false) {
			chara = RotateFloor(chara, CurrentRot);
			if (scnEditor.instance.levelData.isOldLevel && chara != '?')
			{
				instance.CreateFloor(chara, pulseFloorButtons, fullSpin);
				return;
			}

			float angleFromFloorCharDirectionWithCheck = scrLevelMaker.GetAngleFromFloorCharDirectionWithCheck(chara, out bool flag);
			if (flag)
			{
				instance.CreateFloor(angleFromFloorCharDirectionWithCheck, true, false);
				return;
			}
			instance.CreateFloor(angle, pulseFloorButtons, fullSpin);
			
		}

		public static void ReplaceFloorWithShiftedCharOrAngle(this scnEditor instance, float angle, char chara,
			bool pulseFloorButtons = true, bool fullSpin = false) {
			instance.invoke("DeleteFloor")(instance.selectedFloors[0].seqID + 1, true);
			instance.invoke("CreateFloorWithCharOrAngle")(angle, RotateFloor(chara, CurrentRot), pulseFloorButtons,
				fullSpin);
		}

		public static readonly List<char> Specials = new List<char> {'!', '5', '6', '7', '8', '9'};

		public static char RotateFloor(char chara, float rotation) {
			if (Specials.Contains(chara)) return chara;
			var angle = scrLevelMaker.GetAngleFromFloorCharDirectionWithCheck(chara, out _);
			var rot = Mathf.RoundToInt(angle + rotation).NormalizeAngle();
			return Main.AngleChar[rot];
		}

		public static float NormalizeAngle(this float rot) {
			do {
				rot = (rot + 360) % 360;
			} while (rot < 0 || rot >= 360);

			return rot;
		}

		public static int NormalizeAngle(this int rot) {
			do {
				rot = (rot + 360) % 360;
			} while (rot < 0 || rot >= 360);

			return rot;
		}
	}
/*
    [HarmonyPatch(typeof(scrController), "Awake_Rewind")]
    internal static class AwakeRewindPatch {
        private static void Prefix() {
            if (Main.Settings.EnableScreenRot) {
                UpdateRotationPatch.CurrentRot = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(Input), "mousePosition", MethodType.Getter)]
    internal static class PositionPatch {
        private static void Postfix(ref Vector3 __result) {
            if (!Main.Settings.EnableScreenRot || UpdateRotationPatch.CurrentRot == 0f) {
                return;
            }

            
        }
    }*/


	[HarmonyPatch(typeof(scnEditor), "Update")]
	internal static class CorrectRotationPatch {
		public static Vector3 GetMousePositionWithAngle() {
			return CurrentAngle * Input.mousePosition;
		}

		public static Quaternion CurrentAngle => Quaternion.AngleAxis(UpdateRotationPatch.CurrentRot, Vector3.forward);

		private static Vector3 _lastPos = Vector3.zero;

		public static void Prefix() {
			if (scrController.instance.paused && Input.GetMouseButtonDown(0)) {
				_lastPos = GetMousePositionWithAngle();
			}
		}

		public static void Postfix(scnEditor __instance) {
			if (scrController.instance.paused) {
				if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0)) {
					Vector3 vector5 = (GetMousePositionWithAngle() - _lastPos) /
					                  (float) Screen.height *
					                  Camera.current.orthographicSize * 2f;
					Vector3 b5 = new Vector3(vector5.x, vector5.y);
					if (__instance.get<object>("draggedEvIndicator") != null ||
					    __instance.get<bool>("isDraggingTiles")) goto Altdrag;

					var vector6 = __instance.get<Vector3>("cameraPositionAtDragStart") - b5;
					Camera.current.transform.position =
						new Vector3(vector6.x, vector6.y, -10f);
					
					Altdrag:
					if (!__instance.get<bool>("cancelDrag") && __instance.get<EventIndicator>("draggedEvIndicator") == null)
					{
						if (__instance.SelectionIsSingle())
						{
							Vector3 vector7 = __instance.get<Dictionary<scrFloor, Vector3>>("floorPositionsAtDragStart")[__instance.selectedFloors[0]] + b5;
							__instance.selectedFloors[0].transform.position = new Vector3(vector7.x, vector7.y, __instance.selectedFloors[0].transform.position.z);
							return;
						}
						using (List<scrFloor>.Enumerator enumerator2 = __instance.selectedFloors.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								scrFloor scrFloor3 = enumerator2.Current;
								Vector3 vector8 = __instance.get<Dictionary<scrFloor, Vector3>>("floorPositionsAtDragStart")[scrFloor3] + b5;
								scrFloor3.transform.position = new Vector3(vector8.x, vector8.y, scrFloor3.transform.position.z);
							}

							return;
						}
					}
					return;
				}
			}
			
			bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			bool flag2 = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
			bool key = Input.GetKey(KeyCode.BackQuote);
			bool flag3 = flag || flag2;
			bool flag4 = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		}
	}
}