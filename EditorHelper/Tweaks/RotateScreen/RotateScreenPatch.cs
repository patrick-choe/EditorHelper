using System.Collections.Generic;
using ADOFAI;
using EditorHelper.Core.Patch;
using EditorHelper.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Tweaks.RotateScreen {
	public abstract class RotateScreenPatch {
		
		public static bool forceNotDelete {
			get => _forceNotDelete;
			set {
				UnityModManager.Logger.Log($"Force: {value}");
				_forceNotDelete = value;
			}
		}

		private static bool _forceNotDelete;
		internal static float CurrentRot => Camera.current.transform.eulerAngles.z;
		public static Quaternion CurrentAngle => Quaternion.AngleAxis(CurrentRot, Vector3.forward);

		[TweakPatchId(nameof(scnEditor), "Update")]
		internal static class UpdateRotationPatch {

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

			public static Dictionary<int, Button> Buttons = new();
		}

		[TweakPatchId(nameof(scnEditor), "UpdateDirectionButton")]
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

				num = Mathf.RoundToInt(num + CurrentRot).NormalizeAngle();
				btn.delete = (Mathf.Approximately(oppositeAngle, num) && !flag);
				btn.gameObject.SetActive(!btn.delete || __instance.selectedFloors[0].seqID != 0);
				btn.Init();
				var transform = btn.text.transform;
				var euler = transform.eulerAngles;
				transform.eulerAngles = new Vector3(euler.x, euler.y, CurrentRot);
				btn.textShifted.transform.eulerAngles = new Vector3(euler.x, euler.y, CurrentRot);
				return false;
			}
		}


		[TweakPatchId(nameof(scnEditor), "SwitchToEditMode")]
		public static class ResetPatch {
			public static void Postfix() {
				Camera.current.transform.eulerAngles = new Vector3(0, 0, PlayPatch.Rotation);
				UpdateRotationPatch.UpdateDirectionButtonsRot(scnEditor.instance);
			}
		}


		[TweakPatchId(nameof(scnEditor), "Play")]
		public static class PlayPatch {
			public static float Rotation;

			public static void Prefix() {
				Rotation = Camera.current.transform.eulerAngles.z;
			}
		}
	}

	public static class RotateScreenHelper {
		public static void AddListener(this scnEditor instance, Button button, char angle) {
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => instance.CreateFloorWithShiftedCharOrAngle(0f, angle));
			//UnityModManager.Logger.Log($"{button.name} => {Angle.RotateFloor(angle, CurrentRot)}");
		}

		public static UnityAction call = () => {
			UnityModManager.Logger.Log("ForceNotDelete");
			RotateScreenPatch.forceNotDelete = true;
		};

		public static void AddSpaceEscape(this scnEditor instance) {
			instance.buttonSpace.onClick.RemoveAllListeners();
			instance.buttonSpace.onClick.AddListener(call);
			instance.buttonSpace.onClick.AddListener(
				() => instance.invoke("CreateFloorWithCharOrAngle")(0f,
					(Angle.RotateFloor(scrLevelMaker.instance.leveldata[instance.selectedFloors[0].seqID], 180)),
					true,
					false));
		}

		public static void ResetListener(this scnEditor instance, Button button, char angle) {
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => instance.invoke("CreateFloorWithCharOrAngle")(0f, angle, true, false));
			//UnityModManager.Logger.Log($"{button.name} => {Angle.RotateFloor(angle, CurrentRot)}");
		}
		
		
		public static void CreateFloorWithShiftedCharOrAngle(this scnEditor instance, float angle, char chara,
			bool pulseFloorButtons = true, bool fullSpin = false) {
			if (!Input.GetKeyDown(KeyCode.Space))
				chara = Angle.RotateFloor(chara, RotateScreenPatch.CurrentRot);
			if (scnEditor.instance.levelData.isOldLevel && chara != '?') {
				instance.CreateFloor(chara, pulseFloorButtons, fullSpin);
				return;
			}

			float angleFromFloorCharDirectionWithCheck =
				scrLevelMaker.GetAngleFromFloorCharDirectionWithCheck(chara, out bool flag);
			if (flag) {
				instance.CreateFloor(angleFromFloorCharDirectionWithCheck, true, false);
				return;
			}

			instance.CreateFloor(angle, pulseFloorButtons, fullSpin);

		}

		public static void ReplaceFloorWithShiftedCharOrAngle(this scnEditor instance, float angle, char chara,
			bool pulseFloorButtons = true, bool fullSpin = false) {
			instance.invoke("DeleteFloor")(instance.selectedFloors[0].seqID + 1, true);
			instance.invoke("CreateFloorWithCharOrAngle")(angle, Angle.RotateFloor(chara, RotateScreenPatch.CurrentRot),
				pulseFloorButtons,
				fullSpin);
		}
	}
}