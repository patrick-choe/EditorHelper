using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using ADOFAI;
using GDMiniJSON;
using RDTools;
using UnityEngine;
using UnityEngine.Networking;
using UnityModManagerNet;
using Object = System.Object;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace EditorHelper.Utils {
	internal static class Misc {

		public static Dictionary<string, object> Encode(this PropertyInfo prop) {
			var propDict = new Dictionary<string, object>();
			propDict["name"] = prop.name;
			propDict["type"] = prop.type.ToString();
			switch (prop.type) {
				case PropertyType.Float:
					propDict["default"] = prop.value_default;
					propDict["min"] = prop.float_min;
					propDict["max"] = prop.float_max;
					break;

				case PropertyType.Bool:
					propDict["default"] = prop.value_default;
					break;

				case PropertyType.Int:
					propDict["default"] = prop.value_default;
					propDict["min"] = prop.int_min;
					propDict["max"] = prop.int_max;
					break;

				case PropertyType.Color:
					propDict["default"] = prop.value_default;
					propDict["usesAlpha"] = prop.color_usesAlpha;
					break;

				case PropertyType.File:
					propDict["default"] = prop.value_default;
					break;

				case PropertyType.String:
					propDict["default"] = prop.value_default;
					propDict["minLength"] = prop.string_minLength;
					propDict["maxLength"] = prop.string_maxLength;
					propDict["needsUnicode"] = prop.string_needsUnicode;
					break;

				case PropertyType.Enum:
					propDict["type"] = $"Enum:{prop.enumType.Name}";
					propDict["default"] = prop.value_default.ToString();
					break;

				case PropertyType.Vector2:
					var defVec = (Vector2) prop.value_default;
					propDict["default"] = new List<object> {defVec.x, defVec.y};
					propDict["min"] = new List<object> {prop.minVec.x, prop.minVec.y};
					propDict["max"] = new List<object> {prop.maxVec.x, prop.maxVec.y};
					break;

				case PropertyType.Tile:
					var defTuple = (Tuple<int, TileRelativeTo>) prop.value_default;
					propDict["default"] = new List<object> {defTuple.Item1, defTuple.Item2.ToString()};
					propDict["min"] = prop.int_min;
					propDict["max"] = prop.int_max;
					break;

				case PropertyType.Rating:
					propDict["default"] = prop.value_default;
					break;
				
				default:
					propDict["type"] = "Text";
					propDict["default"] = prop.value_default;
					break;
			}

			return propDict;
		}

		public static List<object> EncodeLevelEventInfoList(Dictionary<string, LevelEventInfo> eventInfos) {
			var result = new List<object>();
			foreach ((_, var info) in eventInfos) {
				var dict = new Dictionary<string, object>();
				dict["name"] = info.name;
				dict["executionTime"] = info.executionTime.ToString();
				var props = new List<object>();
				foreach ((_, var prop) in info.propertiesInfo) {
					props.Add(prop.Encode());
				}

				dict["properties"] = props;
				result.Add(dict);
			}

			return result;
		}

		internal static Dictionary<string, LevelEventInfo> Decode(IEnumerable<object> eventInfoList) {
			var dictionary = new Dictionary<string, LevelEventInfo>();
			foreach (Dictionary<string, object> eventInfo in eventInfoList) {
				var levelEventInfo = new LevelEventInfo {
					name = eventInfo["name"] as string
				};
				levelEventInfo.type = RDUtils.ParseEnum<LevelEventType>(levelEventInfo.name);
				levelEventInfo.executionTime =
					RDUtils.ParseEnum(eventInfo["executionTime"] as string, LevelEventExecutionTime.OnBar);
				levelEventInfo.propertiesInfo = new Dictionary<string, PropertyInfo>();
				var objectList = eventInfo["properties"] as List<object>;
				foreach (var propertyInfo in
					from Dictionary<string, object> dict in objectList
					where !dict.ContainsKey("enabled") || (bool) dict["enabled"]
					select new PropertyInfo(dict, levelEventInfo)) {
					levelEventInfo.propertiesInfo.Add(propertyInfo.name, propertyInfo);
				}

				dictionary.Add(levelEventInfo.name ?? string.Empty, levelEventInfo);
			}

			return dictionary;
		}

		internal static bool IsFinite(this double d) =>
			(BitConverter.DoubleToInt64Bits(d) & long.MaxValue) < 9218868437227405312L;

		public static Texture2D DuplicateTexture(Texture2D source) {
			RenderTexture renderTex = RenderTexture.GetTemporary(
				source.width,
				source.height,
				0,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Linear);

			Graphics.Blit(source, renderTex);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTex;
			Texture2D readableText = new Texture2D(source.width, source.height);
			readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
			readableText.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTex);
			return readableText;
		}

		public static IAsyncResult RunAsync(Action task, AsyncCallback callback = null) {
			return task.BeginInvoke(callback, null);
		}

		public static IAsyncResult RunAsync(Action task, Action callback) {
			return task.BeginInvoke(ar => callback?.Invoke(), null);
		}

		public static IEnumerator RunAsyncCo(Action task, AsyncCallback callback = null) {
			var asyncResult = task.BeginInvoke(callback, null);
			yield return new WaitUntil(() => asyncResult.IsCompleted);
		}

		public static IEnumerator RunAsyncCo(Action task, Action callback) {
			var asyncResult = task.BeginInvoke(ar => callback?.Invoke(), null);
			yield return new WaitUntil(() => asyncResult.IsCompleted);
		}
	}
}