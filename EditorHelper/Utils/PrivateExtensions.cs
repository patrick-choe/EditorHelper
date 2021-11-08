using ADOFAI;
using DG.Tweening;
using EditorHelper.Tweaks.RotateScreen;
using UnityEngine;
using UnityEngine.UI;

namespace EditorHelper.Utils {
	public static class PrivateExtensions {
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

		public static void MultiSelectFloors(this scnEditor instance, scrFloor startFloor, scrFloor endFloor,
			bool setSelectPoint = false) =>
			instance.invoke<object>("MultiSelectFloors")(startFloor, endFloor, setSelectPoint);

		public static void CreateFloorWithCharOrAngle(this scnEditor instance, float angle, char chara,
			bool pulseFloorButtons = true, bool fullSpin = false) =>
			instance.invoke<object>("CreateFloor")(angle, chara, pulseFloorButtons, fullSpin);

		public static void CreateFloor(this scnEditor instance, char floorType, bool pulseFloorButtons = true, bool fullSpin = false) {
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
			if (!RotateScreenPatch.forceNotDelete && instance.FloorPointsBackwards(floorType) && !Input.GetKeyDown(KeyCode.Space) &&
			    !Input.GetKeyDown(KeyCode.Tab)) {
				if (x != null) {
					int seqID2 = scrFloor.seqID;
					if (instance.DeleteFloor(seqID2, true)) {
						instance.SelectFloor(scrLevelMaker.instance.listFloors[seqID2 - 1], true);
					}

					var floor = scrLevelMaker.instance.listFloors[seqID2 - 1];
					instance.MoveCameraToFloor(floor);
				}
			} else {
				RotateScreenPatch.forceNotDelete = false;
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
					Button? button = null;
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
			if (!RotateScreenPatch.forceNotDelete && instance.FloorPointsBackwards(floorAngle) && !Input.GetKeyDown(KeyCode.Space) &&
			    !Input.GetKeyDown(KeyCode.Tab)) {
				if (x != null) {
					int seqID2 = scrFloor.seqID;
					if (instance.DeleteFloor(seqID2, true)) {
						instance.SelectFloor(scrLevelMaker.instance.listFloors[seqID2 - 1], true);
					}

					var floor = scrLevelMaker.instance.listFloors[seqID2 - 1];
					instance.MoveCameraToFloor(floor);
				}
			} else {
				RotateScreenPatch.forceNotDelete = false;
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
					Button? button = null;
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
	}
}