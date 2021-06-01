using System.Collections.Generic;
using System.Linq;
using ADOFAI;

namespace EditorHelper
{
	internal static class Utils
	{
		internal static Dictionary<string, LevelEventInfo> Decode(IEnumerable<object> eventInfoList)
		{
			var dictionary = new Dictionary<string, LevelEventInfo>();
			foreach (Dictionary<string, object> eventInfo in eventInfoList)
			{
				var levelEventInfo = new LevelEventInfo {
					name = eventInfo["name"] as string
				};
				levelEventInfo.type = RDUtils.ParseEnum<LevelEventType>(levelEventInfo.name);
				levelEventInfo.executionTime = RDUtils.ParseEnum(eventInfo["executionTime"] as string, LevelEventExecutionTime.OnBar);
				levelEventInfo.propertiesInfo = new Dictionary<string, PropertyInfo>();
				var objectList = eventInfo["properties"] as List<object>;
				foreach (var propertyInfo in from Dictionary<string, object> dict in objectList where !dict.ContainsKey("enabled") || (bool)dict["enabled"] select new PropertyInfo(dict, levelEventInfo))
				{
					levelEventInfo.propertiesInfo.Add(propertyInfo.name, propertyInfo);
				}
				dictionary.Add(levelEventInfo.name ?? string.Empty, levelEventInfo);
			}
			return dictionary;
		}
	}
}
