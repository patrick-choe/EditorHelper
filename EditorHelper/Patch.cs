using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ADOFAI;
using EditorHelper.Core.Initializer;
using EditorHelper.Core.Patch;
using EditorHelper.Core.Tweaks;
using EditorHelper.Tweaks.ChangeAngleByDragging;
using EditorHelper.Tweaks.Miscellaneous;
using EditorHelper.Tweaks.RemoveEditorLimits;
using EditorHelper.Tweaks.RotateScreen;
using EditorHelper.Utils;
using HarmonyLib;
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
		
		[TweakPatchId(nameof(PropertyControl_File), "ProcessFile")]
		public static class AllowMp3Patch {
			public static bool Prefix(PropertyControl_File __instance, string? newFilename, FileType fileType) {
				if (!string.IsNullOrEmpty(newFilename) && string.IsNullOrEmpty(__instance.levelPath) ||
				    newFilename == null || __instance.get<string>("filename") == newFilename) {
					return true;
				}

				if (fileType == FileType.Audio) {
					LevelEvent selectedEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
					__instance.set("filename", newFilename);
					__instance.ToggleOthersEnabled();
					if (Path.GetExtension(newFilename).Replace(".", string.Empty) == "mp3" && !TweakManager.Setting<RemoveEditorLimitsTweak, RemoveEditorLimitsSetting>()!.AllowMp3) {
						scnEditor.instance.songToConvert = newFilename;
						scnEditor.instance.ShowPopup(true, scnEditor.PopupType.OggEncode);
						return false;
					}
					selectedEvent[__instance.propertyInfo.name] = newFilename;
					__instance.inputField.text = newFilename;
					__instance.editor.UpdateSongAndLevelSettings();

					if (!TweakManager.Setting<MiscellaneousTweak, MiscellaneousSetting>()!.DetectBpm) return false;
					string directoryName = Path.GetDirectoryName(scnEditor.instance.levelPath) ?? "";
					var path = Path.Combine(directoryName, newFilename);
					DetectBpm(path);
					return false;
				}

				return true;
			}
			
			
			public static void DetectBpm(string path) {
				scnEditor.instance.StartCoroutine(DetectBpmCo(path));
			}

			public static IEnumerator DetectBpmCo(string path) {
				BPMDetector detect = null!;
				var asyncResult = Misc.RunAsync(() => { detect = new BPMDetector(path); });
				while (!asyncResult.IsCompleted) {
					scnEditor.instance.settingsPanel.panelsList[0].properties["bpm"].control.text = "Analyzing...";
					yield return null;
				}

				scnEditor.instance.levelData.songSettings["bpm"] = (float) detect.Groups[0].Tempo;
				scnEditor.instance.UpdateSongAndLevelSettings();
			}
		}
		/*
		[TweakPatchId(nameof(FloorMeshRenderer), "SetLength")]
		public static class LengthPatch {
			public static void Postfix(FloorMeshRenderer __instance, float length) {
				__instance.floorMesh._length *= (1f / 1.5f);
			}
		}
		
		[TweakPatchId(nameof(FloorMeshRenderer), "Awake")]
		public static class LengthPatch2 {
			public static void Postfix(FloorMeshRenderer __instance, float length) {
				__instance.floorMesh._length *= (1f / 1.5f);
			}
		}
		
		[TweakPatchId(nameof(scrController), "startRadius", MethodType.Getter)]
		public static class RadiusPatch {
			public static void Postfix(out float __result) {
				if (!scrController.instance.isbigtiles) {
					__result = 1f;
					return;
				}
				__result = 1f;
			}
		}*/

		[Init]
		public static void Init() {
			Main.Harmony.TweakPatch(typeof(CorrectRotationPatch));
			Main.Harmony.TweakPatch(typeof(CreateFloorPatch));
			Main.Harmony.TweakPatch(typeof(AllowMp3Patch));
			//Main.Harmony.TweakPatch(typeof(LengthPatch));
			//Main.Harmony.TweakPatch(typeof(LengthPatch2));
			//Main.Harmony.TweakPatch(typeof(RadiusPatch));
		}
    }
}