using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EditorHelper.Utils;
using GDMiniJSON;
using Ookii.Dialogs;
using SA.GoogleDoc;
using UnityEngine;
using Screen = UnityEngine.Screen;

namespace EditorHelper.Components {
    public enum PanelState {
        Main,
        LevelEvents,
        SearchAssets,
        ImportAsset
    }

    public class EditorHelperPanel : MonoBehaviour {
        public static EditorHelperPanel Instance { get; private set; }
        public static int panelSizeX => 450 * Screen.width / 1920;
        public static int panelSizeY => 500 * Screen.height / 1080;
        public static bool Inited = false;
        public Vector2 lastMousePos;

        public static bool ShowGui {
            get => Main.Settings.EditorPanelOpen;
            set => Main.Settings.EditorPanelOpen = value;
        }

        public PanelState currentState = PanelState.Main;
        public CustomAssets.CustomAssetData currentData;

        public static GUIStyle StyleBackground = new GUIStyle();
        public static GUIStyle StyleAssetPack = new GUIStyle();
        public static GUIStyle StyleXBtn = new GUIStyle();

        public static readonly Texture2D BGTexture = CanvasDrawer.MakeRoundRectangle(
            1, panelSizeX, panelSizeY, 0, 10, new Color32(32, 32, 32, 192));

        public static readonly Texture2D AssetPackTexture = CanvasDrawer.MakeRoundRectangle(
            1, panelSizeX - 30, 50, 0, 10, new Color32(64, 64, 64, 216));

        public static readonly Texture2D AssetPackTextureHover = CanvasDrawer.MakeRoundRectangle(
            1, panelSizeX - 30, 50, 0, 10, new Color32(56, 56, 56, 216));

        public static readonly Texture2D XButton = CanvasDrawer.MakeRoundRectangle(
            1, 30, 30, 0, 5, new Color32(128, 0, 0, 192));

        public static readonly Texture2D XButtonHover = CanvasDrawer.MakeRoundRectangle(
            1, 30, 30, 0, 5, new Color32(192, 0, 0, 192));


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


        private Rect _boxPos = new(Main.Settings.EditorPanelPos, new Vector2(panelSizeX, panelSizeY));

        public Rect BoxPos {
            get => _boxPos;
            set {
                _boxPos = value;
                Main.Settings.EditorPanelPos = new Vector2(value.x, value.y);
            }
        }

        private Vector2 _scrollPos = Vector2.zero;

        private void Update() {
            if (Main.Settings.OpenEditorHelperPanel.Check)
                ShowGui = !ShowGui;
        }

        private static bool _isCreatingBundle = false;
        private static string _bundleName = "";
        private static string _author = "";
        private static string _search = "";
        private static List<CustomAssets.CustomAssetData> Datas = null;

        private void OnGUI() {
            if (!ShowGui) return;
            MoveBox();
            GUI.Box(BoxPos, "EditorHelper Editor Panel", StyleBackground);
            if (GUI.Button(new Rect(BoxPos.x + panelSizeX - 35, BoxPos.y + 5, 30, 30), "×", StyleXBtn)) {
                ShowGui = false;
                return;
            }

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
                    _search = "";
                    Datas = null;
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUIEx.Label(
                (LangCode.English,
                    $"<size=16>Press {Main.Settings.OpenEditorHelperPanel} to Open/Close</size>"),
                (LangCode.Korean, $"<size=16>{Main.Settings.OpenEditorHelperPanel}을 눌러 창을 열거나 닫기</size>"));
            GUILayout.Space(5);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(BoxPos.width));
            GUILayout.BeginVertical(GUILayout.Width(BoxPos.width - 45));
            //Real content
            switch (currentState) {
                case PanelState.Main:
                    if (GUILayout.Button(
                        GUIEx.CheckLangCode((LangCode.English, "<size=22>Event Bundle</size>"),
                            (LangCode.Korean, "<size=22>이벤트 번들</size>")), GUILayout.Height(60)))
                        currentState = PanelState.LevelEvents;

                    if (GUILayout.Button(
                        GUIEx.CheckLangCode((LangCode.English, "<size=22>Search Assets Packs</size>"),
                            (LangCode.Korean, "<size=22>에셋 팩 검색</size>")), GUILayout.Height(60)))
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
                            if (GUILayout.Button(
                                GUIEx.CheckLangCode((LangCode.English, "Create"), (LangCode.Korean, "생성")),
                                GUILayout.Height(30))) {
                                var events = scnEditor.instance.events
                                    .Where(evnt => evnt.floor == selectedFloor.seqID)
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
                            if (GUILayout.Button(
                                GUIEx.CheckLangCode((LangCode.English, "<size=20>Save New Event Bundle</size>"),
                                    (LangCode.Korean, "<size=20>새로운 이벤트 번들로 저장</size>")),
                                GUILayout.Height(40))) {
                                _isCreatingBundle = true;
                                _author = DiscordController.currentUsername ??
                                          SteamIntegration.Instance.GetPlayersName() ?? "";
                            }
                        }

                        if (GUILayout.Button(
                            GUIEx.CheckLangCode((LangCode.English, "or import eventbundle files..."),
                                (LangCode.Korean, "또는 이벤트 번들 파일 불러오기...")), GUILayout.Height(30))) {
                            var dialog = new VistaOpenFileDialog();
                            dialog.Multiselect = true;
                            dialog.Filter = GUIEx.CheckLangCode(
                                (LangCode.English, "Event Bundle (*.eventbundle)|*.eventbundle"),
                                (LangCode.Korean, "이벤트 번들 (*.eventbundle)|*.eventbundle"));
                            dialog.Title = GUIEx.CheckLangCode((LangCode.English, "Find Event Bundle..."),
                                (LangCode.Korean, "이벤트 번들 찾기..."));
                            if (dialog.ShowDialog() == DialogResult.OK) {
                                foreach (string fileName in dialog.FileNames) {
                                    var data = File.ReadAllText(fileName);
                                    try {
                                        EventBundleManager.Datas.Add(
                                            new EventBundle(
                                                (Dictionary<string, object>) Json.Deserialize(data)));
                                    } catch { /* ignored */
                                    }

                                    EventBundleManager.Save();
                                }
                            }
                        }

                        foreach (var bundle in EventBundleManager.Datas.ToArray()) {
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(bundle.Name, GUILayout.Height(25),
                                GUILayout.Width(panelSizeX - 200))) {
                                scnEditor.instance.ApplyBundle(selectedFloor.seqID, bundle);
                            }

                            if (GUILayout.Button(
                                GUIEx.CheckLangCode((LangCode.English, "Delete"), (LangCode.Korean, "삭제")),
                                GUILayout.Height(25))) {
                                EventBundleManager.Datas.Remove(bundle);
                            }

                            if (GUILayout.Button(
                                GUIEx.CheckLangCode((LangCode.English, "Export"), (LangCode.Korean, "내보내기")),
                                GUILayout.Height(25))) {
                                var dialog = new VistaSaveFileDialog();
                                dialog.Filter = GUIEx.CheckLangCode(
                                    (LangCode.English, "Event Bundle (*.eventbundle)|*.eventbundle"),
                                    (LangCode.Korean, "이벤트 번들 (*.eventbundle)|*.eventbundle"));
                                dialog.Title = GUIEx.CheckLangCode((LangCode.English, "Save Event Bundle..."),
                                    (LangCode.Korean, "이벤트 번들 내보내기..."));
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
                        GUIEx.Label((LangCode.English, "<size=22>Select Single tile!</size>"),
                            (LangCode.Korean, "<size=22>타일을 하나만 선택하세요!</size>"));
                    }

                    break;
                case PanelState.SearchAssets:
                    if (!CustomAssets.Inited) {
                        GUILayout.Label(GUIEx.CheckLangCode(
                            (LangCode.English, "<size=18>Loading Assets...</size>"),
                            (LangCode.Korean, "<size=18>에셋 로딩 중...</size>")));
                        break;
                    }

                    GUILayout.Label("Custom Assets Pack");
                    var serachLast = _search;
                    GUIEx.Label((LangCode.English, "Search"), (LangCode.Korean, "검색"));
                    _search = GUILayout.TextField(_search);
                    if (_search != serachLast || Datas == null) {
                        Datas = CustomAssets.CustomAssetDatas
                            .Where(data => data.Name.GetInclusion(_search) >= 0.8)
                            .ToList();
                        Datas.Sort((data, assetData) => {
                            var s1 = data.Name.GetInclusion(_search);
                            var s2 = assetData.Name.GetInclusion(_search);
                            if (s1 > s2) return 1;
                            if (s1 < s2) return -1;
                            return 0;
                        });
                    }

                    foreach (var data in Datas) {
                        if (GUILayout.Button(
                            new GUIContent(
                                $"<color=#ffffff>   <size=16><b>{data.Name}</b></size>\n    <size=12>by {data.Creator}</size></color>",
                                data.SmallLogo), StyleAssetPack, GUILayout.Height(64))) {
                            currentData = data;
                            currentState = PanelState.ImportAsset;
                        }

                        GUILayout.Space(5);
                    }

                    break;
                case PanelState.ImportAsset:
                    if (currentData.Logo != null)
                        GUILayout.Label(currentData.Logo);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(
                        $"<b><size=18>{currentData.Name}</size></b>\n<size=14>by {currentData.Creator}</size>");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUIEx.Label((LangCode.English, "<b><size=16><color=#FF6666>License</color></size></b>"),
                        (LangCode.Korean, "<b><size=16><color=#FF6666>사용 조건</color></size></b>"));
                    GUIEx.Label((LangCode.English, currentData.LicenseEN),
                        (LangCode.Korean, currentData.LicenseKR));
                    GUILayout.Space(20);
                    GUIEx.Label((LangCode.English, "<b><size=16>Description<size></b>"),
                        (LangCode.Korean, "<b><size=16>설명</size></b>"));
                    GUILayout.Label($"{currentData.Description}");
                    GUILayout.Space(10);
                    var path = Path.Combine(Path.GetDirectoryName(CustomLevel.instance.levelPath) ?? "",
                        currentData.Name);
                    if (CustomAssets.CustomAssetData.count >= 0) {
                        GUILayout.Button(
                            GUIEx.CheckLangCode((LangCode.English, "<size=22>Importing Asset Pack"),
                                (LangCode.Korean, "<size=22>에셋 팩 불러오는 중")) +
                            $" ({CustomAssets.CustomAssetData.count}/{CustomAssets.CustomAssetData.total})</size>",
                            GUILayout.Height(60));
                    } else if (Directory.Exists(path)) {
                        if (GUILayout.Button(
                            GUIEx.CheckLangCode((LangCode.English, "<size=22>Re-Import Asset Pack</size>"),
                                (LangCode.Korean, "<size=22>에셋 팩 다시 불러오기</size>")), GUILayout.Height(60))) {
                            scnEditor.instance.StartCoroutine(currentData.ReImport(path));
                        }
                    } else if (GUILayout.Button(
                        GUIEx.CheckLangCode((LangCode.English, "<size=22>Import Asset Pack</size>"),
                            (LangCode.Korean, "<size=22>에셋 팩 불러오기</size>")), GUILayout.Height(60))) {
                        if (CustomLevel.instance.levelPath.IsNullOrEmpty()) {
                            scnEditor.instance.ShowPopup(transform, scnEditor.PopupType.SaveBeforeImageImport);
                        } else {
                            scnEditor.instance.StartCoroutine(currentData.Import(path));
                        }
                    }

                    GUILayout.BeginHorizontal();
                    GUIEx.Label((LangCode.English, "<size=20>Contents</size>"),
                        (LangCode.Korean, "<size=20>내용</size>"));
                    if (GUILayout.Button(
                        GUIEx.CheckLangCode((LangCode.English, "Selct All"), (LangCode.Korean, "모두 선택")),
                        GUILayout.Height(25))) {
                        var keys = currentData.TextureToDownload.Keys.ToArray();
                        foreach (var key in keys) {
                            currentData.TextureToDownload[key] = true;
                        }
                    }

                    if (GUILayout.Button(
                        GUIEx.CheckLangCode((LangCode.English, "Selct All"), (LangCode.Korean, "모두 선택 해제")),
                        GUILayout.Height(25))) {
                        var keys = currentData.TextureToDownload.Keys.ToArray();
                        foreach (var key in keys) {
                            currentData.TextureToDownload[key] = false;
                        }
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.BeginVertical(GUI.skin.textArea);
                    foreach (var (name, value) in currentData.TextureToDownload.ToArray()) {
                        currentData.TextureToDownload[name] = GUILayout.Toggle(value, name);
                    }

                    GUILayout.EndVertical();
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

            StyleXBtn.normal.background = XButton;
            StyleXBtn.normal.textColor = Color.white;
            StyleXBtn.hover.background = XButtonHover;
            StyleXBtn.hover.textColor = Color.white;
            StyleXBtn.alignment = TextAnchor.UpperCenter;
            StyleXBtn.fontSize = 28;
            StyleXBtn.fontStyle = FontStyle.Bold;
        }
    }
}