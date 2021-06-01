using UnityModManagerNet;

namespace EditorHelper.Settings
{
    public class MainSettings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Enable Floor 0 Events")] public bool EnableFloor0Events = true;
        [Draw("Remove All Editor Limits")] public bool RemoveLimits = true;
        [Draw("Enable Auto Paste Artist URL")] public bool AutoArtistURL = true;
        [Draw("Enable Smaller Delta Degree (90° -> 15°, Press 'Ctrl + Alt + ,' or 'Ctrl + Alt + .' to use 15°)")] public bool SmallerDeltaDeg = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
        }
    }
}