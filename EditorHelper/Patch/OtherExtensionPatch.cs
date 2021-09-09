using System;
using System.Collections;
using System.IO;
using ADOFAI;
using HarmonyLib;
using MoreEditorOptions.Util;
using NAudio.Wave;
using NAudioBPM;
using RDTools;
using UnityEngine;
using UnityEngine.Video;
using UnityModManagerNet;

namespace EditorHelper.Patch {
	[HarmonyPatch(typeof(PropertyControl_File), "BrowseFile")]
	public class OtherExtensionPatch {
		public static bool Prefix(PropertyControl_File __instance) {
			if (!string.IsNullOrEmpty(__instance.levelPath)) {
				if (__instance.propertyInfo.name == "songFilename") {
					string text = RDEditorUtils.ShowFileSelectorForAudio("Import Song", -1L);
					if (!text.IsNullOrEmpty()) {
						string fileName = Path.GetFileName(text);
						if (__instance.get<string>("filename") == fileName) {
							RDBaseDll.printesw("Old filename and new one are the same.");
							return false;
						}

						LevelEvent selectedEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
						__instance.set("filename", fileName);
						__instance.ToggleOthersEnabled();
						var extension = Path.GetExtension(fileName).Replace(".", string.Empty).ToLower();
						var allowed = extension == "ogg" || Main.Settings.AllowOtherSongTypes && (
							Main.Settings.AllowMP3 && extension == "mp3" ||
							Main.Settings.AllowWAV && extension == "wav"
						);
						if (!allowed) {
							__instance.editor.songToConvert = text;
							__instance.editor.ShowPopup(true, scnEditor.PopupType.OggEncode, false);
							return false;
						}

						string directoryName =
							Path.GetDirectoryName(scnEditor.instance.levelPath) ?? "fuck you damn bug";
						var path = Path.Combine(directoryName, text);
						DetectBpmAndOffset(path);

						selectedEvent[__instance.propertyInfo.name] = __instance.get<string>("filename");
						__instance.inputField.text = __instance.get<string>("filename");
						__instance.editor.UpdateSongAndLevelSettings();

						return false;
					}
				}
				return true; /*if (__instance.propertyInfo.name == "bgImage" ||
				           __instance.propertyInfo.name == "previewImage" ||
				           __instance.propertyInfo.name == "previewIcon" ||
				           __instance.propertyInfo.name == "artistPermission" ||
				           __instance.propertyInfo.name == "decorationImage") {
					string text2 = RDEditorUtils.ShowFileSelectorForImage("Import Image", -1L);
					if (!text2.IsNullOrEmpty()) {
						string fileName2 = Path.GetFileName(text2);
						if (__instance.get<string>("filename") == fileName2) {
							RDBaseDll.printesw("Old filename and new one are the same.");
							return false;
						}

						LevelEvent selectedEvent2 = __instance.propertiesPanel.inspectorPanel.selectedEvent;
						__instance.set("filename", fileName2);
						selectedEvent2[__instance.propertyInfo.name] = __instance.get<string>("filename");
						__instance.inputField.text = __instance.get<string>("filename");
						__instance.ToggleOthersEnabled();
						if (selectedEvent2.eventType == LevelEventType.BackgroundSettings) {
							__instance.customLevel.SetBackground();
							return false;
						}

						if (selectedEvent2.eventType == LevelEventType.AddDecoration) {
							__instance.editor.UpdateDecorationSprites();
							return false;
						}

						__instance.customLevel.UpdateBackgroundSprites();
						return false;
					}
				} else if (__instance.propertyInfo.name == "bgVideo") {
					string text3 = RDEditorUtils.ShowFileSelectorForVideo("Import Video", -1L);
					if (!text3.IsNullOrEmpty()) {
						string fileName3 = Path.GetFileName(text3);
						if (__instance.get<string>("filename") != fileName3) {
							LevelEvent selectedEvent3 = __instance.propertiesPanel.inspectorPanel.selectedEvent;
							__instance.set("filename", fileName3);
							selectedEvent3[__instance.propertyInfo.name] = __instance.get<string>("filename");
							__instance.inputField.text = __instance.get<string>("filename");
							VideoPlayer videoBG = __instance.customLevel.videoBG;
							videoBG.gameObject.SetActive(true);
							videoBG.url = Path.Combine(Path.GetDirectoryName(__instance.levelPath),
								__instance.editor.levelData.miscSettings.data["bgVideo"].ToString());
							videoBG.Prepare();
							videoBG.Stop();
							__instance.ToggleOthersEnabled();
							return false;
						}

						RDBaseDll.printesw("Old filename and new one are the same.");
						return false;
					}*/
			}

			return false;
		}

		public static void DetectBpmAndOffset(string path) {
			if (Main.Settings.DetectOffsetOnLoadSong)
				scnEditor.instance.StartCoroutine(DetectOffsetCo(path, null));
			if (Main.Settings.DetectBpmOnLoadSong)
				DetectBpm(path);
		}

		public static void DetectBpm(string path) {
			var detect = new BPMDetector(path);
			scnEditor.instance.levelData.songSettings["bpm"] = (float) detect.Groups[0].Tempo;
		}
		
		public static IEnumerator DetectOffsetCo(string path, PropertyControl_Text controlText) {
			UnityModManager.Logger.Log("Detecting offset");
			string filename = Path.GetFileName(path);
			int num = 0;
			yield return AudioManager.Instance.FindOrLoadAudioClipExternal(path, false, 0f);
			UnityModManager.Logger.Log("Loaded clip");
			var clip = AudioManager.Instance.audioLib[filename + "*external"];
			float[] array = new float[clip.samples];
			var yieldDuration = array.Length / 60;
			clip.GetData(array, 0);
			for (int i = 0; i < array.Length; i += clip.channels)
			{
				if (array[i] != 0f)
				{
					float num3 = (float)i;
					float num4 = (float)clip.channels;
					float num5 = (float)clip.frequency;
					num = (int)Convert.ToInt16(num3 / num4 / num5 * 1000f);
					break;
				}

				if (i % yieldDuration == 0) yield return null;
			}
			
			UnityModManager.Logger.Log($"Offset: {num}");
			scnEditor.instance.levelData.songSettings["offset"] = (int) num;
			if (controlText != null) {
				controlText.text = num.ToString();
			}
			
			scnEditor.instance.UpdateSongAndLevelSettings();
		}
	}
}