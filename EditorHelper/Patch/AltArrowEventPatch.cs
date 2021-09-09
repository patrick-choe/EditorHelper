using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using HarmonyLib;
using MoreEditorOptions.Util;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper.Patch {
	public static class Inspectors {
		public static LevelEventType EventType => scnEditor.instance.levelEventsPanel.selectedEventType;
		public static int EventNum => scnEditor.instance.levelEventsPanel.EventNumOfTab(EventType);
	}

	[HarmonyPatch(typeof(scnEditor), "Update")]
	public class RightClickToGenerate15Patch {
		public static LevelEventType[] EventsOrder = {
			LevelEventType.EditorComment,
			LevelEventType.SetSpeed,
			LevelEventType.Twirl,
			LevelEventType.Checkpoint,
			LevelEventType.CustomBackground,
			LevelEventType.ChangeTrack,
			LevelEventType.ColorTrack,
			LevelEventType.AnimateTrack,
			LevelEventType.AddDecoration,
			LevelEventType.Flash,
			LevelEventType.MoveCamera,
			LevelEventType.SetHitsound,
			LevelEventType.RecolorTrack,
			LevelEventType.MoveTrack,
			LevelEventType.SetFilter,
			LevelEventType.HallOfMirrors,
			LevelEventType.ShakeScreen,
			LevelEventType.SetPlanetRotation,
			LevelEventType.MoveDecorations,
			LevelEventType.PositionTrack,
			LevelEventType.RepeatEvents,
			LevelEventType.Bloom,
			LevelEventType.SetConditionalEvents,
			LevelEventType.ScreenTile,
			LevelEventType.ScreenScroll,
			LevelEventType.AddText,
			LevelEventType.SetText,
			LevelEventType.LevelSettings,
			LevelEventType.SongSettings,
			LevelEventType.TrackSettings,
			LevelEventType.BackgroundSettings,
			LevelEventType.CameraSettings,
			LevelEventType.MiscSettings,
		};

		public static bool Prefix(scnEditor __instance) {
			if (Main.Settings.MoveEventUp.Check) {
				var order = EventsOrder.Reverse().ToList();
				var index = order.IndexOf(Inspectors.EventType);
				order = order.Skip(index).ToList();
				if (order.Any()) {
					order = order.Skip(1).ToList();
					foreach (var eventType in order) {
						UnityModManager.Logger.Log(eventType.ToString());
						var events = scnEditor.instance.events.Where(evnt =>
							evnt.floor == scnEditor.instance.selectedFloors[0].seqID);
						if (events.All(evnt => evnt.eventType != eventType)) continue;
						scnEditor.instance.levelEventsPanel.ShowPanel(eventType);
						break;
					}
				}
			}

			if (Main.Settings.MoveEventDown.Check) {
				var order = EventsOrder.ToList();
				var index = order.IndexOf(Inspectors.EventType);
				order = order.Skip(index).ToList();
				if (order.Any()) {
					order = order.Skip(1).ToList();
					foreach (var eventType in order) {
						UnityModManager.Logger.Log(eventType.ToString());
						var events = scnEditor.instance.events.Where(evnt =>
							evnt.floor == scnEditor.instance.selectedFloors[0].seqID);
						if (events.All(evnt => evnt.eventType != eventType)) continue;
						scnEditor.instance.levelEventsPanel.ShowPanel(eventType);
						break;
					}
				}
			}

			if (Main.Settings.MoveEventRight.Check) {
				InspectorTab current = null;
				foreach (var obj in scnEditor.instance.levelEventsPanel.tabs) {
					InspectorTab component = ((RectTransform) obj).gameObject.GetComponent<InspectorTab>();
					if (component.levelEventType == Inspectors.EventType) {
						current = component;
						break;
					}
				}

				if (current == null) return true;
				current.cycleButtons.CycleEvent(true);
				return false;
			}

			if (Main.Settings.MoveEventLeft.Check) {
				InspectorTab current = null;
				foreach (var obj in scnEditor.instance.levelEventsPanel.tabs) {
					InspectorTab component = ((RectTransform) obj).gameObject.GetComponent<InspectorTab>();
					if (component.levelEventType == Inspectors.EventType) {
						current = component;
						break;
					}
				}

				if (current == null) return true;
				current.cycleButtons.CycleEvent(false);
				return false;
			}

			if (Main.Settings.DeleteEvent.Check) {
				int num = scnEditor.instance.levelEventsPanel.EventNumOfTab(Inspectors.EventType);
				scnEditor.instance.RemoveEventAtSelected(Inspectors.EventType);
				scnEditor.instance.levelEventsPanel.ShowPanel(Inspectors.EventType, num - 1);
				return false;
			}

			return true;
		}
	}
}