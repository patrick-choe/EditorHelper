using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using EditorHelper.Utils;
using GDMiniJSON;
using HarmonyLib;
using org.mariuszgromada.math.mxparser;
using UnityEngine;
using UnityEngine.UI;

namespace EditorHelper.Patch {
	[HarmonyPatch(typeof(RDEditorUtils), "IsValidHexColor", typeof(string), typeof(bool))]
	internal static class IsValidHexColorPatch {
		private static bool Prefix(ref bool __result, string s) {
			if (!Main.Settings.RemoveLimits) {
				return true;
			}

			__result = s.IsValidHexColor();

			return false;
		}
	}

	[HarmonyPatch(typeof(PropertyControl_Color), "Validate")]
	internal static class ValidatePatch {
		private static bool Prefix(ref string __result, PropertyControl_Color __instance) {
			if (!Main.Settings.RemoveLimits || __instance.propertyInfo?.type != PropertyType.Color) {
				return true;
			}

			var text = __instance.inputField.text;
			__result = text.IsValidHexColor() ? text : (string) __instance.propertyInfo.value_default;
			return false;
		}
	}

	[HarmonyPatch(typeof(PropertyControl_Text), "Setup")]
	internal static class SetupPatch {
		private static bool Prefix(PropertyControl_Text __instance, bool addListener) {
			if (!Main.Settings.RemoveLimits || !addListener) {
				return true;
			}

			if (__instance.propertyInfo.name == "artist") {
				__instance.inputField.onValueChanged.AddListener(value => {
					__instance.editor.settingsPanel.ToggleArtistPopup(value, __instance.rectTransform.position.y,
						__instance);
					__instance.ToggleOthersEnabled();
				});
			} else {
				__instance.inputField.onEndEdit.AddListener(value => {
					__instance.editor.SaveState();
					++__instance.editor.changingState;
					__instance.ValidateInput();
					var selectedEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
					var text = __instance.inputField.text;
					object result = null;
					switch (__instance.propertyInfo.type) {
						case PropertyType.Int:
							int intNum;
							if (int.TryParse(__instance.inputField.text, out var intParsed)) {
								intNum = __instance.propertyInfo.Validate(intParsed);
							} else {
								var expr = new Expression(__instance.inputField.text).calculate();
								if (expr.IsFinite()) {
									intNum = __instance.propertyInfo.Validate(Mathf.RoundToInt((float) expr));
								} else {
									intNum = (int) __instance.propertyInfo.value_default;
								}
							}

							result = intNum;
							break;
						case PropertyType.Float:
							float floatNum;
							if (float.TryParse(__instance.inputField.text, out var floatParsed)) {
								floatNum = __instance.propertyInfo.Validate(floatParsed);
							} else {
								var expr = new Expression(__instance.inputField.text).calculate();
								if (double.IsFinite(expr)) {
									floatNum = __instance.propertyInfo.Validate((float) expr);
								} else {
									floatNum = (float) __instance.propertyInfo.value_default;
								}
							}

							result = floatNum;
							break;
						case PropertyType.String:
							result = text;
							break;
						case PropertyType.Tile:
							if (selectedEvent[__instance.propertyInfo.name] is Tuple<int, TileRelativeTo> tuple) {
								result = new Tuple<int, TileRelativeTo>(int.Parse(text), tuple.Item2);
							}

							break;
					}

					selectedEvent[__instance.propertyInfo.name] = result;
					if (__instance.propertyInfo.name == "angleOffset") {
						__instance.editor.levelEventsPanel.ShowPanelOfEvent(selectedEvent);
					}

					__instance.ToggleOthersEnabled();
					switch (selectedEvent.eventType) {
						case LevelEventType.BackgroundSettings:
							__instance.customLevel.SetBackground();
							break;
						case LevelEventType.AddDecoration:
						case LevelEventType.AddText:
							__instance.editor.UpdateDecorationSprites();
							break;
					}

					__instance.editor.ApplyEventsToFloors();
					__instance.editor.ShowEventIndicators(__instance.editor.selectedFloors[0]);
					--__instance.editor.changingState;
				});
			}

			if (string.IsNullOrEmpty(__instance.propertyInfo.unit)) {
				return false;
			}

			__instance.unit.gameObject.SetActive(true);
			__instance.unit.text = RDString.Get("editor.unit." + __instance.propertyInfo.unit);
			return false;
		}
	}

	[HarmonyPatch(typeof(PropertyControl_Text), "Validate")]
	internal static class ValidateTextPatch {
		private static bool Prefix(PropertyControl_Text __instance, ref string __result) {
			if (!Main.Settings.RemoveLimits || __instance.propertyInfo == null) {
				return true;
			}

			if (__instance.propertyInfo.type == PropertyType.Float) {
				float floatNum;
				if (float.TryParse(__instance.inputField.text, out var result)) {
					floatNum = __instance.propertyInfo.Validate(result);
				} else {
					var expr = new Expression(__instance.inputField.text).calculate();
					if (double.IsFinite(expr)) {
						floatNum = __instance.propertyInfo.Validate((float) expr);
					} else {
						floatNum = (float) __instance.propertyInfo.value_default;
					}
				}

				__result = floatNum.ToString();
				return false;
			}

			if (__instance.propertyInfo.type != PropertyType.Int && __instance.propertyInfo.type != PropertyType.Tile) {
				__result = __instance.inputField.text;
				return false;
			}

			int intNum;
			if (int.TryParse(__instance.inputField.text, out var result1)) {
				intNum = __instance.propertyInfo.Validate(result1);
			} else {
				var expr = new Expression(__instance.inputField.text).calculate();
				if (double.IsFinite(expr)) {
					intNum = __instance.propertyInfo.Validate(Mathf.RoundToInt((float) expr));
				} else {
					intNum = (int) __instance.propertyInfo.value_default;
				}
			}

			__result = intNum.ToString();
			return false;
		}
	}

	[HarmonyPatch(typeof(PropertyControl_Vector2), "Validate")]
	internal static class ValidateVectorPatch {
		private static bool Prefix(PropertyControl __instance, ref string __result, InputField x, InputField y,
			bool returnX) {
			if (!Main.Settings.RemoveLimits) {
				return true;
			}

			var resultX = 0f;
			if (float.TryParse(x.text, out var parsedX)) {
				resultX = parsedX;
			} else {
				var expr = new Expression(x.text).calculate();
				if (double.IsFinite(expr)) {
					resultX = (float) expr;
				}
			}

			var resultY = 0f;
			if (float.TryParse(y.text, out var parsedY)) {
				resultY = parsedY;
			} else {
				var expr = new Expression(y.text).calculate();
				if (double.IsFinite(expr)) {
					resultY = (float) expr;
				}
			}

			var vector2 = __instance.propertyInfo.Validate(new Vector2(resultX, resultY));

			__result = !returnX ? vector2.y.ToString() : vector2.x.ToString();

			return false;
		}
	}

	[HarmonyPatch(typeof(scnEditor), "Start")]
	internal static class StartPatch {
		private static void Prefix() {
			if (!Main.Settings.RemoveLimits || Main.FirstLoaded) {
				return;
			}

			Main.FirstLoaded = true;

			if (Main.Settings.RemoveLimits) {
				foreach (var propertyInfo in GCS.levelEventsInfo.SelectMany(eventPair =>
					eventPair.Value.propertiesInfo.Select(propertiesPair => propertiesPair.Value))) {
					switch (propertyInfo.type) {
						case PropertyType.Color:
							propertyInfo.color_usesAlpha = true;
							break;
						case PropertyType.Int:
							propertyInfo.int_min = int.MinValue;
							propertyInfo.int_max = int.MaxValue;
							break;
						case PropertyType.Float:
							propertyInfo.float_min = float.NegativeInfinity;
							propertyInfo.float_max = float.PositiveInfinity;
							break;
						case PropertyType.Vector2:
							propertyInfo.maxVec = Vector2.positiveInfinity;
							propertyInfo.minVec = Vector2.negativeInfinity;
							break;
					}
				}

				foreach (var propertyInfo in GCS.settingsInfo.SelectMany(eventPair =>
					eventPair.Value.propertiesInfo.Select(propertiesPair => propertiesPair.Value))) {
					switch (propertyInfo.type) {
						case PropertyType.Color:
							propertyInfo.color_usesAlpha = true;
							break;
						case PropertyType.Int:
							propertyInfo.int_min = int.MinValue;
							propertyInfo.int_max = int.MaxValue;
							break;
						case PropertyType.Float:
							propertyInfo.float_min = float.NegativeInfinity;
							propertyInfo.float_max = float.PositiveInfinity;
							break;
						case PropertyType.Vector2:
							propertyInfo.maxVec = Vector2.positiveInfinity;
							propertyInfo.minVec = Vector2.negativeInfinity;
							break;
					}
				}
			} else {
				if (!(Json.Deserialize(Resources.Load<TextAsset>("LevelEditorProperties").text) is
					Dictionary<string, object> dictionary)) {
					return;
				}

				var levelEventsInfo = Misc.Decode(dictionary["levelEvents"] as IEnumerable<object>);
				var settingsInfo = Misc.Decode(dictionary["settings"] as IEnumerable<object>);

				foreach (var (key, value) in GCS.levelEventsInfo) {
					var levelEventInfo = levelEventsInfo[key];

					foreach (var (property, propertyInfo) in value.propertiesInfo) {
						var originalPropertyInfo = levelEventInfo.propertiesInfo[property];

						switch (propertyInfo.type) {
							case PropertyType.Color:
								propertyInfo.color_usesAlpha = originalPropertyInfo.color_usesAlpha;
								break;
							case PropertyType.Int:
								propertyInfo.int_min = originalPropertyInfo.int_min;
								propertyInfo.int_max = originalPropertyInfo.int_max;
								break;
							case PropertyType.Float:
								propertyInfo.float_min = originalPropertyInfo.float_min;
								propertyInfo.float_max = originalPropertyInfo.float_max;
								break;
							case PropertyType.Vector2:
								propertyInfo.maxVec = originalPropertyInfo.maxVec;
								propertyInfo.minVec = originalPropertyInfo.minVec;
								break;
						}
					}
				}

				foreach (var (key, value) in GCS.settingsInfo) {
					var levelEventInfo = settingsInfo[key];

					foreach (var (property, propertyInfo) in value.propertiesInfo) {
						var originalPropertyInfo = levelEventInfo.propertiesInfo[property];

						switch (propertyInfo.type) {
							case PropertyType.Color:
								propertyInfo.color_usesAlpha = originalPropertyInfo.color_usesAlpha;
								break;
							case PropertyType.Int:
								propertyInfo.int_min = originalPropertyInfo.int_min;
								propertyInfo.int_max = originalPropertyInfo.int_max;
								break;
							case PropertyType.Float:
								propertyInfo.float_min = originalPropertyInfo.float_min;
								propertyInfo.float_max = originalPropertyInfo.float_max;
								break;
							case PropertyType.Vector2:
								propertyInfo.maxVec = originalPropertyInfo.maxVec;
								propertyInfo.minVec = originalPropertyInfo.minVec;
								break;
						}
					}
				}
			}
		}
	}
}