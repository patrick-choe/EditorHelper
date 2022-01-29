using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using DG.Tweening;
using EditorHelper.Components;
using EditorHelper.Core.Patch;
using EditorHelper.Utils;
using HarmonyLib;
using RDTools;
using UnityEngine;
using UnityEngine.UI;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace EditorHelper.Tweaks.EventBundles {
	public enum LevelEventTypeEx {
		EditorHelperEventBundles = 200,
		//EditorHelperAssetPacks
	}
	public class EventBundlesPatch {
		

        [TweakPatch(nameof(scnEditor), "Awake")]
        public static class EventBundlesInit {
            public static void Prefix() {
	            GCS.levelEventIcons[(LevelEventType) LevelEventTypeEx.EditorHelperEventBundles] = Assets.EditorHelperIcon;
	            //GCS.levelEventIcons[(LevelEventType) LevelEventTypeEx.EditorHelperAssetPacks] = Assets.EditorHelperIcon;
	            
	            foreach (LevelEventTypeEx key in Enum.GetValues(typeof(LevelEventTypeEx))) {
		            GCS.levelEventTypeString[(LevelEventType) key] = key.ToString();
		            
		            GCS.levelEventsInfo[key.ToString()] = new LevelEventInfo() {
			            categories = new List<LevelEventCategory> { },
			            executionTime = LevelEventExecutionTime.Special,
			            name = key.ToString(),
			            pro = false,
			            propertiesInfo = new Dictionary<string, PropertyInfo>(),
			            type = (LevelEventType) key
		            };
		            if ((int) key >= 200) {
			            GCS.settingsInfo[key.ToString()] = new LevelEventInfo() {
				            categories = new List<LevelEventCategory> { },
				            executionTime = LevelEventExecutionTime.Special,
				            name = key.ToString(),
				            pro = false,
				            propertiesInfo = new Dictionary<string, PropertyInfo>(),
				            type = (LevelEventType) key
			            };
		            }
	            }
            }
        }
        
		public static readonly List<LevelEventTypeEx> SettingTypes = new() {
			(LevelEventTypeEx) LevelEventType.SongSettings,
			(LevelEventTypeEx) LevelEventType.LevelSettings,
			(LevelEventTypeEx) LevelEventType.TrackSettings,
			(LevelEventTypeEx) LevelEventType.BackgroundSettings,
			(LevelEventTypeEx) LevelEventType.CameraSettings,
			(LevelEventTypeEx) LevelEventType.MiscSettings,
			LevelEventTypeEx.EditorHelperEventBundles,
			//LevelEventTypeEx.EditorHelperAssetPacks,
		};

		[TweakPatch]
		internal static class ParseEnumEx {
			public static MethodBase TargetMethod() => AccessTools.Method(typeof(RDUtils), "ParseEnum").MakeGenericMethod(typeof(LevelEventType));

			public static bool Prefix(string str, LevelEventType defaultValue, out LevelEventType __result) {
				LevelEventType result;
				try {
					result = (LevelEventType) Enum.Parse(typeof(LevelEventType), str, true);
				} catch (Exception) {
					try {
						result = (LevelEventType) Enum.Parse(typeof(LevelEventTypeEx), str, true);
					} catch {
						Debug.LogWarning("ParseEnum(): Returned default value " + defaultValue.ToString() +
						                 " because couldn't find string value " + str);
						result = defaultValue;
					}
				}

				__result = result;
				return false;
			}
		}

		[TweakPatch]
		internal static class ToStringPatch {
			public static MethodBase TargetMethod() => AccessTools.Method(typeof(Enum), "ToString", new Type[] { });

			public static bool Prefix(Enum __instance, ref string? __result) {
				if (__instance is not LevelEventType eventType) return true;
				if (((int) eventType) < 100) __result = Enum.GetName(typeof(LevelEventType), __instance);
				else __result = ((LevelEventTypeEx) eventType).ToString();
				return false;
			}
		}

		[TweakPatch(nameof(EditorConstants), "IsSetting")]
		internal static class InitPatch {
			public static bool Prefix(LevelEventType type, out bool __result) {
				__result = SettingTypes.Contains((LevelEventTypeEx) type);
				return false;
			}
		}

		[TweakPatch(nameof(InspectorPanel), "ShowPanel")]
		public static class ShowPanelPatch {
			public static bool Prefix(InspectorPanel __instance, LevelEventType eventType, int eventIndex) {
				__instance.set("showingPanel", true);
				scnEditor.instance.SaveState(true, false);
				scnEditor.instance.changingState++;
				PropertiesPanel? propertiesPanel = null;
				foreach (PropertiesPanel propertiesPanel2 in __instance.panelsList) {
					if (propertiesPanel2.levelEventType == eventType) {
						propertiesPanel2.gameObject.SetActive(true);
						propertiesPanel = propertiesPanel2;
					} else {
						propertiesPanel2.gameObject.SetActive(false);
					}
				}

				if (eventType != LevelEventType.None) {
					__instance.title.text = RDString.Get("editor." + eventType.ToString());
					LevelEvent? levelEvent = null;
					int num = 1;
					if (eventType == LevelEventType.SongSettings) {
						levelEvent = scnEditor.instance.levelData.songSettings;
					} else if (eventType == LevelEventType.LevelSettings) {
						levelEvent = scnEditor.instance.levelData.levelSettings;
					} else if (eventType == LevelEventType.TrackSettings) {
						levelEvent = scnEditor.instance.levelData.trackSettings;
					} else if (eventType == LevelEventType.BackgroundSettings) {
						levelEvent = scnEditor.instance.levelData.backgroundSettings;
					} else if (eventType == LevelEventType.CameraSettings) {
						levelEvent = scnEditor.instance.levelData.cameraSettings;
					} else if (eventType == LevelEventType.MiscSettings) {
						levelEvent = scnEditor.instance.levelData.miscSettings;
					} else {
						if (!eventType.IsCustomType()) {
							List<LevelEvent> selectedFloorEvents = scnEditor.instance.GetSelectedFloorEvents(eventType);
							num = selectedFloorEvents.Count;
							if (eventIndex > selectedFloorEvents.Count - 1) {
								RDBaseDll.printesw("undo is trying to break down, fix!! or dont");
							} else {
								levelEvent = selectedFloorEvents[eventIndex];
							}
						} else {
							switch ((LevelEventTypeEx) eventType) {
								case LevelEventTypeEx.EditorHelperEventBundles:
									levelEvent = Misc.LevelEvent(0, (LevelEventType) LevelEventTypeEx.EditorHelperEventBundles);
									break;

							}
						}
					}

					if (propertiesPanel == null) {
						RDBaseDll.printesw("selectedPanel should not be null!! >:(");
						goto IL_269;
					}

					if (levelEvent == null) {
						goto IL_269;
					}

					__instance.selectedEvent = levelEvent;
					__instance.selectedEventType = levelEvent.eventType;
					propertiesPanel.SetProperties(levelEvent, true);
					foreach (object obj in __instance.tabs) {
						InspectorTab component = ((RectTransform) obj).gameObject.GetComponent<InspectorTab>();
						if (!(component == null)) {
							if (eventType == component.levelEventType) {
								component.SetSelected(true);
								component.eventIndex = eventIndex;
								if (component.cycleButtons != null) {
									component.cycleButtons.text.text = $"{eventIndex + 1}/{num}";
								}
							} else {
								component.SetSelected(false);
							}
						}
					}

					goto IL_269;
				}

				__instance.selectedEventType = LevelEventType.None;
				IL_269:
				scnEditor.instance.changingState--;
				__instance.set("showingPanel", false);
				return false;
			}
		}

		[TweakPatch(nameof(InspectorTab), "SetSelected")]
		public static class SetSelectedPatch {
			public static bool Prefix(InspectorTab __instance, bool selected) {
				if (!selected) {
					__instance.eventIndex = 0;
				}


				bool flag = false;
				if (scnEditor.instance.SelectionIsSingle() &&
				    !Array.Exists(EditorConstants.soloTypes,
					    t => t == __instance.levelEventType) &&
				    !Array.Exists(SettingTypes.Select(ex => (LevelEventType) ex).ToArray(),
					    t => t == __instance.levelEventType)) {
					flag = selected;
					if (scnEditor.instance.GetSelectedFloorEvents(__instance.levelEventType).Count <= 1) {
						flag = false;
					}
				}

				__instance.cycleButtons.gameObject.SetActive(flag);
				RectTransform component = __instance.GetComponent<RectTransform>();
				float num = flag ? 120f : 0f;
				Vector2 endValue = new Vector2(num, component.sizeDelta.y);
				component.DOKill(false);
				component.DOSizeDelta(endValue, 0.05f, false).SetUpdate(true);
				float num2 = selected ? 0f : 3f;
				num2 *= (__instance.get<bool>("isEventTab") ? -1f : 1f);
				num2 -= num / 2f;

				component.DOAnchorPosX(num2, 0.05f, false).SetUpdate(true);
				float alpha = selected ? 0.7f : 0.45f;
				ColorBlock colors = __instance.button.colors;
				colors.normalColor = Color.white.WithAlpha(alpha);
				__instance.button.colors = colors;
				__instance.icon.DOKill(false);
				float alpha2 = selected ? 1f : 0.6f;
				__instance.icon.DOColor(Color.white.WithAlpha(alpha2), 0.05f).SetUpdate(true);

				return false;
			}
		}

		[TweakPatch(nameof(PropertiesPanel), "Init")]
		public static class PropertyPatch {
			public static bool Prefix(PropertiesPanel __instance, InspectorPanel panel, LevelEventInfo levelEventInfo) {
				__instance.inspectorPanel = panel;
				switch ((LevelEventTypeEx) levelEventInfo.type) {
					case LevelEventTypeEx.EditorHelperEventBundles:

						VerticalLayoutGroup component = __instance.content.GetComponent<VerticalLayoutGroup>();
						var sizeFitter = component.GetOrAddComponent<ContentSizeFitter>();
						sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
						sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
						component.childControlHeight = false;
						component.childControlWidth = false;
						var rect = __instance.content.gameObject.GetOrAddComponent<ScrollRect>();
						rect.movementType = ScrollRect.MovementType.Unrestricted;

						__instance.content.gameObject.AddComponent<EventBundlePage>();
						break;

					default:
						return true;
				}

				return false;
			}
		}
	}

	public static class EventBundlesHelper {
		public static bool IsCustomType(this LevelEventTypeEx type) => ((int) type) >= 100;
		public static bool IsCustomType(this LevelEventType type) => ((int) type) >= 100;
	}
}