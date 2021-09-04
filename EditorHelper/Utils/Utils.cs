using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ADOFAI;
using UnityEngine;
using UnityModManagerNet;
using Object = System.Object;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace EditorHelper {
	internal static class Utils {
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
	}
}