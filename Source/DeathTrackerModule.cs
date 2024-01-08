using System;
using System.Reflection;
using System.Reflection.Emit;
using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.ModInterop;

namespace CelesteDeathTracker
{
    public class DeathTrackerModule : EverestModule
    {
        public static DeathTrackerModule? Instance { get; private set; }

        public override Type SettingsType => typeof(DeathTrackerSettings);
        public static DeathTrackerSettings? Settings => (DeathTrackerSettings?)Instance?._Settings;

        private delegate Level GetLevelDelegate(Player player);
        // ReSharper disable once InconsistentNaming
        private static GetLevelDelegate GetLevel = null!;

        public DeathTrackerModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            CreateGetLevelDelegate();
            ModInteropManager.ModInterop(typeof(CollabUtilsInterop));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "AltSidesHelper")
                {
                    AltSidesHelperInterop.CreateDelegates(assembly);
                }
            }

            On.Celeste.LevelLoader.StartLevel += (orig, self) =>
            {
                var level = self.Level;
                level.Add(new DeathDisplay(level));
                orig(self);
            };
            
            Everest.Events.Player.OnDie += player =>
            {
                var level = GetLevel(player);
                var session = level.Session;
                var sessionDeaths = session.Deaths;
                var stats = session.OldStats.Modes[(int)session.Area.Mode];

                level.Tracker.GetEntity<DeathDisplay>()?.OnDeath();

                if (Settings!.AutoRestartChapter && stats.SingleRunCompleted && sessionDeaths > 0 && sessionDeaths >= stats.BestDeaths)
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
                GetLevel(player).Tracker.GetEntity<DeathDisplay>()?.UpdateDisplayText();
            };

            Everest.Events.Level.OnTransitionTo += (level, next, direction) =>
            {
                level.Tracker.GetEntity<DeathDisplay>()?.OnScreenTransition();
            };
        }

        public override void Unload()
        {
        }

        private static void CreateGetLevelDelegate()
        {
            var dynamicMethod = new DynamicMethod("GetLevel_generated", typeof(Level), [typeof(Player)]);
            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(Player).GetField("level", BindingFlags.Instance | BindingFlags.NonPublic)!);
            il.Emit(OpCodes.Ret);

            GetLevel = dynamicMethod.CreateDelegate<GetLevelDelegate>();
        }
    }
}
