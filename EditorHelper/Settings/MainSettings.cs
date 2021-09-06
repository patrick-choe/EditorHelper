using System.Linq;
using SA.GoogleDoc;
using UnityModManagerNet;
using System.Reflection;
using UnityEngine;

namespace EditorHelper.Settings {
    public class MainSettings : UnityModManager.ModSettings, IDrawable {
        public bool EnableFloor0Events = true;
        public bool RemoveLimits = true;
        public bool AutoArtistURL = true;
        public bool SmallerDeltaDeg = true;
        public bool EnableBetterBackup = true;
        public int MaximumBackups = 25;
        public bool SaveLatestBackup = false;
        public bool HighlightTargetedTiles = true;
        public bool SelectTileWithShortcutKeys = true;
        public bool ChangeIndexWhenToggle = true;
        public bool ChangeIndexWhenCreateTile = true;
        public bool MoreEditorSettings = true;
        public bool EnableScreenRot = true;
        public bool EnableSelectedTileShowAngle = true;
        public bool EnableChangeAngleByDragging = true;
        public int MeshNumerator = 15;
        public int MeshDenominator = 4;
        public double MeshDelta => MeshNumerator / (double) MeshDenominator;

        public override void Save(UnityModManager.ModEntry modEntry) {
            Save(this, modEntry);
        }

        internal bool? moreEditorSettings_prev { get; set; }

        public void OnChange() {
            if (MoreEditorSettings != moreEditorSettings_prev) {
                Main.CheckMoreEditorSettings();
                moreEditorSettings_prev = MoreEditorSettings;
            }
        }
    }
}