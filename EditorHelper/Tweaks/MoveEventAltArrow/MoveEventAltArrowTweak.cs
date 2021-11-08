using System.Linq;
using ADOFAI;
using EditorHelper.Core.TweakFunctions;
using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper.Tweaks.MoveEventAltArrow {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Move Between Events")]
    [TweakDescription(LangCode.Korean, "이벤트 사이 이동")]
    public class MoveEventAltArrowTweak : Tweak, ISettingClass<MoveEventAltArrowSetting> {
        public static LevelEventType[]? EventsOrder;

        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
        }

        [TweakFunction("MoveEventUp")]
        public void MoveEventUp() {
            EventsOrder ??= scnEditor.instance.levelEventsPanel.panelsList.Select(panel => panel.levelEventType).ToArray();
            var order = EventsOrder.Reverse().ToList();
            var index = order.IndexOf(scnEditor.instance.levelEventsPanel.selectedEventType);
            order = order.Skip(index).ToList();
            if (order.Any()) {
                order = order.Skip(1).ToList();
                foreach (var eventType in order) {
                    UnityModManager.Logger.Log(eventType.ToString());
                    var events = scnEditor.instance.events.Where(evnt =>
                        evnt.floor == scnEditor.instance.selectedFloors[0].seqID);
                    if (events.All(evnt => evnt.eventType != eventType)) continue;
                    scnEditor.instance.levelEventsPanel.ShowPanel(eventType);
                    break;
                }
            }
        }
        
        [TweakFunction("MoveEventDown")]
        public void MoveEventDown() {
            EventsOrder ??= scnEditor.instance.levelEventsPanel.panelsList.Select(panel => panel.levelEventType).ToArray();
            var order = EventsOrder.ToList();
            var index = order.IndexOf(scnEditor.instance.levelEventsPanel.selectedEventType);
            order = order.Skip(index).ToList();
            if (order.Any()) {
                order = order.Skip(1).ToList();
                foreach (var eventType in order) {
                    UnityModManager.Logger.Log(eventType.ToString());
                    var events = scnEditor.instance.events.Where(evnt =>
                        evnt.floor == scnEditor.instance.selectedFloors[0].seqID);
                    if (events.All(evnt => evnt.eventType != eventType)) continue;
                    scnEditor.instance.levelEventsPanel.ShowPanel(eventType);
                    break;
                }
            }
        }
        
        [TweakFunction("MoveEventLeft")]
        public void MoveEventLeft() {
            EventsOrder ??= scnEditor.instance.levelEventsPanel.panelsList.Select(panel => panel.levelEventType).ToArray();
            InspectorTab? current = null;
            foreach (var obj in scnEditor.instance.levelEventsPanel.tabs) {
                var component = ((RectTransform) obj).gameObject.GetComponent<InspectorTab>();
                if (component.levelEventType == scnEditor.instance.levelEventsPanel.selectedEventType) {
                    current = component;
                    break;
                }
            }

            if (current == null) return;
            current.cycleButtons.CycleEvent(false);
        }
        
        [TweakFunction("MoveEventRight")]
        public void MoveEventRight() {
            EventsOrder ??= scnEditor.instance.levelEventsPanel.panelsList.Select(panel => panel.levelEventType).ToArray();
            InspectorTab? current = null;
            foreach (var obj in scnEditor.instance.levelEventsPanel.tabs) {
                var component = ((RectTransform) obj).gameObject.GetComponent<InspectorTab>();
                if (component.levelEventType == scnEditor.instance.levelEventsPanel.selectedEventType) {
                    current = component;
                    break;
                }
            }

            if (current == null) return;
            current.cycleButtons.CycleEvent(true);
        }
        
        [TweakFunction("DeleteEvent")]
        public void DeleteEvent() {
            var eventType = scnEditor.instance.levelEventsPanel.selectedEventType;
            int num = scnEditor.instance.levelEventsPanel.EventNumOfTab(eventType);
            scnEditor.instance.RemoveEventAtSelected(eventType);
            scnEditor.instance.levelEventsPanel.ShowPanel(eventType, num - 1);
        }
    }
}