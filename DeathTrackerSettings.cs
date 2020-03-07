using Celeste.Mod;

namespace CelesteDeathTracker
{
    public class DeathTrackerSettings : EverestModuleSettings
    {
        public bool AutoRestartChapter { get; set; } = false;
        
        [SettingMaxLength(32)]
        public string DisplayFormat { get; set; } = "{0} ({1})";

        [SettingRange(0, 105)]
        public int DisplayYPosition { get; set; } = 16;

        public bool FixedYPosition { get; set; } = false;

        public bool AlwaysVisible { get; set; } = false;
    }
}
