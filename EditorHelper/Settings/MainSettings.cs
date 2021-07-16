using UnityModManagerNet;

namespace EditorHelper.Settings
{
    public class MainSettings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Enable Floor 0 Events")] public bool EnableFloor0Events = true;
        [Draw("Remove All Editor Limits")] public bool RemoveLimits = true;
        [Draw("Enable Auto Paste Artist URL")] public bool AutoArtistURL = true;
        [Draw("Enable Smaller Delta Degree (90° -> 15°, Press 'Ctrl + Alt + ,' or 'Ctrl + Alt + .' to use 15°)")] public bool SmallerDeltaDeg = true;
        [Draw("Enable better editor backup in nested directory")] public bool EnableBetterBackup = true;
        [Draw("Limit the amount of backups (0 is infinite)")] public int MaximumBackups = 25;
        [Draw("Still put the backup in backup.adofai after using better backup")] public bool SaveLatestBackup = false;
        // 능지 딸려서 구현 실패
        // [Draw("Enable Rotating Editor Screen (Press 'Alt + ,' or 'Alt + .' to rotate editor screen 15°)")] public bool EnableScreenRot = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
        }
    }
}