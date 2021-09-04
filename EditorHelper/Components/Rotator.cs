/*using System;
using UnityEngine;
using UnityModManagerNet;

namespace MoreEditorOptions.Components {
    public class Rotator : MonoBehaviour {
        public static Vector2 Size = new Vector2(200, 200);
        public static Vector2 Position = new Vector2(800, 200);
        public static string Value = "0";

        public void OnGUI() {
            GUI.Box(new Rect(Position, Size), "EditorHelper Panel");
            var rot = Camera.current.transform.eulerAngles.z;
            GUILayout.BeginArea(new Rect(Position.x, Position.y - 10, Size.x, Size.y - 10));
            var vTemp = GUILayout.TextField(Value);
            if (int.TryParse(vTemp, out var val)) {
                Camera.current.transform.eulerAngles = new Vector3(0, 0, val);
                
                Value = vTemp;
            }
            GUILayout.EndArea();
        }
    }
}*/