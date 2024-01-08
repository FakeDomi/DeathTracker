// ReSharper disable UnusedMember.Global

using Celeste.Mod;

namespace CelesteDeathTracker
{
    public class DeathTrackerSettings : EverestModuleSettings
    {
        private string _displayFormat = "$C ($B)";

        public bool AutoRestartChapter { get; set; } = false;

        [SettingMaxLength(48)]
        public string DisplayFormat
        {
            get => _displayFormat;
            set => _displayFormat = value.Contains("{0}") || value.Contains("{1}")
                ? string.Format(value, "$C", "$B")
                : value;
        }

        [SettingRange(0, 105)]
        public int DisplayYPosition { get; set; } = 16;

        public bool FixedYPosition { get; set; } = false;

        public VisibilityOption DisplayVisibility { get; set; } = VisibilityOption.AfterDeathAndInMenu;

        public enum VisibilityOption
        {
            Disabled,
            AfterDeath,
            InMenu,
            AfterDeathAndInMenu,
            Always
        }
    }
}
