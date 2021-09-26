using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EditorHelper.Components {
    public class StaticScrollPosition : MonoBehaviour {
        public static Dictionary<string, float> positions = new Dictionary<string, float>();
        public ScrollRect scrollRect;
        public bool enabledcomp;
        public string id;

        private void Awake() {
            scrollRect = GetComponent<ScrollRect>();
            if (!positions.ContainsKey(id)) positions[id] = 1;
        }

        private void OnDisable() {
            if (scrollRect.enabled) return;
            positions[id] = scrollRect.verticalNormalizedPosition;
        }

        private void Start() {
            scrollRect.verticalNormalizedPosition = positions[id];
        }
    }
}