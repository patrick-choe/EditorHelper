using System.Collections.Generic;
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
        public bool EnableBetterArtistCheck = true;
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
        public bool GraveToSee15Degs = true;
        public int MeshNumerator = 15;
        public int MeshDenominator = 4;
        public bool AllowOtherSongTypes = true;
        public bool AllowMP3 = false;
        public bool AllowWAV = true;
        public bool EnumInputField = true;
        public bool DetectBpmOnLoadSong = true;
        public bool DetectOffsetOnLoadSong = true;
        public KeyMap MoveEventUp = new KeyMap(KeyCode.UpArrow, AdditionalKey.Alt);
        public KeyMap MoveEventDown = new KeyMap(KeyCode.DownArrow, AdditionalKey.Alt);
        public KeyMap MoveEventLeft = new KeyMap(KeyCode.LeftArrow, AdditionalKey.Alt);
        public KeyMap MoveEventRight = new KeyMap(KeyCode.RightArrow, AdditionalKey.Alt);
        public KeyMap DeleteEvent = new KeyMap(KeyCode.Delete, AdditionalKey.Alt);
        public KeyMap ChangeTileAngle = new KeyMap(KeyCode.None, AdditionalKey.Ctrl);
        public KeyMap RotateScreenCW = new KeyMap(KeyCode.Comma, AdditionalKey.Alt);
        public KeyMap RotateScreenCCW = new KeyMap(KeyCode.Period, AdditionalKey.Alt);
        public KeyMap OpenEditorHelperPanel = new KeyMap(KeyCode.BackQuote, AdditionalKey.Shift);
        public bool EditorPanelOpen = true;
        public Vector2 EditorPanelPos = new Vector2(200, 200);
        public string EventBundles = null;
        
        
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