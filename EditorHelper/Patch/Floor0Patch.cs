using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using HarmonyLib;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace EditorHelper.Patch
{
	[HarmonyPatch(typeof(scnEditor), "Update")]
	internal static class UpdatePatch
	{
		private static readonly MethodInfo CopyFloor = typeof(scnEditor).GetMethod("CopyFloor", AccessTools.all);
		private static readonly MethodInfo MultiSelectFloors = typeof(scnEditor).GetMethod("MultiSelectFloors", AccessTools.all);

		private static void Prefix(scnEditor __instance, ref bool ___refreshBgSprites, ref bool ___refreshDecSprites)
		{
			if (!Main.Settings.EnableFloor0Events ||
				!scrController.instance.paused ||
				GCS.standaloneLevelMode ||
				StandaloneFileBrowser.lastFrameCount == Time.frameCount ||
				(Input.GetKeyDown(KeyCode.Escape) && !GCS.standaloneLevelMode && !scrController.instance.paused))
			{
				return;
			}

			var selectedObj = __instance.eventSystem.currentSelectedGameObject;

			if (selectedObj != null && selectedObj.GetComponent<InputField>() != null)
			{
				return;
			}
			
			if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.LeftCommand) && !Input.GetKey(KeyCode.RightCommand))
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				MultiSelectFloors.Invoke(__instance, new object[] { __instance.customLevel.levelMaker.listFloors.First(), __instance.customLevel.levelMaker.listFloors.Last(), true });
			}

			if (!Input.GetKeyDown(KeyCode.X) || __instance.selectedFloor == null || __instance.selectedFloor.seqID != 0)
			{
				return;
			}

			CopyFloor.Invoke(__instance, new object[] { __instance.selectedFloor, true, false });

			foreach (var ev in __instance.events.FindAll(x => x.floor == 0).Where(ev => __instance.EventHasBackgroundSprite(ev) || ev.eventType == LevelEventType.AddDecoration))
			{
				if (__instance.EventHasBackgroundSprite(ev))
				{
					___refreshBgSprites = true;
				}

				if (ev.eventType == LevelEventType.AddDecoration || ev.eventType == LevelEventType.AddText)
				{
					___refreshDecSprites = true;
				}
			}

			__instance.events.RemoveAll(x => x.floor == 0);
			__instance.ApplyEventsToFloors();
			__instance.levelEventsPanel.ShowTabsForFloor(0);
			__instance.ShowEventIndicators(__instance.selectedFloor);
		}
	}

	[HarmonyPatch(typeof(scnEditor), "OnSelectedFloorChange")]
	internal static class OnSelectedFloorChangePatch
	{
		private static readonly MethodInfo ShowEventPicker = typeof(scnEditor).GetMethod("ShowEventPicker", AccessTools.all);
		private static readonly MethodInfo UpdateFloorDirectionButtons = typeof(scnEditor).GetMethod("UpdateFloorDirectionButtons", AccessTools.all);

		private static bool Prefix(scnEditor __instance)
		{
			if (!Main.IsEnabled || !Main.Settings.EnableFloor0Events || __instance.selectedFloor == null)
			{
				return true;
			}

			__instance.levelEventsPanel.ShowTabsForFloor(__instance.selectedFloor.seqID);
			ShowEventPicker.Invoke(__instance, new object[] { true });
			UpdateFloorDirectionButtons.Invoke(__instance, new object[] { true });
			__instance.ShowEventIndicators(__instance.selectedFloor);

			return false;
		}
	}

	[HarmonyPatch(typeof(scnEditor), "AddEvent")]
	internal static class AddEventPatch
	{
		private static bool Prefix(scnEditor __instance, int floorID, LevelEventType eventType, ref bool ___refreshDecSprites)
		{
			if (!Main.IsEnabled || !Main.Settings.EnableFloor0Events)
			{
				return true;
			}

			__instance.events.Add(new LevelEvent(floorID, eventType));

			if (eventType == LevelEventType.AddDecoration || eventType == LevelEventType.AddText)
			{
				___refreshDecSprites = true;
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(scnEditor), "AddEventAtSelected")]
	internal static class AddEventAtSelectedPatch
	{
		private static bool Prefix(scnEditor __instance, LevelEventType eventType)
		{
			if (!Main.IsEnabled || !Main.Settings.EnableFloor0Events || __instance.selectedFloor == null)
			{
				return true;
			}

			__instance.SaveState();

			++__instance.editor.changingState;

			if (__instance.events.Exists(x => x.eventType == eventType && x.floor == __instance.selectedFloor.seqID))
			{
				if (Array.Exists(EditorConstants.soloTypes, element => element == eventType))
				{
					return false;
				}
			}

			__instance.AddEvent(__instance.selectedFloor.seqID, eventType);
			__instance.editor.levelEventsPanel.selectedEventType = eventType;

			var count = __instance.events.FindAll(x => x.eventType == eventType && x.floor == __instance.selectedFloor.seqID).Count;

			if (count == 1)
			{
				__instance.DecideInspectorTabsAtSelected();
				__instance.editor.levelEventsPanel.ShowPanel(eventType);
			}
			else
			{
				__instance.editor.levelEventsPanel.ShowPanel(eventType, count - 1);
			}

			__instance.ApplyEventsToFloors();
			__instance.ShowEventIndicators(__instance.selectedFloor);
			--__instance.editor.changingState;

			return false;
		}
	}

	[HarmonyPatch(typeof(scnEditor), "PasteEvents")]
	internal static class PasteEventsPatch
	{
		private static readonly MethodInfo CopyEvent = typeof(scnEditor).GetMethod("CopyEvent", AccessTools.all);
		private static readonly MethodInfo SelectFloor = typeof(scnEditor).GetMethod("SelectFloor", AccessTools.all);

		private static bool Prefix(scnEditor __instance, scrFloor targetFloor, IReadOnlyList<LevelEvent> eventsList, bool overwrite, bool selectAfterward, ref bool ___refreshBgSprites,
			ref bool ___refreshDecSprites)
		{
			if (!Main.IsEnabled || !Main.Settings.EnableFloor0Events || !eventsList.Any())
			{
				return true;
			}

			if (overwrite)
			{
				__instance.events.RemoveAll(x => x.floor == targetFloor.seqID);
			}

			foreach (var ev in eventsList.Where(ev => !EditorConstants.soloTypes.Contains(ev.eventType) || !__instance.events.Exists(x => x.floor == targetFloor.seqID && x.eventType == ev.eventType)))
			{
				if (__instance.EventHasBackgroundSprite(ev))
				{
					___refreshBgSprites = true;
				}

				if (ev.eventType == LevelEventType.AddDecoration || ev.eventType == LevelEventType.AddText)
				{
					___refreshDecSprites = true;
				}

				__instance.events.Add(CopyEvent.Invoke(__instance, new object[] { ev, targetFloor.seqID }) as LevelEvent);
			}

			__instance.ApplyEventsToFloors();

			if (!selectAfterward)
			{
				return false;
			}

			var eventType = eventsList[0].eventType;

			SelectFloor.Invoke(__instance, new object[] { targetFloor, true });
			__instance.levelEventsPanel.ShowTabsForFloor(__instance.selectedFloor.seqID);
			__instance.editor.levelEventsPanel.selectedEventType = eventType;
			__instance.editor.levelEventsPanel.ShowPanel(eventType);
			__instance.ShowEventIndicators(targetFloor);

			return false;
		}
	}
}
