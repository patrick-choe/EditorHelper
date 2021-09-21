using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using DG.Tweening;
using EditorHelper.Components;
using EditorHelper.Utils;
using HarmonyLib;
using MoreEditorOptions.Util;
using SFB;
using UnityEngine;
using UnityEngine.Events;
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
			
			if (Main.Settings.RotateScreenCW.Check) {
				scrCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, CurrentRot - 15f);
				UpdateDirectionButtonsRot(__instance);
			}

			if (Main.Settings.RotateScreenCCW.Check) {
				scrCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, CurrentRot + 15f);
				UpdateDirectionButtonsRot(__instance);
			}
		}

		public static bool DeleteFloor(this scnEditor instance, int sequenceIndex, bool remakePath = true) =>
			instance.invoke<bool>("DeleteFloor")(sequenceIndex, remakePath);
		public static scrFloor PreviousFloor(this scnEditor instance, scrFloor floor) =>
			instance.invoke<scrFloor>("PreviousFloor")(floor);
		
		public static bool FloorPointsBackwards(this scnEditor instance, float floorAngle) =>
			instance.invoke<bool>("FloorPointsBackwards")(floorAngle);
		public static bool FloorPointsBackwards(this scnEditor instance, char floorType) =>
			instance.invoke<bool>("FloorPointsBackwards")(floorType);
		public static void SelectFloor(this scnEditor instance, scrFloor floorToSelect, bool cameraJump = true) =>
			instance.invoke<object>("SelectFloor")(floorToSelect, cameraJump);
		public static void MoveCameraToFloor(this scnEditor instance, scrFloor floor) =>
			instance.invoke<object>("MoveCameraToFloor")(floor);
		
		
		public static void ResetDirectionButtonsRot(scnEditor instance) {
			scnEditor.instance.buttonA.transform.parent.parent.rotation = Quaternion.Euler(0f, 0f, 0);
			instance.ResetListener(instance.buttonD, 'R');
			instance.ResetListener(instance.buttonE, 'E');
			instance.ResetListener(instance.buttonW, 'U');
			instance.ResetListener(instance.buttonQ, 'Q');
			instance.ResetListener(instance.buttonA, 'L');
			instance.ResetListener(instance.buttonZ, 'Z');
			instance.ResetListener(instance.buttonS, 'D');
			instance.ResetListener(instance.buttonC, 'C');
			instance.ResetListener(instance.buttonT, 'T');
			instance.ResetListener(instance.buttonG, 'G');
			instance.ResetListener(instance.buttonF, 'F');
			instance.ResetListener(instance.buttonB, 'B');
			instance.ResetListener(instance.buttonJ, 'J');
			instance.ResetListener(instance.buttonH, 'H');
			instance.ResetListener(instance.buttonN, 'N');
			instance.ResetListener(instance.buttonM, 'M');
			instance.AddSpaceEscape();
			instance.invoke("UpdateFloorDirectionButtons")(true);
		}
		
		public static void UpdateDirectionButtonsRot(scnEditor instance) {
			scnEditor.instance.buttonA.transform.parent.parent.rotation = Quaternion.Euler(0f, 0f, CurrentRot);
			instance.AddListener(instance.buttonD, 'R');
			instance.AddListener(instance.buttonE, 'E');
			instance.AddListener(instance.buttonW, 'U');
			instance.AddListener(instance.buttonQ, 'Q');
			instance.AddListener(instance.buttonA, 'L');
			instance.AddListener(instance.buttonZ, 'Z');
			instance.AddListener(instance.buttonS, 'D');
			instance.AddListener(instance.buttonC, 'C');
			instance.AddListener(instance.buttonT, 'T');
			instance.AddListener(instance.buttonG, 'G');
			instance.AddListener(instance.buttonF, 'F');
			instance.AddListener(instance.buttonB, 'B');
			instance.AddListener(instance.buttonJ, 'J');
			instance.AddListener(instance.buttonH, 'H');
			instance.AddListener(instance.buttonN, 'N');
			instance.AddListener(instance.buttonM, 'M');
			instance.AddSpaceEscape();
			instance.invoke("UpdateFloorDirectionButtons")(true);
		}

		public static Dictionary<int, Button> Buttons = new Dictionary<int, Button> { };

		public static void AddListener(this scnEditor instance, Button button, char angle) {
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => instance.CreateFloorWithShiftedCharOrAngle(0f, angle));
			//UnityModManager.Logger.Log($"{button.name} => {Angle.RotateFloor(angle, CurrentRot)}");
		}

		public static UnityAction call = () => {
			UnityModManager.Logger.Log("ForceNotDelete");
			UpdateRotationPatch.forceNotDelete = true;
		};

		public static void AddSpaceEscape(this scnEditor instance) {
			instance.buttonSpace.onClick.RemoveAllListeners();
			instance.buttonSpace.onClick.AddListener(call);
			instance.buttonSpace.onClick.AddListener(
				() => instance.invoke("CreateFloorWithCharOrAngle")(0f,
					(Angle.RotateFloor(scrLevelMaker.instance.leveldata[instance.selectedFloors[0].seqID], 180)), true,
					false));
		}

		public static void ResetListener(this scnEditor instance, Button button, char angle) {
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => instance.invoke("CreateFloorWithCharOrAngle")(0f, angle, true, false));
			//UnityModManager.Logger.Log($"{button.name} => {Angle.RotateFloor(angle, CurrentRot)}");
		}

		public static bool forceNotDelete {
			get => _forceNotDelete;
			set {
				UnityModManager.Logger.Log($"Force: {value}");
				_forceNotDelete = value;
			}
		}
		private static bool _forceNotDelete;

		public static void CreateFloor(this scnEditor instance, char floorType, bool pulseFloorButtons = true,
			bool fullSpin = false) {
			if (!instance.SelectionIsSingle()) {
				return;
			}

			var scrFloor = instance.selectedFloors[0];
			if (fullSpin && scrFloor.seqID == 0) {
				return;
			}

			instance.SaveState(true, false);
			instance.changingState++;
			int seqID = scrFloor.seqID;
			var x = instance.PreviousFloor(scrFloor);
			float num = scrLevelMaker.GetAngleFromFloorCharDirection(floorType) % 360f;
			double num2 = scrFloor.entryangle * 57.295780181884766;
			float num3 = Mathf.Abs(450f - (float) num2) % 360f;
			bool flag =
				scrMisc.ApproximatelyFloor(
					scrMisc.GetAngleMoved(scrFloor.entryangle, scrFloor.exitangle, !scrFloor.isCCW),
					6.2831854820251465) || scrMisc.ApproximatelyFloor(scrFloor.entryangle, scrFloor.exitangle);
			if (!forceNotDelete && instance.FloorPointsBackwards(floorType) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Tab)) {
				if (x != null) {
					int seqID2 = scrFloor.seqID;
					if (instance.DeleteFloor(seqID2, true)) {
						instance.SelectFloor(scrLevelMaker.instance.listFloors[seqID2 - 1], true);
					}

					var floor = scrLevelMaker.instance.listFloors[seqID2 - 1];
					instance.MoveCameraToFloor(floor);
				}
			} else {
				forceNotDelete = false;
				foreach (var levelEvent in instance.events) {
					if (levelEvent.floor > seqID) {
						levelEvent.floor++;
						if (levelEvent.eventType == LevelEventType.AddDecoration ||
						    levelEvent.eventType == LevelEventType.AddText) {
							instance.set("refreshDecSprites", true);
						}
					}
				}

				instance.InsertCharFloor(seqID, floorType);
				var scrFloor2 = scrLevelMaker.instance.listFloors[seqID + 1];
				instance.SelectFloor(scrFloor2, true);
				instance.MoveCameraToFloor(scrFloor2);
				if (pulseFloorButtons) {
					Button button = null;
					switch (floorType) {
						case 'B':
							button = instance.buttonB;
							break;
						case 'C':
							button = instance.buttonC;
							break;
						case 'D':
							button = instance.buttonS;
							break;
						case 'E':
							button = instance.buttonE;
							break;
						case 'H':
							button = instance.buttonH;
							break;
						case 'J':
							button = instance.buttonJ;
							break;
						case 'L':
							button = instance.buttonA;
							break;
						case 'M':
							button = instance.buttonM;
							break;
						case 'N':
							button = instance.buttonN;
							break;
						case 'Q':
							button = instance.buttonQ;
							break;
						case 'R':
							button = instance.buttonD;
							break;
						case 'T':
							button = instance.buttonT;
							break;
						case 'U':
							button = instance.buttonW;
							break;
						case 'V':
							button = instance.buttonF;
							break;
						case 'Y':
							button = instance.buttonG;
							break;
						case 'Z':
							button = instance.buttonZ;
							break;
					}

					if (button != null) {
						var endValue = new Vector3(1f, 1f);
						button.transform.DOKill(false);
						button.transform.ScaleXY(instance.floorButtonPulseSize);
						button.transform.DOScale(endValue, instance.floorButtonPulseDuration).SetUpdate(true)
							.SetEase(Ease.OutQuad);
					}
				}
			}

			instance.changingState--;
		}

		public static void CreateFloor(this scnEditor instance, float floorAngle, bool pulseFloorButtons = true,
			bool fullSpin = false) {
			if (!instance.SelectionIsSingle()) {
				return;
			}

			var scrFloor = instance.selectedFloors[0];
			if (fullSpin && scrFloor.seqID == 0) {
				return;
			}

			instance.SaveState(true, false);
			instance.changingState++;
			int seqID = scrFloor.seqID;
			var x = instance.PreviousFloor(scrFloor);
			float num = floorAngle % 360f;
			double num2 = scrFloor.entryangle * 57.295780181884766;
			float num3 = Mathf.Abs(450f - (float) num2) % 360f;
			bool flag =
				scrMisc.ApproximatelyFloor(
					scrMisc.GetAngleMoved(scrFloor.entryangle, scrFloor.exitangle, !scrFloor.isCCW),
					6.2831854820251465) || scrMisc.ApproximatelyFloor(scrFloor.entryangle, scrFloor.exitangle);
			if (!forceNotDelete && instance.FloorPointsBackwards(floorAngle) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Tab)) {
				if (x != null) {
					int seqID2 = scrFloor.seqID;
					if (instance.DeleteFloor(seqID2, true)) {
						instance.SelectFloor(scrLevelMaker.instance.listFloors[seqID2 - 1], true);
					}

					var floor = scrLevelMaker.instance.listFloors[seqID2 - 1];
					instance.MoveCameraToFloor(floor);
				}
			} else {
				forceNotDelete = false;
				foreach (var levelEvent in instance.events) {
					if (levelEvent.floor > seqID) {
						levelEvent.floor++;
						if (levelEvent.eventType == LevelEventType.AddDecoration ||
						    levelEvent.eventType == LevelEventType.AddText) {
							instance.set("refreshDecSprites", true);
						}
					}
				}

				instance.InsertFloatFloor(seqID, floorAngle);
				var scrFloor2 = scrLevelMaker.instance.listFloors[seqID + 1];
				instance.SelectFloor(scrFloor2, true);
				instance.MoveCameraToFloor(scrFloor2);
				if (pulseFloorButtons) {
					Button button = null;
					if (!0f.Equals(floorAngle)) {
						if (!90f.Equals(floorAngle)) {
							if (!180f.Equals(floorAngle)) {
								if (!270f.Equals(floorAngle)) {
									if (!45f.Equals(floorAngle)) {
										if (!135f.Equals(floorAngle)) {
											if (!225f.Equals(floorAngle)) {
												if (!315f.Equals(floorAngle)) {
													if (!285f.Equals(floorAngle)) {
														if (!60f.Equals(floorAngle)) {
															if (!255f.Equals(floorAngle)) {
																if (!300f.Equals(floorAngle)) {
																	if (!150f.Equals(floorAngle)) {
																		if (!30f.Equals(floorAngle)) {
																			if (!330f.Equals(floorAngle)) {
																				if (210f.Equals(floorAngle)) {
																					button = instance.buttonN;
																				}
																			} else {
																				button = instance.buttonM;
																			}
																		} else {
																			button = instance.buttonJ;
																		}
																	} else {
																		button = instance.buttonH;
																	}
																} else {
																	button = instance.buttonB;
																}
															} else {
																button = instance.buttonF;
															}
														} else {
															button = instance.buttonT;
														}
													} else {
														button = instance.buttonG;
													}
												} else {
													button = instance.buttonC;
												}
											} else {
												button = instance.buttonZ;
											}
										} else {
											button = instance.buttonQ;
										}
									} else {
										button = instance.buttonE;
									}
								} else {
									button = instance.buttonS;
								}
							} else {
								button = instance.buttonA;
							}
						} else {
							button = instance.buttonW;
						}
					} else {
						button = instance.buttonD;
					}

					if (button != null) {
						var endValue = new Vector3(1f, 1f);
						button.transform.DOKill(false);
						button.transform.ScaleXY(instance.floorButtonPulseSize);
						button.transform.DOScale(endValue, instance.floorButtonPulseDuration).SetUpdate(true)
							.SetEase(Ease.OutQuad);
					}
				}
			}

			instance.changingState--;
		}

		public static void CreateFloorWithShiftedCharOrAngle(this scnEditor instance, float angle, char chara,
			bool pulseFloorButtons = true, bool fullSpin = false) {
			if (!Input.GetKeyDown(KeyCode.Space))
				chara = Angle.RotateFloor(chara, CurrentRot);
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
			instance.invoke("CreateFloorWithCharOrAngle")(angle, Angle.RotateFloor(chara, CurrentRot), pulseFloorButtons,
				fullSpin);
		}
	}
	
	[HarmonyPatch(typeof(scnEditor), "UpdateDirectionButton")]
		public static class UpdateDirectionPatch {
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
						num = Input.GetKey(KeyCode.BackQuote) ? 75 : 60;
						break;
					case FloorDirectionButtonType.V:
						num = Input.GetKey(KeyCode.BackQuote) ? 255 : 240;
						break;
					case FloorDirectionButtonType.T:
						num = Input.GetKey(KeyCode.BackQuote) ? 105 : 120;
						break;
					case FloorDirectionButtonType.B:
						num = Input.GetKey(KeyCode.BackQuote) ? 285 : 300;
						break;
					case FloorDirectionButtonType.H:
						num = Input.GetKey(KeyCode.BackQuote) ? 165 : 150;
						break;
					case FloorDirectionButtonType.J:
						num = Input.GetKey(KeyCode.BackQuote) ? 15 : 30;
						break;
					case FloorDirectionButtonType.N:
						num = Input.GetKey(KeyCode.BackQuote) ? 75 : 210;
						break;
					case FloorDirectionButtonType.M:
						num = Input.GetKey(KeyCode.BackQuote) ? 345 : 330;
						break;
					case FloorDirectionButtonType.Space:
						flag = true;
						break;
					case FloorDirectionButtonType.Tab:
						flag = true;
						break;
				}

				num = Mathf.RoundToInt(num + UpdateRotationPatch.CurrentRot).NormalizeAngle();
				btn.delete = (Mathf.Approximately(oppositeAngle, (float) num) && !flag);
				btn.gameObject.SetActive(!btn.delete || __instance.selectedFloors[0].seqID != 0);
				btn.Init();
				var transform = btn.text.transform;
				var euler = transform.eulerAngles;
				transform.eulerAngles = new Vector3(euler.x, euler.y, UpdateRotationPatch.CurrentRot);
				btn.textShifted.transform.eulerAngles = new Vector3(euler.x, euler.y, UpdateRotationPatch.CurrentRot);
				return false;
			}
		}


	[HarmonyPatch(typeof(scnEditor), "SwitchToEditMode")]
	public static class ResetPatch {
		public static void Postfix() {
			Camera.current.transform.eulerAngles = new Vector3(0, 0, PlayPatch.Rotation);
			UpdateRotationPatch.UpdateDirectionButtonsRot(scnEditor.instance);
		}
	}

	
	[HarmonyPatch(typeof(scnEditor), "Play")]
	public static class PlayPatch {
		public static float Rotation;
		public static void Prefix() {
			Rotation = Camera.current.transform.eulerAngles.z;
		}
	}

	[HarmonyPatch(typeof(scnEditor), "Update")]
	internal static class CorrectRotationPatch {
		public static Vector3 GetMousePositionWithAngle() {
			return CurrentAngle * Input.mousePosition;
		}

		public static Quaternion CurrentAngle => Quaternion.AngleAxis(UpdateRotationPatch.CurrentRot, Vector3.forward);

		private static Vector3 _lastPos = Vector3.zero;
		public static Vector3 LastPos2 = Vector3.zero;

		public static bool Prefix(scnEditor __instance) {
			if (scrController.instance.paused && Input.GetMouseButtonDown(0)) {
				_lastPos = GetMousePositionWithAngle();
				__instance.set("cameraPositionAtDragStart", LastPos2);
			}


			if (!EditorHelperPanel.ShowGui) return true;
			if (Input.GetMouseButtonUp(0) && EditorHelperPanel.IsDragging || (Input.mouseScrollDelta.y != 0 || Input.GetMouseButtonDown(0)) && EditorHelperPanel.Contains) {
				var vector6 = LastPos2;
				Camera.current.transform.position = new Vector3(vector6.x, vector6.y, -10f);
				return false;
			} else {
				LastPos2 = Camera.current.transform.position;
			}

			return true;
		}

		public static void Postfix(scnEditor __instance) {
			if (scrController.instance.paused) {
				if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0) && Main.Settings.ChangeTileAngle.Check) {
					var vector6 = LastPos2;
					Camera.current.transform.position = new Vector3(vector6.x, vector6.y, -10f);
					return;
				}
				if (!Input.GetMouseButtonDown(0) && 
				    Input.GetMouseButton(0) &&
				    !__instance.get<bool>("cancelDrag")) {
					Vector3 vector6;
					var b5 = Vector3.zero;
					if (EditorHelperPanel.IsDragging) {
						vector6 = __instance.get<Vector3>("cameraPositionAtDragStart");
					} else {
						var vector5 = (GetMousePositionWithAngle() - _lastPos) /
						              (float) Screen.height *
						              Camera.current.orthographicSize * 2f;
						b5 = new Vector3(vector5.x, vector5.y);
						if (__instance.get<object>("draggedEvIndicator") != null ||
						    __instance.get<bool>("isDraggingTiles")) goto Altdrag;

						vector6 = __instance.get<Vector3>("cameraPositionAtDragStart") - b5;
					}
					Camera.current.transform.position =
						new Vector3(vector6.x, vector6.y, -10f);
					
					Altdrag:
					if (!__instance.get<bool>("cancelDrag") && __instance.get<EventIndicator>("draggedEvIndicator") == null)
					{
						try {
							if (__instance.SelectionIsSingle()) {
								var vector7 =
									__instance.get<Dictionary<scrFloor, Vector3>>("floorPositionsAtDragStart")[
										__instance.selectedFloors[0]] + b5;
								__instance.selectedFloors[0].transform.position = new Vector3(vector7.x, vector7.y,
									__instance.selectedFloors[0].transform.position.z);
								return;
							}

							using (var enumerator2 = __instance.selectedFloors.GetEnumerator()) {
								while (enumerator2.MoveNext()) {
									var scrFloor3 = enumerator2.Current;
									var vector8 =
										__instance.get<Dictionary<scrFloor, Vector3>>("floorPositionsAtDragStart")[
											scrFloor3] + b5;
									scrFloor3.transform.position = new Vector3(vector8.x, vector8.y,
										scrFloor3.transform.position.z);
								}

								return;
							}
						} catch (KeyNotFoundException) { }
					}
				}
			}
		}
	}
}