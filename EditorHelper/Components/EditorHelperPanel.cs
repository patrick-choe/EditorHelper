using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EditorHelper.Utils;
using GDMiniJSON;
using Ookii.Dialogs;
using SA.GoogleDoc;
using UnityEngine;
using UnityEngine.Serialization;
using UnityModManagerNet;

namespace EditorHelper.Components {
    public enum PanelState {
        Main,
        LevelEvents,
        SearchAssets,
        ImportAsset
    }
    public class EditorHelperPanel : MonoBehaviour {
        public static EditorHelperPanel Instance { get; private set; }
        public const int panelSizeX = 400;
        public const int panelSizeY = 500;
        public static bool Inited = false;

        public static bool ShowGui {
            get => Main.Settings.EditorPanelOpen;
            set => Main.Settings.EditorPanelOpen = value;
        }
        public PanelState currentState = PanelState.Main;
        public CustomAssets.CustomAssetData currentData;

        public static GUIStyle StyleBackground = new GUIStyle();
        public static GUIStyle StyleAssetPack = new GUIStyle();
        public static Texture2D BGTexture = CanvasDrawer.MakeRoundRectangle(
            1, panelSizeX, panelSizeY, 0, 10, new Color32(32, 32, 32, 192));
        public static Texture2D AssetPackTexture = CanvasDrawer.MakeRoundRectangle(
            1, panelSizeX - 30, 50, 0, 10, new Color32(64, 64, 64, 216));
        public static Texture2D AssetPackTextureHover = CanvasDrawer.MakeRoundRectangle(
            1, panelSizeX - 30, 50, 0, 10, new Color32(56, 56, 56, 216));

        public static Texture2D Example = CanvasDrawer.MakeRoundRectangle(
            1, 30, 30, 0, 10, Color.white);

        public static bool IsDragging {
            get {
                if (Instance == null) isDragging = false;
                return isDragging;
            }
        }
        
        public static bool Contains {
            get {
                if (Instance == null) contains = false;
                return contains;
            }
        }

        private static bool isDragging;
        private static bool contains;

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
                return;
            }

            Init();
            CustomAssets.Load();
            Instance = this;
        }


        private Rect _boxPos = new Rect(Main.Settings.EditorPanelPos, new Vector2(panelSizeX, panelSizeY));
        public Rect BoxPos {
            get => _boxPos;
            set {
                _boxPos = value;
                Main.Settings.EditorPanelPos = new Vector2(value.x, value.y);
            }
        }
        [FormerlySerializedAs("_lastMousePos")] public Vector2 lastMousePos;
        private Vector2 _scrollPos = Vector2.zero;

        private void Update() {
            if (Main.Settings.OpenEditorHelperPanel.Check)
                ShowGui = !ShowGui;
        }

        private static bool _isCreatingBundle = false;
        private static string _bundleName = "";
        private static string _author = "";
        private void OnGUI() {
            if (!ShowGui) return;
            MoveBox();
            GUI.Box(BoxPos, "EditorHelper Editor Panel", StyleBackground);
            GUILayout.BeginArea(new Rect(BoxPos.x, BoxPos.y + 20, BoxPos.width, BoxPos.height - 20));
            if (currentState == PanelState.ImportAsset) {
                if (GUILayout.Button("◀ Back", GUILayout.Width(80), GUILayout.Height(30))) {
                    currentState = PanelState.SearchAssets;
                }
            }
            if (currentState is PanelState.LevelEvents or PanelState.SearchAssets) {
                if (GUILayout.Button("◀ Back", GUILayout.Width(80), GUILayout.Height(30))) {
                    currentState = PanelState.Main;
                    _bundleName = "";
                    _author = "";
                    _isCreatingBundle = false;
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUIEx.Label((LangCode.English, $"<size=16>Press {Main.Settings.OpenEditorHelperPanel} to Open/Close</size>"), (LangCode.Korean, $"<size=16>{Main.Settings.OpenEditorHelperPanel}을 눌러 창을 열거나 닫기</size>"));
            GUILayout.Space(5);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(BoxPos.width));
            GUILayout.BeginVertical(GUILayout.Width(BoxPos.width - 45));
            //Real content
            switch (currentState) {
                case PanelState.Main:
                    if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "<size=22>Event Bundle</size>"), (LangCode.Korean, "<size=22>이벤트 번들</size>")), GUILayout.Height(60)))
                        currentState = PanelState.LevelEvents;
                    
                    if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "<size=22>Search Assets Packs</size>"), (LangCode.Korean, "<size=22>에셋 팩 검색</size>")), GUILayout.Height(60)))
                        currentState = PanelState.SearchAssets;
                    break;
                case PanelState.LevelEvents:
                    if (scnEditor.instance.SelectionIsSingle()) {
                        var selectedFloor = scnEditor.instance.selectedFloors[0];


                        if (_isCreatingBundle) {
                            GUILayout.BeginHorizontal();
                            GUIEx.Label(90, null, (LangCode.English, "Name"), (LangCode.Korean, "이름"));
                            GUIEx.TextField(ref _bundleName);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUIEx.Label(90, null, (LangCode.English, "Author"), (LangCode.Korean, "제작자"));
                            GUIEx.TextField(ref _author);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "Create"), (LangCode.Korean, "생성")), GUILayout.Height(30))) {
                                var events = scnEditor.instance.events.Where(evnt => evnt.floor == selectedFloor.seqID)
                                    .ToList();
                                var eventData = new EventBundle(_bundleName, _author, events);
                                EventBundleManager.Datas.Add(eventData);
                                EventBundleManager.Save();
                                _bundleName = "";
                                _author = "";
                                _isCreatingBundle = false;
                            }
                            if (GUILayout.Button("×", GUILayout.Height(30), GUILayout.Width(30))) {
                                _bundleName = "";
                                _author = "";
                                _isCreatingBundle = false;
                            }
                            GUILayout.EndHorizontal();
                        } else {
                            _bundleName = "";
                            if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "<size=20>Save New Event Bundle</size>"), (LangCode.Korean, "<size=20>새로운 이벤트 번들로 저장</size>")), GUILayout.Height(40))) {
                                _isCreatingBundle = true;
                                _author = DiscordController.currentUsername ??
                                          SteamIntegration.Instance.GetPlayersName() ?? "";
                            }
                        }

                        if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "or import eventbundle files..."), (LangCode.Korean, "또는 이벤트 번들 파일 불러오기...")), GUILayout.Height(30))) {
                            var dialog = new VistaOpenFileDialog();
                            dialog.Multiselect = true;
                            dialog.Filter = GUIEx.CheckLangCode((LangCode.English, "Event Bundle (*.eventbundle)|*.eventbundle"), (LangCode.Korean, "이벤트 번들 (*.eventbundle)|*.eventbundle"));
                            dialog.Title = GUIEx.CheckLangCode((LangCode.English, "Find Event Bundle..."), (LangCode.Korean, "이벤트 번들 찾기..."));
                            if (dialog.ShowDialog() == DialogResult.OK) {
                                foreach (string fileName in dialog.FileNames) {
                                    var data = File.ReadAllText(fileName);
                                    try {
                                        EventBundleManager.Datas.Add(
                                            new EventBundle((Dictionary<string, object>) Json.Deserialize(data)));
                                    } catch { /* ignored */ }
                                    EventBundleManager.Save();
                                }
                            }
                        }

                        foreach (var bundle in EventBundleManager.Datas.ToArray()) {
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(bundle.Name, GUILayout.Height(25), GUILayout.Width(panelSizeX - 200))) {
                                scnEditor.instance.ApplyBundle(selectedFloor.seqID, bundle);
                            }
                            if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "Delete"), (LangCode.Korean, "삭제")), GUILayout.Height(25))) {
                                EventBundleManager.Datas.Remove(bundle);
                            }
                            if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "Export"), (LangCode.Korean, "내보내기")), GUILayout.Height(25))) {
                                var dialog = new VistaSaveFileDialog();
                                dialog.Filter = GUIEx.CheckLangCode((LangCode.English, "Event Bundle (*.eventbundle)|*.eventbundle"), (LangCode.Korean, "이벤트 번들 (*.eventbundle)|*.eventbundle"));
                                dialog.Title = GUIEx.CheckLangCode((LangCode.English, "Save Event Bundle..."), (LangCode.Korean, "이벤트 번들 내보내기..."));
                                dialog.DefaultExt = ".eventbundle";
                                dialog.AddExtension = true;
                                if (dialog.ShowDialog() == DialogResult.OK) {
                                    if (!dialog.FileName.EndsWith(".eventbundle")) {
                                        dialog.FileName += ".eventbundle";
                                    }
                                    File.WriteAllText(dialog.FileName, bundle.Encode());
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    } else {
                        GUIEx.Label((LangCode.English, "<size=22>Select Single tile!</size>"), (LangCode.Korean, "<size=22>타일을 하나만 선택하세요!</size>"));
                    }
                    break;
                case PanelState.SearchAssets:
                    if (!CustomAssets.Inited) {
                        GUILayout.Label("Loading Assets...");
                        break;
                    }
                    GUILayout.Label("Custom Assets Pack");
                    foreach (var data in CustomAssets.CustomAssetDatas) {
                        if (GUILayout.Button(new GUIContent($"<color=#ffffff>   <size=16><b>{data.Name}</b></size>\n    <size=12>by {data.Creator}</size>\n   <size=11>{data.summary}</size></color>", data.SmallLogo), StyleAssetPack)) {
                            currentData = data;
                            currentState = PanelState.ImportAsset;
                        }
                        GUILayout.Space(5);
                    }
                    break;
                case PanelState.ImportAsset:
                    GUILayout.BeginHorizontal();
                    if (currentData.Logo != null)
                        GUILayout.Label(currentData.Logo);
                    GUILayout.Label($"  <b><size=18>{currentData.Name}</size></b>\n  <size=14>by {currentData.Creator}</size>");
                    GUILayout.EndHorizontal();
                    GUILayout.Label($"{currentData.description}");
                    GUILayout.Space(10);
                    var path = Path.Combine(Path.GetDirectoryName(CustomLevel.instance.levelPath) ?? "", currentData.Name);
                    if (Directory.Exists(path)) {
                        GUILayout.Button("<size=22>Asset Pack Imported</size>", GUILayout.Height(60));
                    }
                    else if (GUILayout.Button("<size=22>Import Asset Pack</size>", GUILayout.Height(60))) {
                        if (CustomLevel.instance.levelPath.IsNullOrEmpty()) {
                            scnEditor.instance.ShowPopup(transform, scnEditor.PopupType.SaveBeforeImageImport);
                        } else {
                            currentData.Import(path);
                        }
                    }
                    GUILayout.Label("Contents");
                    var content = " - " + String.Join("\n - ", currentData.TextureList);
                    GUILayout.TextArea(content);
                    break;
            }
            
            //Real content
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void MoveBox() {
            contains = BoxPos.Contains(Event.current.mousePosition);
            if (Input.GetMouseButtonDown(0) && Contains) {
                isDragging = true;
                lastMousePos = Input.mousePosition;
            }
            if (!Input.GetMouseButton(0)) {
                isDragging = false;
            }
            if (isDragging) {
                BoxPos = new Rect(
                    BoxPos.x + Input.mousePosition.x - lastMousePos.x,
                    BoxPos.y - Input.mousePosition.y + lastMousePos.y,
                    panelSizeX,
                    panelSizeY);
            }
            lastMousePos = Input.mousePosition;
        }

        private static void Init() {
            if (Inited) return;
            Inited = true;

            StyleBackground.normal.background = BGTexture;
            StyleBackground.alignment = TextAnchor.UpperCenter;
            StyleBackground.fontSize = 18;
            StyleBackground.normal.textColor = Color.white;
            StyleBackground.fontStyle = FontStyle.Bold;
            
            StyleAssetPack.normal.background = AssetPackTexture;
            StyleAssetPack.hover.background = AssetPackTextureHover;
            StyleAssetPack.alignment = TextAnchor.MiddleLeft;
            StyleAssetPack.clipping = TextClipping.Clip;
            StyleAssetPack.fixedWidth = panelSizeX - 30;
        }
    }
}