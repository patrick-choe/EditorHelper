using System;
using System.Collections.Generic;
using ADOFAI;
using EditorHelper.Core.Initializer;
using EditorHelper.Core.Patch;
using EditorHelper.Tweaks.ChangeAngleByDragging;
using EditorHelper.Tweaks.RotateScreen;
using EditorHelper.Utils;
using UnityEngine;

namespace EditorHelper {
    public class Patch {
	    [TweakPatchId(nameof(scnEditor), "Update")]
		internal static class CorrectRotationPatch {
			public static Vector3 GetMousePositionWithAngle() {
				return RotateScreenPatch.CurrentAngle * Input.mousePosition;
			}

			private static Vector3 _lastPos = Vector3.zero;
			public static Vector3 LastPos2 = Vector3.zero;

			public static bool Prefix(scnEditor __instance) {
				if (ChangeAngleByDraggingPatch.UpdateFloorAnglePatch.Pressing) return true;
				if (scrController.instance.paused && Input.GetMouseButtonDown(0)) {
					_lastPos = GetMousePositionWithAngle();
					__instance.set("cameraPositionAtDragStart", LastPos2);
				}
				
				LastPos2 = Camera.current.transform.position;

				return true;
			}

			public static void Postfix(scnEditor __instance) {
				if (scrController.instance.paused) {
					if (ChangeAngleByDraggingPatch.UpdateFloorAnglePatch.Pressing) {
						Camera.current.transform.position = LastPos2;
						return;
					}

					try {

						if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0) &&
						    !__instance.get<bool>("cancelDrag")) {
							var vector5 = (GetMousePositionWithAngle() - _lastPos) /
							              Screen.height *
							              Camera.current.orthographicSize * 2f;
							var b5 = new Vector3(vector5.x, vector5.y);
							if (__instance.get<object>("draggedEvIndicator") != null ||
							    __instance.get<bool>("isDraggingTiles")) goto Altdrag;

							var vector6 = __instance.get<Vector3>("cameraPositionAtDragStart") - b5;

							Camera.current.transform.position =
								new Vector3(vector6.x, vector6.y, -10f);

							Altdrag:
							if (!__instance.get<bool>("cancelDrag") &&
							    __instance.get<EventIndicator>("draggedEvIndicator") == null) {
								try {
									if (__instance.SelectionIsSingle()) {
										var vector7 =
											__instance.get<Dictionary<scrFloor, Vector3>>("floorPositionsAtDragStart")![
												__instance.selectedFloors[0]] + b5;
										__instance.selectedFloors[0].transform.position = new Vector3(vector7.x,
											vector7.y,
											__instance.selectedFloors[0].transform.position.z);
										return;
									}

									using (var enumerator2 = __instance.selectedFloors.GetEnumerator()) {
										while (enumerator2.MoveNext()) {
											var scrFloor3 = enumerator2.Current;
											var vector8 =
												__instance.get<Dictionary<scrFloor, Vector3>>(
													"floorPositionsAtDragStart")![
													scrFloor3] + b5;
											var transform = scrFloor3.transform;
											transform.position = new Vector3(vector8.x, vector8.y,
												transform.position.z);
										}
									}
								} catch (KeyNotFoundException) { }
							}
						}
					} catch (NullReferenceException) { }
				}
			}
		}		
		
		[TweakPatchId(nameof(scnEditor), "CreateFloorWithCharOrAngle")]
		public static class CreateFloorPatch {
			public static bool Prefix(scnEditor __instance, float angle, char chara, bool pulseFloorButtons,
				bool fullSpin) {
				__instance.CreateFloorWithShiftedCharOrAngle(angle, chara, pulseFloorButtons, fullSpin);
				return false;
			}
		}

		[Init]
		public static void Init() {
			Main.Harmony.TweakPatch(typeof(CorrectRotationPatch));
			Main.Harmony.TweakPatch(typeof(CreateFloorPatch));
		}
    }
}