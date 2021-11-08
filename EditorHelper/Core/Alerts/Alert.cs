using System;
using System.Collections;
using System.Collections.Generic;
using EditorHelper.Utils;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class Alert : MonoBehaviour {
        protected static Rect _screenRect;
        private Rect _screenRectSized;
        private static Texture2D _bg = null!;
        protected static GUIStyle SettingTitle = null!;
        private static GUIStyle _btn = null!;
        private static GUIStyle _btn2 = null!;
        private int _buttonCount;
        private List<(Func<string>, Action, int, int, bool)> _buttons = new();

        private const float RES = 1.7f;
        private bool _init = false;

        protected virtual (int, int) WindowSize => (960, 540);
        protected virtual (int, int) TargetRes => (1920, 1080);

        public virtual void Init() { }
        public virtual void Content() { }

        public TextAnchor Alignment {
            get => SettingTitle.alignment;
            set => SettingTitle.alignment = value;
        }
        

        private void Awake() {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(Prepare());
        }

        private IEnumerator Prepare() {
            var windowSize = WindowSize.TargetRes(TargetRes.Item1, TargetRes.Item2);
            var width = Screen.width * (float) windowSize.Item1;
            var height = Screen.height * (float) windowSize.Item2;
            
            _screenRect = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
            _screenRectSized = new Rect((Screen.width - width) / 2 - 60, (Screen.height - height) / 2, width, height);
            _bg = CanvasDrawer.MakeRoundBorder(
                1,
                (int) (_screenRect.width * RES),
                (int) (_screenRect.height * RES),
                (int) (5 * RES), (int) (40 * RES), 
                new Color32(0, 0, 0, 225), new Color32(255, 255, 255, 255));
            SettingTitle = new GUIStyle();
            SettingTitle.alignment = TextAnchor.UpperCenter;
            SettingTitle.font = RDC.data.koreanFont;
            _btn = new GUIStyle();
            _btn.alignment = TextAnchor.MiddleCenter;
            _btn.font = RDC.data.koreanFont;
            _btn.normal.textColor = Color.black;
            _btn2 = new GUIStyle();
            _btn2.alignment = TextAnchor.MiddleCenter;
            _btn2.font = RDC.data.koreanFont;
            _btn2.normal.textColor = Color.black;
            _init = true;
            Init();
            yield break;
        }

        private void OnGUI() {
            if (!_init) return;
            if (_btn.normal.background == null) {
                _btn.normal.background = CanvasDrawer.MakeRoundRectangle(1, (int) (100 * RES), (int) (20 * RES), (int) RES, (int) (2 * RES), Color.white);
                _btn.active.background = CanvasDrawer.MakeRoundRectangle(1, (int) (100 * RES), (int) (20 * RES), (int) RES, (int) (2 * RES), new Color(0.9f, 0.9f, 0.9f));
                _btn2.normal.background = CanvasDrawer.MakeRoundRectangle(1, (int) (100 * RES), (int) (20 * RES), (int) RES, (int) (2 * RES), new Color(0.75f, 0.75f, 0.75f));
                
                _btn2.active.background = _btn2.normal.background;
            }
            
            GUI.Label(_screenRect, _bg);
            GUILayout.BeginArea(_screenRect, SettingTitle);
            GUILayout.Space(10);
            GUIEx.BeginIndent(40);
            GUILayout.BeginVertical(GUILayout.Width(_screenRect.width - 80));
            Content();
            GUILayout.EndVertical();
            GUIEx.EndIndent();
            GUILayout.EndArea();
            GUILayout.BeginArea(_screenRect, SettingTitle);
            GUILayout.Space(_screenRect.height - 50 - 30);
            RenderButton();
            GUILayout.EndArea();
        }

        protected void AddButton(Func<string> label, Action onClick, bool enabled = true, int width = 250, int textSize = 30) {
            _buttons.Add((label, onClick, MatchRes(width), textSize, enabled));
            _buttonCount += 4 + MatchRes(width);
        }

        protected void RenderButton() {
            GUILayout.BeginHorizontal();
            GUILayout.Space((_screenRect.width - (_buttonCount - 4)) / 2f);
            foreach (var btn in _buttons) {
                if (GUILayout.Button($"<size={btn.Item4 * Screen.width / 1920}>" + btn.Item1() + "</size>", btn.Item5 ? _btn : _btn2, GUILayout.Width(btn.Item3), GUILayout.Height(MatchRes(50))) && btn.Item5) {
                    btn.Item2();
                }
                GUILayout.Space(4);
            }
            GUILayout.EndHorizontal();
        }

        protected double MatchRes(double toMatch) {
            return toMatch * ((double) Screen.width / TargetRes.Item1 + (double) Screen.height / TargetRes.Item2) / 2;
        }
        
        protected int MatchRes(int toMatch) {
            return (int) MatchRes((double) toMatch);
        }

        protected void Close() {
            StartCoroutine(CloseCo());
        }

        private IEnumerator CloseCo() {
            if (_alertQueue.Count != 0) {
                var nextAlert = (Alert) new GameObject().AddComponent(_alertQueue.Dequeue());
                yield return new WaitUntil(() => nextAlert._init);
            }
            Destroy(gameObject);
        }

        public static void Show<T>() where T : Alert {
            if (_alertQueue.Count == 0) new GameObject().AddComponent<T>();
            else _alertQueue.Enqueue(typeof(T));
        }

        private static Queue<Type> _alertQueue = new Queue<Type>();
    }
}