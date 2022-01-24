#if false
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using DG.Tweening;
using EditorHelper.Components;
using EditorHelper.Utils;
using HarmonyLib;
using RDTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace EditorHelper.Patch {
    public enum LevelEventTypeEx {
        None,
        SetSpeed,
        Twirl,
        Checkpoint,
        //EditorComment,
        LevelSettings,
        SongSettings,
        TrackSettings,
        BackgroundSettings,
        CameraSettings,
        MiscSettings,
        MoveCamera,
        CustomBackground,
        ChangeTrack,
        ColorTrack,
        AnimateTrack,
        RecolorTrack,
        MoveTrack,
        AddDecoration,
        AddText,
        SetText,
        Flash,
        SetHitsound,
        SetFilter,
        SetPlanetRotation,
        HallOfMirrors,
        ShakeScreen,
        MoveDecorations,
        PositionTrack,
        RepeatEvents,
        Bloom,
        SetConditionalEvents,
        ScreenTile,
        ScreenScroll,
        EditorHelperEventBundles = 100,
        EditorHelperAssetPacks
    }

    [HarmonyPatch]
    internal static class ParseEnumEx {
        public static MethodBase TargetMethod() =>
            AccessTools.Method(typeof(RDUtils), "ParseEnum").MakeGenericMethod(typeof(LevelEventType));

        public static bool Prefix(string str, LevelEventType defaultValue, ref LevelEventType __result) {
            LevelEventType result;
            try {
                result = (LevelEventType) Enum.Parse(typeof(LevelEventTypeEx), str, true);
            } catch (Exception) {	
                Debug.LogWarning("ParseEnum(): Returned default value " + defaultValue.ToString() +
                                             " because couldn't find string value " + str);
                result = defaultValue;
            }
            __result = result;
            return false;
        }
    }

    [HarmonyPatch(typeof(Enum), "ToString", new Type[] {})]
    internal static class ToStringPatch {
        public static bool Prefix(Enum __instance, ref string __result) {
            if (__instance is not LevelEventType) return true;
            __result = ((LevelEventTypeEx) __instance).ToString();
            return false;
        }
    }

    [HarmonyPatch(typeof(InspectorTab), "Init")]
    internal static class InitPatch {
        public static readonly List<LevelEventTypeEx> SettingTypes = new() {
            LevelEventTypeEx.SongSettings,
            LevelEventTypeEx.LevelSettings,
            LevelEventTypeEx.TrackSettings,
            LevelEventTypeEx.BackgroundSettings,
            LevelEventTypeEx.CameraSettings,
            LevelEventTypeEx.MiscSettings,
            LevelEventTypeEx.EditorHelperEventBundles,
            LevelEventTypeEx.EditorHelperAssetPacks,
        };

        public static bool IsSettingType(this LevelEventTypeEx type) => SettingTypes.Contains(type);
        public static bool IsSettingType(this LevelEventType type) => ((LevelEventTypeEx) type).IsSettingType();
        
        public static bool IsCustomType(this LevelEventTypeEx type) => !Enum.IsDefined(typeof(LevelEventType), (LevelEventType) type);
        public static bool IsCustomType(this LevelEventType type) => ((LevelEventTypeEx) type).IsCustomType();
        
        public static bool Prefix(InspectorTab __instance, LevelEventType type, InspectorPanel panel) {
            __instance.panel = panel;
            __instance.levelEventType = type;
            string name = type.ToString();
            __instance.icon.sprite = GCS.levelEventIcons[type];
            __instance.button.name = name;
            var isEventTab = !type.IsSettingType();
            __instance.set("isEventTab", isEventTab);
            if (isEventTab) {
                __instance.invoke("FlipTab")();
            }

            __instance.cycleButtons.panel = panel;
            __instance.SetSelected(false);
            return false;
        }
    }

    [HarmonyPatch(typeof(InspectorPanel), "GetEventNum")]
    internal static class EventNumPatch {
        public static bool Prefix(InspectorPanel __instance, LevelEventType eventType, int eventNum, ref LevelEvent __result) {
            __result = null;
            if (eventNum == -1) {
                return false;
            }

            if (eventType.IsSettingType()) {
                return false;
            }

            LevelEvent result = null;
            int seqID = scnEditor.instance.selectedFloors[0].seqID;
            int num = 0;
            foreach (var levelEvent in scnEditor.instance.events) {
                if (seqID == levelEvent.floor && levelEvent.eventType == eventType) {
                    if (num == eventNum) {
                        result = levelEvent;
                        break;
                    }

                    num++;
                }
            }

            __result = result;
            return false;
        }
    }

    [HarmonyPatch(typeof(InspectorPanel), "ShowPanel")]
    public static class ShowPanelPatch {
	    public static bool Prefix(InspectorPanel __instance, LevelEventType eventType, int eventNum) {
		    __instance.set("showingPanel", true);
		    scnEditor.instance.SaveState();
		    scnEditor.instance.changingState++;
		    PropertiesPanel propertiesPanel = null;
		    foreach (var propertiesPanel2 in __instance.panelsList) {
			    if (propertiesPanel2.levelEventType == eventType) {
				    propertiesPanel2.gameObject.SetActive(true);
				    propertiesPanel = propertiesPanel2;
			    } else {
				    propertiesPanel2.gameObject.SetActive(false);
			    }
		    }

		    if (eventType != LevelEventType.None) {
			    __instance.title.text = RDString.Get("editor." + eventType);
			    LevelEvent levelEvent;
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
			    } else if (!eventType.IsCustomType()) {
				    levelEvent = __instance.GetEventNum(eventType, eventNum);
			    } else {
				    levelEvent = new LevelEvent(0, eventType, GCS.settingsInfo["EditorHelperEventBundles"]);
			    }

			    if (propertiesPanel == null) {
				    RDBaseDll.printesw("selectedPanel should not be null!! >:(");
				    goto IL_221;
			    }

			    if (levelEvent == null) {
				    goto IL_221;
			    }

			    __instance.selectedEvent = levelEvent;
			    __instance.selectedEventType = levelEvent.eventType;
			    propertiesPanel.SetProperties(levelEvent, true);
			    foreach (var obj in __instance.tabs) {
				    var component = ((RectTransform) obj).gameObject.GetComponent<InspectorTab>();
				    if (!(component == null)) {
					    component.SetSelected(eventType == component.levelEventType);
					    if (eventType == component.levelEventType) {
						    component.eventNum = eventNum;
						    if (component.cycleButtons != null) {
							    component.cycleButtons.text.text = (eventNum + 1).ToString();
						    }
					    }
				    }
			    }

			    goto IL_221;
		    }

		    __instance.selectedEventType = LevelEventType.None;
		    IL_221:
		    scnEditor.instance.changingState--;
		    __instance.set("showingPanel", false);
		    return false;
	    }
    }

    [HarmonyPatch(typeof(scnEditor), "Awake")]
    internal static class AwakePatch {
        private static void Prefix() {
	        GCS.levelEventTypeString = new Dictionary<LevelEventType, string>();
	        LevelEventTypeEx[] array = (LevelEventTypeEx[])Enum.GetValues(typeof(LevelEventTypeEx));
	        foreach (var key in array) {
		        GCS.levelEventTypeString.Add((LevelEventType) key, key.ToString());
	        }
	        
	        GCS.levelEventIcons[(LevelEventType) 100] = Assets.EditorHelperIcon;
	        GCS.levelEventIcons[(LevelEventType) 101] = Assets.EditorHelperIcon;

	        GCS.settingsInfo["EditorHelperEventBundles"] = new LevelEventInfo() {
		        categories = new List<LevelEventCategory>(),
		        executionTime = LevelEventExecutionTime.Special,
		        name = "EditorHelperEventBundles",
		        propertiesInfo = new Dictionary<string, PropertyInfo>(),
		        type = (LevelEventType) LevelEventTypeEx.EditorHelperEventBundles
	        };
/*
	        GCS.settingsInfo["EditorHelperAssetPacks"] = new LevelEventInfo() {
		        categories = new List<LevelEventCategory>(),
		        executionTime = LevelEventExecutionTime.Special,
		        name = "EditorHelperAssetPacks",
		        propertiesInfo = new Dictionary<string, PropertyInfo>(),
		        type = (LevelEventType) LevelEventTypeEx.EditorHelperAssetPacks
	        };*/
        }
    }

    [HarmonyPatch(typeof(PropertiesPanel), "Init")]
    public static class PropertyPatch {
	    public static bool Prefix(PropertiesPanel __instance, InspectorPanel panel, LevelEventInfo levelEventInfo) {
		    __instance.inspectorPanel = panel;
		    /*
		    Dictionary<string, PropertyInfo> propertiesInfo = levelEventInfo.propertiesInfo;
		    Dictionary<string, PropertyInfo>.KeyCollection keys = propertiesInfo.Keys;
		    int num = 0;
		    foreach (string text in keys) {
			    PropertyInfo propertyInfo = propertiesInfo[text];
			    if (propertyInfo.controlType != ControlType.Hidden) {
				    GameObject original = null;
				    List<string> list = new List<string>();
				    switch (propertyInfo.type) {
					    case PropertyType.Bool:
						    original = RDC.data.prefab_controlToggle;
						    break;
					    case PropertyType.Int:
					    case PropertyType.Float:
					    case PropertyType.String:
						    original = RDC.data.prefab_controlText;
						    break;
					    case PropertyType.LongString:
						    original = RDC.data.prefab_controlLongText;
						    break;
					    case PropertyType.Color:
						    original = RDC.data.prefab_controlColor;
						    break;
					    case PropertyType.File:
						    original = RDC.data.prefab_controlBrowse;
						    break;
					    case PropertyType.Enum: {
						    Type enumType = propertyInfo.enumType;
						    original = RDC.data.prefab_controlToggle;
						    foreach (string text2 in Enum.GetNames(enumType)) {
							    if ((enumType != typeof(Ease) || (text2 != "Unset" && text2 != "INTERNAL_Zero" &&
							                                      text2 != "INTERNAL_Custom")) &&
							        (levelEventInfo.type != LevelEventType.CameraSettings ||
							         !(text2 == "LastPosition"))) {
								    list.Add(text2);
							    }
						    }

						    break;
					    }
					    case PropertyType.Vector2:
						    original = RDC.data.prefab_controlVector2;
						    break;
					    case PropertyType.Tile:
						    original = RDC.data.prefab_controlTile;
						    break;
					    case PropertyType.Export:
						    original = RDC.data.prefab_controlExport;
						    break;
					    case PropertyType.Rating:
						    original = RDC.data.prefab_controlRating;
						    break;
				    }

				    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(RDC.data.prefab_property);
				    gameObject.transform.SetParent(__instance.content, false);
				    Property property = gameObject.GetComponent<Property>();
				    property.gameObject.name = text;
				    property.key = text;
				    property.info = propertyInfo;
				    GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original);
				    gameObject2.GetComponent<RectTransform>().SetParent(property.controlContainer, false);
				    property.control = gameObject2.GetComponent<PropertyControl>();
				    property.control.propertyInfo = propertyInfo;
				    property.control.propertiesPanel = __instance;
				    if (propertyInfo.type == PropertyType.Enum) {
					    ((PropertyControl_Toggle) property.control).EnumSetup(propertyInfo.enumTypeString, list);
				    }

				    string key = "editor." + property.key + ".help";
				    bool flag;
				    string helpString = RDString.GetWithCheck(key, out flag);
				    if (flag) {
					    Button helpButton = property.helpButton;
					    helpButton.gameObject.SetActive(true);
					    string buttonText =
						    RDString.GetWithCheck("editor." + property.key + ".help.buttonText", out flag);
					    string buttonURL =
						    RDString.GetWithCheck("editor." + property.key + ".help.buttonURL", out flag);
					    helpButton.onClick.AddListener(delegate() {
						    __instance.editor.ShowPropertyHelp(true, helpButton.transform, helpString, default,
							    buttonText, buttonURL);
					    });
				    }

				    property.control.Setup(true);
				    if (property.info.hasRandomValue) {
					    string randValueKey = property.info.randValueKey;
					    property.control.randomControl.propertyInfo = levelEventInfo.propertiesInfo[randValueKey];
					    property.control.randomControl.propertiesPanel = __instance;
					    property.control.randomControl.Setup(true);
					    Button randomButton = property.randomButton;
					    randomButton.gameObject.SetActive(true);
					    randomButton.onClick.AddListener(delegate() {
						    string randModeKey = property.info.randModeKey;
						    int num2 = ((int) __instance.inspectorPanel.selectedEvent[randModeKey] + 1) % 3;
						    __instance.inspectorPanel.selectedEvent[randModeKey] = (RandomMode) num2;
						    property.control.SetRandomLayout();
					    });
				    }

				    num++;
				    __instance.properties.Add(propertyInfo.name, property);
			    }
		    }
			*/

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

				    var parent = __instance.content.gameObject.AddComponent<EventBundlePage>();
				    break;
			    default:
				    return true;
		    }
		    
		    //float y = (69f + component.spacing) * (float) 1;
		    //__instance.content.sizeDelta = __instance.content.sizeDelta.WithY(y);

		    return false;
	    }
    }
}
#endif