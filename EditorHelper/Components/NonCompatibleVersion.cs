using System;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;
using Application = System.Windows.Forms.Application;

namespace EditorHelper.Components {
    public class NonCompatibleVersion : MonoBehaviour {
        public bool complete;
        
        private Rect screenRect;
        
        private Texture2D _bg;
        private GUIStyle _area;
        private GUIStyle _btn;
        private GUIStyle _btn2;

        private void Awake() {
            DontDestroyOnLoad(gameObject);
            screenRect = new Rect(Screen.width / 4f, Screen.height / 4f, Screen.width / 2f, Screen.height / 2f);
            _bg = CanvasDrawer.MakeRoundBorder(
                1,
                (int) screenRect.width,
                (int) screenRect.height,
                5, 40, 
                new Color32(0, 0, 0, 225), new Color32(255, 255, 255, 255));
            _area = new GUIStyle();
            _area.alignment = TextAnchor.UpperCenter;
            _area.font = RDC.data.koreanFont;
            _btn = new GUIStyle();
            _btn.alignment = TextAnchor.MiddleCenter;
            _btn.font = RDC.data.koreanFont;
            _btn.normal.textColor = Color.black;
            _btn2 = new GUIStyle();
            _btn2.alignment = TextAnchor.MiddleCenter;
            _btn2.font = RDC.data.koreanFont;
            _btn2.normal.textColor = Color.black;
        }

        private void OnGUI() {
            if (_btn.normal.background == null) {
                _btn.normal.background = CanvasDrawer.MakeRoundRectangle(1, 250, 50, 1, 5, Color.white);
                _btn.active.background = CanvasDrawer.MakeRoundRectangle(1, 250, 50, 1, 5, new Color(0.9f, 0.9f, 0.9f));
                _btn2.normal.background = CanvasDrawer.MakeRoundRectangle(1, 250, 50, 1, 5, Color.white);
                _btn2.active.background = CanvasDrawer.MakeRoundRectangle(1, 250, 50, 1, 5, new Color(0.9f, 0.9f, 0.9f));
            }
            
            GUI.Label(screenRect, _bg);
            GUILayout.BeginArea(screenRect, _area);
            GUILayout.Space(10);
            if (complete) {
                GUILayout.Label(GUIEx.CheckLangCode(
                    (LangCode.English, "<color=#ff3333><size=65>You cannot use EditorHelper currently</size></color>"),
                    (LangCode.Korean, "<color=#ff3333><size=65>EditorHelper를 현재 사용할 수 없습니다.</size></color>")), _area);
                GUILayout.Label(GUIEx.CheckLangCode((LangCode.English, $"<color=#ffffff><size=40>Compatible ADOFAI version: r{Main.Target}\nCurrent version: r{Main.version}</size></color>"),
                    (LangCode.Korean, $"<color=#ffffff><size=40>호환되는 ADOFAI 버전: r{Main.Target}\n현재 버전: r{Main.version}</size></color>")), _area);
                GUILayout.Label(GUIEx.CheckLangCode((LangCode.English, $"<color=#ffffff><size=40>Use another version of EditorHelper.</size></color>"),
                    (LangCode.Korean, $"<color=#ffffff><size=40>다른 버전의 EditorHelper을 사용하십시오.</size></color>")), _area);
            } else {
                GUILayout.Label(GUIEx.CheckLangCode(
                    (LangCode.English, "<color=#ff3333><size=65>EditorHelper Version is Incompatible</size></color>"),
                    (LangCode.Korean, "<color=#ff3333><size=65>EditorHelper 버전이 호환되지 않습니다</size></color>")), _area);
                GUILayout.Label(GUIEx.CheckLangCode(
                        (LangCode.English,
                            $"<color=#ffffff><size=40>Compatible ADOFAI version: r{Main.Target}\nCurrent version: r{Main.version}</size></color>"),
                        (LangCode.Korean,
                            $"<color=#ffffff><size=40>호환되는 ADOFAI 버전: r{Main.Target}\n현재 버전: r{Main.version}</size></color>")),
                    _area);
            }

            if (complete) {
                GUILayout.Space(screenRect.height - 310);
                GUILayout.BeginHorizontal();
                GUILayout.Space(screenRect.width / 2 - 125);
                if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "<size=30>Disable EditorHelper</size>"), (LangCode.Korean, "<size=30>EditorHelper 비활성화</size>")), _btn, GUILayout.Width(250), GUILayout.Height(50))) {
                    Main.ForceDisableMode = true;
                    Destroy(gameObject);
                }
            } else {
                GUILayout.Space(screenRect.height - 260);
                GUILayout.BeginHorizontal();
                GUILayout.Space(screenRect.width / 2 - 252);
                if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "<size=30>I'll use anyway</size>"), (LangCode.Korean, "<size=30>무시하고 사용하기</size>")), _btn2, GUILayout.Width(250), GUILayout.Height(50))) {
                    Destroy(gameObject);
                }
                GUILayout.Space(4);
                if (GUILayout.Button(GUIEx.CheckLangCode((LangCode.English, "<size=30>Disable EditorHelper</size>"), (LangCode.Korean, "<size=30>EditorHelper 비활성화</size>")), _btn, GUILayout.Width(250), GUILayout.Height(50))) {
                    Main.ForceDisableMode = true;
                    Destroy(gameObject);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            
        }
    }
}