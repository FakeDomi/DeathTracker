using System;
using System.Linq;
using System.Text;
using Celeste;
using Celeste.Mod;
using Monocle;

namespace CelesteDeathTracker
{
    public class DeathTrackerModule : EverestModule
    {
        public static DeathTrackerModule Module;

        public DeathTrackerModule()
        {
            Module = this;
        }

        public override Type SettingsType => typeof(DeathTrackerSettings);
        public static DeathTrackerSettings Settings => (DeathTrackerSettings) Module._Settings;

        public override void Load()
        {
            DeathDisplay display = null;
            Level level = null;

            On.Celeste.LevelLoader.LoadingThread += (orig, self) =>
            {
                orig(self);
                self.Level.Add(display = new DeathDisplay(self.Level));
                
                level = self.Level;
            };

            Everest.Events.Player.OnDie += player =>
            {
                var sessionDeaths = level.Session.Deaths;
                var stats = level.Session.OldStats.Modes[(int)level.Session.Area.Mode];

                if (Settings.AutoRestartChapter && stats.SingleRunCompleted && sessionDeaths > 0 &&
                    sessionDeaths >= stats.BestDeaths)
                {
                    Engine.TimeRate = 1f;
                    level.Session.InArea = false;
                    Audio.SetMusic(null);
                    Audio.BusStopAll("bus:/gameplay_sfx", true);
                    level.DoScreenWipe(false, () => Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, level.Session));

                    foreach (var component in level.Tracker.GetComponents<LevelEndingHook>())
                    {
                        ((LevelEndingHook)component).OnEnd?.Invoke();
                    }
                }
            };

            Everest.Events.Player.OnSpawn += player =>
            {
                var mode = (int) level.Session.Area.Mode;
                var stats = level.Session.OldStats.Modes[mode];

                display.SetDisplayText(new StringBuilder(Settings.DisplayFormat)
                    .Replace("$C", level.Session.Deaths.ToString())
                    .Replace("$B", stats.SingleRunCompleted ? stats.BestDeaths.ToString() : "-")
                    .Replace("$A", SaveData.Instance.Areas_Safe.First(a => a.ID_Safe == level.Session.Area.ID).Modes[mode].Deaths.ToString())
                    .Replace("$T", SaveData.Instance.TotalDeaths.ToString())
                    .ToString());
            };
        }

        public override void Unload()
        {
        }
    }
}
