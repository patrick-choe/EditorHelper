using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using ADOFAI;
using DG.Tweening;
using EditorHelper.Components;
using EditorHelper.Patch;
using EditorHelper.Settings;
using EditorHelper.Utils;
using GDMiniJSON;
using HarmonyLib;
using SA.GoogleDoc;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModManagerNet;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace EditorHelper {
    public static class Main {
        private static Harmony _harmony;
        private static UnityModManager.ModEntry _mod;
        internal static MainSettings Settings { get; private set; }
        internal static bool FirstLoaded;
        internal static bool IsEnabled;

        //internal static bool highlightEnabled;
        // internal static UnityModManager.ModEntry.ModLogger Logger => _mod?.Logger;

        private const int Exact = 0;
        private const int NotLess = 1;
        private const int NotBigger = 2;
        private const int Bigger = 3;
        private const int Less = 4;

        public const int Width = 150;
        public const int Height = 1200;
        public static int Target = 80;
        public static int[] CompletelyNonCompatible = {77, 76, 75, 74, 73, 72, 71, 70, 69, 68};
        public static int? version = null;

        private static bool _forceDisableMode = false;

        public static bool ForceDisableMode {
            get => _forceDisableMode;
            set {
                _forceDisableMode = value;
                try {
                    StopTweaks();
                } catch { }

                try {
                    ApplyConfig(true);
                } catch { }
            }
        }
        
        private static bool Load(UnityModManager.ModEntry modEntry) {
            version = AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null) as int?;
            var editorHelperDir = modEntry.Path;
            UnityModManager.Logger.Log("Dir: " + editorHelperDir);
            var mode = NotBigger;
            bool unsupported = false;
            if (File.Exists(Path.Combine(editorHelperDir, "Version.txt"))) {
                var value = File.ReadAllText(Path.Combine(editorHelperDir, "Version.txt"));
                if (value.StartsWith(">=")) {
                    mode = NotLess;
                    value = value.Substring(2);
                } else if (value.StartsWith("<=")) {
                    mode = NotBigger;
                    value = value.Substring(2);
                } else if (value.StartsWith(">")) {
                    mode = Bigger;
                    value = value.Substring(2);
                } else if (value.StartsWith("<")) {
                    mode = Less;
                    value = value.Substring(2);
                } else if (value.StartsWith("==")) {
                    mode = Exact;
                    value = value.Substring(2);
                }
                
                if (int.TryParse(value, out var val)) {
                    Target = val;
                    UnityModManager.Logger.Log($"EditorHelper version set to {value}");
                }
            }

            UnityModManager.Logger.Log($"Current version: {version}");
            if (version == null) unsupported = true;
            
            switch (mode) {
                case Exact:
                    if (version != Target) unsupported = true;
                    break;
                case NotLess:
                    if (version < Target) unsupported = true;
                    break;
                case NotBigger:
                    if (version > Target) unsupported = true;
                    break;
                case Bigger:
                    if (version <= Target) unsupported = true;
                    break;
                case Less:
                    if (version >= Target) unsupported = true;
                    break;
            }

            if (unsupported) {
                new GameObject().AddComponent<NonCompatibleVersion>().complete =
                    CompletelyNonCompatible.Contains(version ?? -1);
                if (CompletelyNonCompatible.Contains(version ?? -1)) {
                    return true;
                }
            }

            var path = Path.Combine(modEntry.Path, "editorhelper");
            Assets.Load(path);
            Settings = UnityModManager.ModSettings.Load<MainSettings>(modEntry);
            Settings.moreEditorSettings_prev = Settings.MoreEditorSettings;

            _mod = modEntry;
            _mod.OnToggle = OnToggle;
            _mod.OnGUI = OnGUI;
            _mod.OnSaveGUI = OnSaveGUI; 
            
            PatchRDString.Translations["editor.useLegacyFlash"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "기존 플래시 사용"},
                {LangCode.English, "Use legacy flash"},
            };
            PatchRDString.Translations["editor.convertFloorMesh"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "레벨 변환"},
                {LangCode.English, "Convert level"},
            };
            PatchRDString.Translations["editor.convertFloorMesh.toLegacy"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "기존 타일로 변환"},
                {LangCode.English, "Convert to legacy tiles"},
            };
            PatchRDString.Translations["editor.convertFloorMesh.toMesh"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "자유 각도로 변환"},
                {LangCode.English, "Convert to mesh models"},
            };
            PatchRDString.Translations["editor.EditorHelperEventBundles"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "<size=20>EditorHelper 이벤트 번들</size>"},
                {LangCode.English, "EditorHelper Event Bundles</size>"},
            };
            
            PatchRDString.Translations["editor.EditorHelperAssetPacks"] = new Dictionary<LangCode, string> {
                {LangCode.Korean, "<size=20>EditorHelper 에셋 팩</size>"},
                {LangCode.English, "<size=20>EditorHelper Asset Packs</size>"},
            };
            
            EventBundleManager.Load();

            /*
            var toggle = RDC.data.prefab_controlToggle;
            var dropdown = toggle.transform.Find("Dropdown").GetComponent<Dropdown>();
            var label = dropdown.captionText;
            var inputSelector = label.gameObject.AddComponent<InputSelector>();
            inputSelector.Dropdown = dropdown;
            inputSelector.textComponent = label;
            */
            //RDC.data.prefab_controlToggle = Assets.ControlTogglePrefab;
            //Settings.levelEvents ??= Json.Serialize(Misc.EncodeLevelEventInfoList(GCS.levelEventsInfo));
            //GCS.levelEventsInfo = ADOStartup.DecodeLevelEventInfoList(Json.Deserialize(Settings.levelEvents) as List<object>);

            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            _mod = modEntry;
            if (ForceDisableMode) {
                try {
                    StopTweaks();
                } catch { }

                try {
                    ApplyConfig(true);
                } catch { }

                return false;
            }
            
            IsEnabled = value;

            if (value) {
                StartTweaks();
                ApplyConfig();
            } else {
                StopTweaks();
                ApplyConfig(true);
            }


            return true;
        }

        public static void ShowHelperPanel() {
            /*if (IsEnabled && !GCS.standaloneLevelMode)
                new GameObject().AddComponent<EditorHelperPanel>();*/
        }

        private static void StartTweaks() {
            _harmony = new Harmony(_mod.Info.Id);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            CheckMoreEditorSettings();
        }

        internal static void CheckMoreEditorSettings() {
            if (Settings.MoreEditorSettings) {
                try {
                    GCS.settingsInfo["MiscSettings"].propertiesInfo["useLegacyFlash"] =
                        new PropertyInfo(new Dictionary<string, object> {
                            {"name", "useLegacyFlash"},
                            {"type", "Enum:Toggle"},
                            {"default", "Disabled"}
                        }, GCS.settingsInfo["MiscSettings"]);
                    GCS.settingsInfo["MiscSettings"].propertiesInfo["convertFloorMesh"] =
                        new PropertyInfo(new Dictionary<string, object> {
                            {"name", "convertFloorMesh"},
                            {"type", "Export"}
                        }, GCS.settingsInfo["MiscSettings"]);
                } catch (NullReferenceException) { }
            } else {
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("convertFloorMesh");
            }

        }
        
        private static void StopTweaks() {
            //TargetPatch.UntargetFloor();
            _harmony.UnpatchAll(_harmony.Id);
            _harmony = null;
            GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");
            GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("convertFloorMesh");
        }

        public static GUIStyle GrayOverlay;
        private static void OnGUI(UnityModManager.ModEntry modEntry) {
            if (scnEditor.instance != null && !GCS.standaloneLevelMode) {
                GUIEx.Label((LangCode.English, "<size=20>Exit editor to change settings.</size>"), (LangCode.Korean, "<size=20>설정은 에디터 외부에서만 바꿀 수 있습니다.</size>"));
                GUILayout.Space(15);
                GUIEx.DisableAll = true;
                if (GrayOverlay == null) {
                    GrayOverlay = new GUIStyle();
                    GrayOverlay.normal.background = CanvasDrawer.MakeTexture(1000, Height, new Color32(17, 17, 17, 127));
                }
            }
            GUIEx.Label((LangCode.English, "<b><size=20>Floors</size></b>"), (LangCode.Korean, "<b><size=20>타일</size></b>"));
            GUIEx.BeginIndent(10);
            GUIEx.Toggle(ref Settings.EnableFloor0Events, (LangCode.English, "Enable Floor 0 Events"), (LangCode.Korean, "첫 타일 이벤트 활성화"));
            GUIEx.Toggle(ref Settings.HighlightTargetedTiles, (LangCode.English, "Highlight Targeted Tiles"), (LangCode.Korean, "목표 타일 하이라이트"));
            //GUIEx.Toggle(ref Settings.SelectTileWithShortcutKeys, (LangCode.English, "Select Tile With ; + Click, ' + Click"), (LangCode.Korean, "타일을 ; + 클릭, ' + 클릭으로 선택"));
            GUIEx.Toggle(ref Settings.ChangeIndexWhenToggle, (LangCode.English, "Change Index When Toggle This Tile, First Tile, Last Tile"), (LangCode.Korean, "이 타일, 첫 타일, 마지막 타일 전환 시 선택된 타일 유지"));
            GUIEx.Toggle(ref Settings.ChangeIndexWhenCreateTile, (LangCode.English, "Change Index When Create/Delete Tile"), (LangCode.Korean, "타일 생성/제거 시 선택된 타일 유지"));
            GUIEx.Toggle(ref Settings.SmallerDeltaDeg, (LangCode.English, "Enable Smaller Delta Degree (90° -> 15°, Press 'Ctrl + Alt + ,' or 'Ctrl + Alt + .' to use 15°)"), (LangCode.Korean, "더 작은 각도로 타일 회전 (90° -> 15°, 'Ctrl + Alt + ,' 또는 'Ctrl + Alt + .'로 15° 단위 회전)"));
            /*GUIEx.Toggle(ref Settings.EnableChangeAngleByDragging, (LangCode.English, "Enable Change Angle By Dragging Tile"), (LangCode.Korean, "타일을 드래그해서 각도 변경"));
            if (Settings.EnableChangeAngleByDragging) {
                GUIEx.BeginIndent();
                GUILayout.BeginHorizontal();
                GUIEx.IntField(ref Settings.MeshNumerator, 0, int.MaxValue);
                GUILayout.Label("/", GUILayout.Width(15));
                GUIEx.IntField(ref Settings.MeshDenominator, 1, int.MaxValue);
                GUILayout.Label($"({Settings.MeshDelta})", GUILayout.Width(40));
                GUIEx.Label((LangCode.English, "Changed Angle Delta"), (LangCode.Korean, "각도 변경 단위"));
                GUILayout.EndHorizontal();
                GUIEx.EndIndent();
            }*/
            GUIEx.Toggle(ref Settings.EnableSelectedTileShowAngle, (LangCode.English, "Show Angle of Selected Tiles"), (LangCode.Korean, "선택된 타일의 각도 보기"));
            GUIEx.Toggle(ref Settings.GraveToSee15Degs, (LangCode.English, "Show 15 degs in editor while pressing ` key"), (LangCode.Korean, "에디터에서 `키를 누르는 동안 15도 단위 각도 표시"));
            GUIEx.EndIndent();
            GUILayout.BeginHorizontal(); GUILayout.Space(10); GUILayout.EndHorizontal();
            
            GUIEx.Label((LangCode.English, "<b><size=20>Events</size></b>"), (LangCode.Korean, "<b><size=20>이벤트</size></b>"));
            GUIEx.BeginIndent(10);
            GUIEx.Toggle(ref Settings.RemoveLimits, (LangCode.English, "Remove All Editor Limits"), (LangCode.Korean, "에디터 입력값 제한 비활성화"));
            //GUIEx.Toggle(ref Settings.EnumInputField, (LangCode.English, "Direct Text Input in Selections"), (LangCode.Korean, "셀렉터에 직접 값 입력"));
            GUIEx.EndIndent();
            GUILayout.BeginHorizontal(); GUILayout.Space(10); GUILayout.EndHorizontal();
            
            GUIEx.Label((LangCode.English, "<b><size=20>Miscellaneous</size></b>"), (LangCode.Korean, "<b><size=20>기타</size></b>"));
            GUIEx.BeginIndent(10);
            GUIEx.Toggle(ref Settings.MoreEditorSettings, (LangCode.English, "Enable More Editor Settings (Toggle Mesh tiles, etc.)"), (LangCode.Korean, "더 많은 에디터 설정 (자유 각도 토글 등)"));
            GUIEx.Toggle(ref Settings.AutoArtistURL, (LangCode.English, "Enable Auto Paste Artist URL"), (LangCode.Korean, "작곡가 URL 자동 입력"));
            GUIEx.Toggle(ref Settings.EnableBetterArtistCheck, (LangCode.English, "Enable Better Artist Check"), (LangCode.Korean, "더 나은 작곡가 확인"));
            GUIEx.Toggle(ref Settings.AllowOtherSongTypes, (LangCode.English, "Allow other audio file extension"), (LangCode.Korean, "다른 형식의 오디오 파일 허용"));
            if (Settings.AllowOtherSongTypes) {
                GUIEx.BeginIndent();
                GUIEx.Toggle(ref Settings.AllowWAV, (LangCode.English, "WAV"));
                GUIEx.Toggle(ref Settings.AllowMP3, (LangCode.English, "MP3"));
                GUIEx.EndIndent();
            }
            GUIEx.Toggle(ref Settings.DetectBpmOnLoadSong, (LangCode.English, "Detect BPM on Load Song"), (LangCode.Korean, "곡을 로드할 때 BPM 측정"));
            GUIEx.Toggle(ref Settings.DetectOffsetOnLoadSong, (LangCode.English, "Detect Offset on Load Song"), (LangCode.Korean, "곡을 로드할 때 오프셋 측정"));
            GUIEx.Toggle(ref Settings.EnableScreenRot, (LangCode.English, "Enable Rotating Editor Screen (Press 'Alt + ,' or 'Alt + .' to rotate editor screen 15°)"), (LangCode.Korean, "에디터 화면 회전 ('Alt' + , 또는 'Alt' + .)"));
            GUIEx.EndIndent();
            
            GUIEx.Label((LangCode.English, "<b><size=20>Keymap</size></b>"), (LangCode.Korean, "<b><size=20>키 설정</size></b>"));
            GUIEx.BeginIndent(10);
            GUIEx.KeyMap(ref Settings.MoveEventDown, (LangCode.English, "Move to Downer Event"), (LangCode.Korean, "아래에 있는 이벤트로 이동"));
            GUIEx.KeyMap(ref Settings.MoveEventUp, (LangCode.English, "Move to Upper Event"), (LangCode.Korean, "위에 있는 이벤트로 이동"));
            GUIEx.KeyMap(ref Settings.MoveEventRight, (LangCode.English, "Move to Right Event"), (LangCode.Korean, "오른쪽에 있는 이벤트로 이동"));
            GUIEx.KeyMap(ref Settings.MoveEventLeft, (LangCode.English, "Move to Left Event"), (LangCode.Korean, "왼쪽에 있는 이벤트로 이동"));
            GUIEx.KeyMap(ref Settings.DeleteEvent, (LangCode.English, "Delete Event"), (LangCode.Korean, "이벤트 삭제"));
            GUIEx.KeyMap(ref Settings.ChangeTileAngle, (LangCode.English, "Change Angle by Dragging Tile"), (LangCode.Korean, "타일을 드래그해서 각도 변경"));
            GUIEx.KeyMap(ref Settings.RotateScreenCW, (LangCode.English, "Rotate Editor Screen Clockwise"), (LangCode.Korean, "시계 방향으로 에디터 화면 회전"));
            GUIEx.KeyMap(ref Settings.RotateScreenCCW, (LangCode.English, "Rotate Editor Screen Counter Clockwise"), (LangCode.Korean, "반시계 방향으로 에디터 화면 회전"));
            //GUIEx.KeyMap(ref Settings.OpenEditorHelperPanel, (LangCode.English, "Open EditorHelper Panel in Editor"), (LangCode.Korean, "에디터에서 EditorHelper 패널 열기"));
            GUIEx.EndIndent();
            GUILayout.BeginHorizontal(); GUILayout.Space(10); GUILayout.EndHorizontal();
            if (GUIEx.DisableAll) {
                GUILayout.Space(-Height);
                GUILayout.TextArea(string.Empty, GrayOverlay, GUILayout.Height(Height));
            }
            
            GUIEx.DisableAll = false;
            /*GUIEx.Toggle(ref Settings.EnableBetterBackup, (LangCode.English, "Enable better editor backup in nested directory"), (LangCode.Korean, "레벨이 있는 폴더에서 더 나은 백업"));
            if (Settings.EnableBetterBackup) {
                GUIEx.BeginIndent();
                GUIEx.IntField(ref Settings.MaximumBackups, (LangCode.English, "Limit the amount of backups (0 is infinite)"), (LangCode.Korean, "백업 개수 제한 (0 ⇒ 제한 없음)"));
                GUIEx.Toggle(ref Settings.SaveLatestBackup, (LangCode.English, "Still put the backup in backup.adofai after using better backup"), (LangCode.Korean, "더 나은 백업 활성화 후에도 backup.adofai 사용"));
                GUIEx.EndIndent();
            }*/

            /*
            if (Settings.HighlightTargetedTiles && !highlightEnabled) {
                highlightEnabled = true;
                if (scnEditor.instance?.selectedFloors.Count == 1)
                    if (scnEditor.instance.levelEventsPanel.selectedEventType == LevelEventType.MoveTrack ||
                        scnEditor.instance.levelEventsPanel.selectedEventType == LevelEventType.RecolorTrack)
                        TargetPatch.TargetFloor();
            }

            
            if (!Settings.HighlightTargetedTiles && highlightEnabled) {
                highlightEnabled = false;
                if (TargetPatch.targets.Count != 0)
                    TargetPatch.UntargetFloor();
            }*/
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            Settings.Save(modEntry);

            ApplyConfig();
        }

        private static void ApplyConfig(bool forceRemove = false) {
            if (GCS.settingsInfo == null) {
                return;
            }

            if (!FirstLoaded) {
                FirstLoaded = true;
            }

            if (Settings.RemoveLimits && !forceRemove) {
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