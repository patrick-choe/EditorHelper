using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EditorHelper.Utils;
using GDMiniJSON;
using Ookii.Dialogs;
using SA.GoogleDoc;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Button = UnityEngine.UI.Button;

namespace EditorHelper.Components {
    public class EventBundlePage : MonoBehaviour {
        private enum Status {
            Main,
            Creating
        }

        public Text selectionLabel = null!;
        public Button saveButton = null!;
        public Button loadButton = null!;
        public GameObject nameObj = null!;
        public Text nameLabel = null!;
        public InputField bundleName = null!;
        public GameObject creatorObj = null!;
        public Text creatorLabel = null!;
        public InputField bundleCreator = null!;
        public Button saveBundleButton = null!;
        public Button cancelSaveButton = null!;
        public RectTransform bundles = null!;

        private Status _status;
        private int seqId;

        private void Awake() {
            EventBundleManager.Load();

            selectionLabel = new GameObject().AddComponent<Text>();
            selectionLabel.transform.SetParent(transform, false);
            selectionLabel.font = Assets.SettingFont;
            selectionLabel.color = Color.white;
            selectionLabel.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 30);
            selectionLabel.fontSize = 19;
            selectionLabel.text = GUIEx.CheckLangCode((LangCode.English, "Select tile!"),
                (LangCode.Korean, "타일을 선택하세요!"));

            saveButton = new GameObject().AddComponent<Button>();
            saveButton.transform.SetParent(transform, false);
            var labelSave = new GameObject().AddComponent<Text>();
            labelSave.transform.SetParent(saveButton.transform, false);
            labelSave.text = GUIEx.CheckLangCode((LangCode.English, "Save New Event Bundle"),
                (LangCode.Korean, "새로운 이벤트 번들로 저장"));
            labelSave.color = Color.black;
            labelSave.font = Assets.SettingFont;
            labelSave.fontSize = 19;
            labelSave.alignment = TextAnchor.MiddleCenter;
            labelSave.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 40);
            saveButton.image = saveButton.gameObject.AddComponent<Image>();
            saveButton.image.sprite = Assets.ButtonImage;
            saveButton.image.type = Image.Type.Sliced;
            saveButton.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 40);

            nameObj = new GameObject();
            nameObj.transform.SetParent(transform, false);
            var l1 = nameObj.AddComponent<VerticalLayoutGroup>();
            l1.childControlHeight = false;
            l1.childControlWidth = false;
            l1.spacing = 3;
            var s1 = nameObj.AddComponent<ContentSizeFitter>();
            s1.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            s1.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            nameLabel = new GameObject().AddComponent<Text>();
            nameLabel.transform.SetParent(nameObj.transform, false);
            nameLabel.font = Assets.SettingFont;
            nameLabel.color = Color.white;
            nameLabel.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 30);
            nameLabel.fontSize = 19;
            nameLabel.alignment = TextAnchor.LowerLeft;
            nameLabel.text = GUIEx.CheckLangCode((LangCode.English, "Name"), (LangCode.Korean, "이름"));

            bundleName = new GameObject().AddComponent<InputField>();
            bundleName.transform.SetParent(nameObj.transform, false);
            bundleName.image = bundleName.gameObject.AddComponent<Image>();
            bundleName.image.sprite = Assets.InputImage;
            bundleName.image.type = Image.Type.Sliced;
            bundleName.textComponent = new GameObject().AddComponent<Text>();
            bundleName.textComponent.transform.SetParent(bundleName.transform, false);
            bundleName.textComponent.font = Assets.SettingFontRegular;
            bundleName.textComponent.fontSize = 19;
            bundleName.textComponent.color = Color.white;
            bundleName.textComponent.alignment = TextAnchor.MiddleLeft;
            var rct1 = bundleName.textComponent.GetOrAddComponent<RectTransform>();
            rct1.sizeDelta = new Vector2(290, 40);
            rct1.anchoredPosition = new Vector2(5, 0);
            var rct2 = bundleName.GetOrAddComponent<RectTransform>();
            rct2.sizeDelta = new Vector2(300, 40);

            creatorObj = new GameObject();
            creatorObj.transform.SetParent(transform, false);
            var l2 = creatorObj.AddComponent<VerticalLayoutGroup>();
            l2.childControlHeight = false;
            l2.childControlWidth = false;
            l2.spacing = 3;
            var s2 = creatorObj.AddComponent<ContentSizeFitter>();
            s2.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            s2.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            creatorLabel = new GameObject().AddComponent<Text>();
            creatorLabel.transform.SetParent(creatorObj.transform, false);
            creatorLabel.font = Assets.SettingFont;
            creatorLabel.color = Color.white;
            creatorLabel.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 30);
            creatorLabel.fontSize = 19;
            creatorLabel.alignment = TextAnchor.LowerLeft;
            creatorLabel.text = GUIEx.CheckLangCode((LangCode.English, "Creator"), (LangCode.Korean, "제작자"));

            bundleCreator = new GameObject().AddComponent<InputField>();
            bundleCreator.transform.SetParent(creatorObj.transform, false);
            bundleCreator.image = bundleCreator.gameObject.AddComponent<Image>();
            bundleCreator.image.sprite = Assets.InputImage;
            bundleCreator.image.type = Image.Type.Sliced;
            bundleCreator.textComponent = new GameObject().AddComponent<Text>();
            bundleCreator.textComponent.transform.SetParent(bundleCreator.transform, false);
            bundleCreator.textComponent.font = Assets.SettingFontRegular;
            bundleCreator.textComponent.fontSize = 19;
            bundleCreator.textComponent.color = Color.white;
            bundleCreator.textComponent.alignment = TextAnchor.MiddleLeft;
            var rct3 = bundleCreator.textComponent.GetOrAddComponent<RectTransform>();
            rct3.sizeDelta = new Vector2(290, 40);
            rct3.anchoredPosition = new Vector2(5, 0);
            var rct4 = bundleCreator.GetOrAddComponent<RectTransform>();
            rct4.sizeDelta = new Vector2(300, 40);

            saveBundleButton = new GameObject().AddComponent<Button>();
            saveBundleButton.transform.SetParent(transform, false);
            var labelSaveBundle = new GameObject().AddComponent<Text>();
            labelSaveBundle.transform.SetParent(saveBundleButton.transform, false);
            labelSaveBundle.text = GUIEx.CheckLangCode((LangCode.English, "Save"),
                (LangCode.Korean, "저장"));
            labelSaveBundle.color = Color.black;
            labelSaveBundle.font = Assets.SettingFont;
            labelSaveBundle.fontSize = 19;
            labelSaveBundle.alignment = TextAnchor.MiddleCenter;
            labelSaveBundle.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 35);
            saveBundleButton.image = saveBundleButton.gameObject.AddComponent<Image>();
            saveBundleButton.image.sprite = Assets.ButtonImage;
            saveBundleButton.image.type = Image.Type.Sliced;
            saveBundleButton.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 35);

            cancelSaveButton = new GameObject().AddComponent<Button>();
            cancelSaveButton.transform.SetParent(transform, false);
            var labelCancelSave = new GameObject().AddComponent<Text>();
            labelCancelSave.transform.SetParent(cancelSaveButton.transform, false);
            labelCancelSave.text = GUIEx.CheckLangCode((LangCode.English, "Cancel"),
                (LangCode.Korean, "취소"));
            labelCancelSave.color = Color.black;
            labelCancelSave.font = Assets.SettingFont;
            labelCancelSave.fontSize = 14;
            labelCancelSave.alignment = TextAnchor.MiddleCenter;
            labelCancelSave.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 25);
            cancelSaveButton.image = cancelSaveButton.gameObject.AddComponent<Image>();
            cancelSaveButton.image.sprite = Assets.ButtonImage;
            cancelSaveButton.image.type = Image.Type.Sliced;
            cancelSaveButton.image.color = new Color32(255, 184, 184, 255);
            cancelSaveButton.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 25);

            loadButton = new GameObject().AddComponent<Button>();
            loadButton.transform.SetParent(transform, false);
            var labelLoad = new GameObject().AddComponent<Text>();
            labelLoad.transform.SetParent(loadButton.transform, false);
            labelLoad.text = GUIEx.CheckLangCode((LangCode.English, "or import eventbundle files..."),
                (LangCode.Korean, "또는 이벤트 번들 파일 불러오기..."));
            labelLoad.color = Color.black;
            labelLoad.font = Assets.SettingFont;
            labelLoad.fontSize = 14;
            labelLoad.alignment = TextAnchor.MiddleCenter;
            labelLoad.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 25);
            loadButton.image = loadButton.gameObject.AddComponent<Image>();
            loadButton.image.sprite = Assets.ButtonImage;
            loadButton.image.type = Image.Type.Sliced;
            loadButton.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 25);

            bundles = new GameObject().GetOrAddComponent<RectTransform>();
            var s3 = bundles.gameObject.AddComponent<ContentSizeFitter>();
            s3.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            s3.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            bundles.SetParent(transform, false);
            var l3 = bundles.gameObject.AddComponent<VerticalLayoutGroup>();
            l3.childControlHeight = false;
            l3.childControlWidth = false;
            l3.spacing = 6;
            l3.childAlignment = TextAnchor.UpperLeft;

            saveButton.onClick.AddListener(() => _status = Status.Creating);
            cancelSaveButton.onClick.AddListener(() => _status = Status.Main);
            saveBundleButton.onClick.AddListener(() => {
                var selectedFloor = scnEditor.instance.selectedFloors[0];
                var events = scnEditor.instance.events
                    .Where(evnt => evnt.floor == selectedFloor.seqID)
                    .ToList();
                var eventData = new EventBundle(bundleName.text, bundleCreator.text, events);
                EventBundleManager.Datas.Add(eventData);
                EventBundleManager.Save();
                bundleName.text = "";
                bundleCreator.text = "";
                UpdateEventBundle();
                _status = Status.Main;
            });
            loadButton.onClick.AddListener(() => {
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
                        } catch {
                            scnEditor.instance.ShowNotification(RDString.Get("editor.notification.loadingFailed"));
                        }

                        EventBundleManager.Save();
                        UpdateEventBundle();
                    }
                }
            });

            selectionLabel.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(false);
            loadButton.gameObject.SetActive(false);
            nameObj.gameObject.SetActive(false);
            creatorObj.gameObject.SetActive(false);
            saveBundleButton.gameObject.SetActive(false);
            cancelSaveButton.gameObject.SetActive(false);
            bundles.gameObject.SetActive(false);
            UpdateEventBundle();
        }

        private void Update() {
            if (scnEditor.instance.SelectionIsEmpty()) {
                seqId = -1;
                _status = Status.Main;
                if (!selectionLabel.gameObject.activeSelf) {
                    selectionLabel.gameObject.SetActive(true);
                }

                if (saveButton.gameObject.activeSelf) {
                    saveButton.gameObject.SetActive(false);
                }

                if (nameObj.gameObject.activeSelf) {
                    nameObj.gameObject.SetActive(false);
                }

                if (creatorObj.gameObject.activeSelf) {
                    creatorObj.gameObject.SetActive(false);
                }

                if (saveBundleButton.gameObject.activeSelf) {
                    saveBundleButton.gameObject.SetActive(false);
                }

                if (cancelSaveButton.gameObject.activeSelf) {
                    cancelSaveButton.gameObject.SetActive(false);
                }

                if (loadButton.gameObject.activeSelf) {
                    loadButton.gameObject.SetActive(false);
                }

                if (bundles.gameObject.activeSelf) {
                    bundles.gameObject.SetActive(false);
                }
            } else {
                if (selectionLabel.gameObject.activeSelf) {
                    selectionLabel.gameObject.SetActive(false);
                }

                switch (_status) {
                    case Status.Main:
                        seqId = -1;
                        if (scnEditor.instance.SelectionIsSingle()) {
                            if (!saveButton.gameObject.activeSelf)
                                saveButton.gameObject.SetActive(true);
                            if (!loadButton.gameObject.activeSelf)
                                loadButton.gameObject.SetActive(true);
                        } else {
                            if (saveButton.gameObject.activeSelf)
                                saveButton.gameObject.SetActive(false);
                            if (loadButton.gameObject.activeSelf)
                                loadButton.gameObject.SetActive(false);
                        }

                        if (nameObj.gameObject.activeSelf) {
                            nameObj.gameObject.SetActive(false);
                        }

                        if (creatorObj.gameObject.activeSelf) {
                            creatorObj.gameObject.SetActive(false);
                        }

                        if (saveBundleButton.gameObject.activeSelf) {
                            saveBundleButton.interactable = false;
                            saveBundleButton.gameObject.SetActive(false);
                        }

                        if (cancelSaveButton.gameObject.activeSelf) {
                            cancelSaveButton.gameObject.SetActive(false);
                        }

                        if (!bundles.gameObject.activeSelf) {
                            bundles.gameObject.SetActive(true);
                        }

                        break;
                    case Status.Creating:
                        if (seqId != -1 && seqId != scnEditor.instance.selectedFloors[0].seqID) {
                            _status = Status.Main;
                            break;
                        }

                        seqId = scnEditor.instance.selectedFloors[0].seqID;
                        if (saveButton.gameObject.activeSelf) {
                            saveButton.gameObject.SetActive(false);
                        }

                        if (!nameObj.gameObject.activeSelf) {
                            nameObj.gameObject.SetActive(true);
                        }

                        if (!creatorObj.gameObject.activeSelf) {
                            creatorObj.gameObject.SetActive(true);
                            bundleCreator.text = DiscordController.currentUsername ??
                                                 SteamIntegration.Instance.GetPlayersName() ?? "";
                        }

                        if (!saveBundleButton.gameObject.activeSelf) {
                            saveBundleButton.gameObject.SetActive(true);
                        }

                        if (!cancelSaveButton.gameObject.activeSelf) {
                            cancelSaveButton.gameObject.SetActive(true);
                        }

                        if (loadButton.gameObject.activeSelf) {
                            loadButton.gameObject.SetActive(false);
                        }

                        if (bundles.gameObject.activeSelf) {
                            bundles.gameObject.SetActive(false);
                        }

                        saveBundleButton.interactable = !bundleName.text.IsNullOrEmpty() &&
                                                        scnEditor.instance.events.Any(evnt => evnt.floor == seqId);
                        break;
                }
            }
        }

        private void UpdateEventBundle() {
            for (int i = 0; i < bundles.childCount; i++) {
                Destroy(bundles.GetChild(i).gameObject);
            }

            DebugUtils.Log("Destroy complete");

            foreach (var bundle in EventBundleManager.Datas) {
                var obj = new GameObject();
                obj.transform.SetParent(bundles, false);
                obj.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(300, 50);
                var applyButton = new GameObject().AddComponent<Button>();
                applyButton.transform.SetParent(obj.transform, false);
                var label = new GameObject().AddComponent<Text>();
                label.transform.SetParent(applyButton.transform, false);
                label.text = $"{bundle.Name}\n<size=11>{bundle.Author}</size>";
                label.color = Color.black;
                label.font = Assets.SettingFont;
                label.fontSize = 18;
                label.lineSpacing = 0.6f;
                label.alignment = TextAnchor.MiddleCenter;
                label.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
                applyButton.image = applyButton.gameObject.AddComponent<Image>();
                applyButton.image.sprite = Assets.ButtonImage;
                applyButton.image.type = Image.Type.Sliced;
                var rct1 = applyButton.GetOrAddComponent<RectTransform>();
                rct1.sizeDelta = new Vector2(200, 50);
                rct1.anchorMin = new Vector2(0, 0.5f);
                rct1.anchorMax = new Vector2(0, 0.5f);
                rct1.pivot = new Vector2(0, 0.5f);
                rct1.anchoredPosition = new Vector2(0, 0);
                applyButton.onClick.AddListener(() => {
                    if (scnEditor.instance.SelectionIsEmpty()) return;
                    foreach (var floor in scnEditor.instance.selectedFloors) {
                        scnEditor.instance.ApplyBundle(floor.seqID, bundle);
                    }
                });
                DebugUtils.Log("applyButton complete");

                var exportButton = new GameObject().AddComponent<Button>();
                exportButton.transform.SetParent(obj.transform, false);
                var label2 = new GameObject().AddComponent<Text>();
                label2.transform.SetParent(exportButton.transform, false);
                label2.text = GUIEx.CheckLangCode((LangCode.English, "Export"), (LangCode.Korean, "내보내기"));
                label2.color = Color.black;
                label2.font = Assets.SettingFont;
                label2.fontSize = 14;
                label2.lineSpacing = 0.6f;
                label2.alignment = TextAnchor.MiddleCenter;
                label2.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(95, 23);
                exportButton.image = exportButton.gameObject.AddComponent<Image>();
                exportButton.image.sprite = Assets.ButtonImage;
                exportButton.image.type = Image.Type.Sliced;
                var rct2 = exportButton.GetOrAddComponent<RectTransform>();
                rct2.sizeDelta = new Vector2(95, 23);
                rct2.anchorMin = new Vector2(1, 1);
                rct2.anchorMax = new Vector2(1, 1);
                rct2.pivot = new Vector2(1, 1);
                rct2.anchoredPosition = new Vector2(0, 0);
                exportButton.onClick.AddListener(() => {
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
                });
                DebugUtils.Log("exportButton complete");

                var deleteButton = new GameObject().AddComponent<Button>();
                deleteButton.transform.SetParent(obj.transform, false);
                var label3 = new GameObject().AddComponent<Text>();
                label3.transform.SetParent(deleteButton.transform, false);
                label3.text = GUIEx.CheckLangCode((LangCode.English, "Delete"), (LangCode.Korean, "삭제"));
                label3.color = Color.black;
                label3.font = Assets.SettingFont;
                label3.fontSize = 14;
                label3.lineSpacing = 0.6f;
                label3.alignment = TextAnchor.MiddleCenter;
                label3.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(95, 22);
                deleteButton.image = deleteButton.gameObject.AddComponent<Image>();
                deleteButton.image.sprite = Assets.ButtonImage;
                deleteButton.image.type = Image.Type.Sliced;
                deleteButton.image.color = new Color32(255, 184, 184, 255);
                var rct3 = deleteButton.GetOrAddComponent<RectTransform>();
                rct3.sizeDelta = new Vector2(95, 22);
                rct3.anchorMin = new Vector2(1, 0);
                rct3.anchorMax = new Vector2(1, 0);
                rct3.pivot = new Vector2(1, 0);
                rct3.anchoredPosition = new Vector2(0, 0);
                deleteButton.onClick.AddListener(() => {
                    EventBundleManager.Datas.Remove(bundle);
                    EventBundleManager.Save();
                    UpdateEventBundle();

                    IEnumerator Coro() {
                        yield return null;
                        UpdateEventBundle();
                    }

                    StartCoroutine(Coro());
                });
                DebugUtils.Log("deleteButton complete");
            }

            var layout = bundles.GetComponent<VerticalLayoutGroup>();
            layout.enabled = false;
            layout.enabled = true;
        }
    }
}