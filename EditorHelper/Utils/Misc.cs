using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using ADOFAI;
using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using HarmonyLib;
using UnityEngine;
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
				
				case PropertyType.LongString:
					propDict["type"] = "Text";
					propDict["default"] = prop.value_default;
					break;
			}

			return propDict;
		}

		public static List<object> EncodeLevelEventInfoList(Dictionary<string, LevelEventInfo> eventInfos) {
			var result = new List<object>();
			foreach (var info in eventInfos.Values) {
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

		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> dictionary, out TKey key, out TValue value) {
			key = dictionary.Key;
			value = dictionary.Value;
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

		public static IAsyncResult RunAsync(Action task, AsyncCallback? callback = null) {
			return task.BeginInvoke(callback, null);
		}

		public static IAsyncResult RunAsync(Action task, Action? callback) {
			return task.BeginInvoke(_ => callback?.Invoke(), null);
		}

		public static IEnumerator RunAsyncCo(Action task, AsyncCallback? callback = null) {
			var asyncResult = task.BeginInvoke(callback, null);
			yield return new WaitUntil(() => asyncResult.IsCompleted);
		}

		public static IEnumerator RunAsyncCo(Action task, Action? callback) {
			var asyncResult = task.BeginInvoke(_ => callback?.Invoke(), null);
			yield return new WaitUntil(() => asyncResult.IsCompleted);
		}
		
		public static (double, double) TargetRes(this (int, int) size, int targetResX, int targetResY) {
			return (size.Item1 / (double) targetResX, size.Item2 / (double) targetResY);
		}

		public static Assembly? GetAssemblyByName(string name) {
			return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
		}

		public static Type[] GetAllTypes(Assembly? source) {
			if (source == null) return new Type[] { };
			var types = new HashSet<Type>();
			var typeQueue = new Queue<Type>();

			foreach (var t in source.GetTypes()) typeQueue.Enqueue(t);

			while (typeQueue.Count > 0) {
				var type = typeQueue.Dequeue();
				foreach (var t in type.GetNestedTypes()) typeQueue.Enqueue(t);
				types.Add(type);
			}

			return types.ToArray();
		}
		
		public static Type[] GetAllTypes(Type source) {
			var types = new HashSet<Type>();
			var typeQueue = new Queue<Type>();

			foreach (var t in source.GetNestedTypes(AccessTools.all)) typeQueue.Enqueue(t);

			while (typeQueue.Count > 0) {
				var type = typeQueue.Dequeue();
				foreach (var t in type.GetNestedTypes(AccessTools.all)) typeQueue.Enqueue(t);
				types.Add(type);
			}

			return types.ToArray();
		}
		
		internal static MethodBase? GetOriginalMethod(this HarmonyMethod attr) {
			try {
				MethodType? methodType = attr.methodType;
				if (methodType != null) {
					switch (methodType.GetValueOrDefault()) {
						case MethodType.Normal:
							if (attr.methodName == null) {
								return null;
							}
							return AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes, null);
						case MethodType.Getter:
							if (attr.methodName == null) {
								return null;
							}
							return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetGetMethod(true);
						case MethodType.Setter:
							if (attr.methodName == null) {
								return null;
							}
							return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetSetMethod(true);
						case MethodType.Constructor:
							return AccessTools.DeclaredConstructor(attr.declaringType, attr.argumentTypes, false);
						case MethodType.StaticConstructor:
							return (from c in AccessTools.GetDeclaredConstructors(attr.declaringType, null)
								where c.IsStatic
								select c).FirstOrDefault<ConstructorInfo>();
					}
				}
			} catch (AmbiguousMatchException ex) {
				throw new Exception("Ambiguous match for HarmonyMethod[" + attr.invoke<string>("Description")() + "]", ex.InnerException ?? ex);
			}
			return null;
		}

		public static (string, XmlObject?)[] ToXmlSerialziable(this ITweakSetting setting) {
			if (setting == null) throw new NullReferenceException();
			var result = new List<(string, XmlObject?)>();
			var type = setting.GetType();

			foreach (var info in type.GetFields()
				.Where(info => info.GetCustomAttribute<XmlIgnoreAttribute>() == null)) {
				var value = info.GetValue(setting);
				var obj = new XmlObject();
				if (value is KeyMap keyMap) {
					obj.KeyMap = keyMap;
				} else if (value is Fraction fraction) {
					obj.Fraction = fraction;
				} else if (value is bool boolean) {
					obj.Boolean = boolean;
				} else if (value is int int32) {
					obj.Int32 = int32;
				} else if (value is float single) {
					obj.Single = single;
				} else if (value is string str) {
					obj.String = str;
				}

				result.Add((info.Name, obj));
			}

			return result.ToArray();
		}

		public static void SetValue(this MemberInfo memberInfo, object? obj, object? value) {
			if (memberInfo is FieldInfo fieldInfo) {
				fieldInfo.SetValue(obj, value);
			}
			if (memberInfo is System.Reflection.PropertyInfo propertyInfo) {
				propertyInfo.SetValue(obj, value);
			}
		}
		public static object? GetValue(this MemberInfo memberInfo, object obj) {
			if (memberInfo is FieldInfo fieldInfo) {
				return fieldInfo.GetValue(obj);
			}
			if (memberInfo is System.Reflection.PropertyInfo propertyInfo) {
				return propertyInfo.GetValue(obj);
			}

			return null;
		}

		public static (Tuple<int, TileRelativeTo> startTile, Tuple<int, TileRelativeTo> endTile) CreateShiftedTileIndex(Tuple<int, TileRelativeTo> startTile, Tuple<int, TileRelativeTo> endTile, int createdSeqId, int refSeqId, int lastSeqId) {
			var startSeqId = GetAbsoluteSeqId(startTile.Item1, startTile.Item2, refSeqId, lastSeqId);
			var endSeqId = GetAbsoluteSeqId(endTile.Item1, endTile.Item2, refSeqId, lastSeqId);
			if (createdSeqId <= refSeqId) {
				startSeqId++;
				endSeqId++;
				refSeqId++;
			}
			lastSeqId++;
			return (
				new Tuple<int, TileRelativeTo>(GetRelativeSeqId(startSeqId, startTile.Item2, refSeqId, lastSeqId), startTile.Item2),
				new Tuple<int, TileRelativeTo>(GetRelativeSeqId(endSeqId, endTile.Item2, refSeqId, lastSeqId), endTile.Item2)
			);
		}
		
		public static (Tuple<int, TileRelativeTo> startTile, Tuple<int, TileRelativeTo> endTile) CreateDeletedTileIndex(Tuple<int, TileRelativeTo> startTile, Tuple<int, TileRelativeTo> endTile, int deletedSeqId, int refSeqId, int lastSeqId) {
			var startSeqId = GetAbsoluteSeqId(startTile.Item1, startTile.Item2, refSeqId, lastSeqId);
			var endSeqId = GetAbsoluteSeqId(endTile.Item1, endTile.Item2, refSeqId, lastSeqId);
			DebugUtils.Log($"Deleted: {deletedSeqId} / Start: {startSeqId} / Ref: {refSeqId}");
			if (deletedSeqId <= refSeqId) {
				startSeqId--;
				endSeqId--;
				refSeqId--;
			}
			lastSeqId--;
			return (
				new Tuple<int, TileRelativeTo>(GetRelativeSeqId(startSeqId, startTile.Item2, refSeqId, lastSeqId), startTile.Item2),
				new Tuple<int, TileRelativeTo>(GetRelativeSeqId(endSeqId, endTile.Item2, refSeqId, lastSeqId), endTile.Item2)
			);
		}

		public static int GetAbsoluteSeqId(int tile, TileRelativeTo relativeTo, int refSeqId, int lastSeqId) {
			switch (relativeTo) {
				case TileRelativeTo.ThisTile:
					return refSeqId + tile;
				case TileRelativeTo.Start:
					return tile;
				case TileRelativeTo.End:
					return lastSeqId + tile;
				default:
					throw new ArgumentOutOfRangeException(nameof(relativeTo), relativeTo, null);
			}
		}
		public static int GetRelativeSeqId(int tile, TileRelativeTo relativeTo, int refSeqId, int lastSeqId) {
			switch (relativeTo) {
				case TileRelativeTo.ThisTile:
					return tile - refSeqId;
				case TileRelativeTo.Start:
					return tile;
				case TileRelativeTo.End:
					return tile - lastSeqId;
				default:
					throw new ArgumentOutOfRangeException(nameof(relativeTo), relativeTo, null);
			}
		}

		public static Tuple<int, TileRelativeTo> ChangeRelativeTo(int tile, TileRelativeTo orig, TileRelativeTo to, int refSeqId, int lastSeqId) {
			var absolute = GetAbsoluteSeqId(tile, orig, refSeqId, lastSeqId);
			var change = GetRelativeSeqId(absolute, to, refSeqId, lastSeqId);
			DebugUtils.Log($"({tile}, {orig}) => {absolute} => ({change}, {to})");
			return new Tuple<int, TileRelativeTo>(change, to);
		}
	}
}