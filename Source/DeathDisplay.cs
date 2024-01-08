using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;
using System.Text;
using static CelesteDeathTracker.DeathTrackerSettings.VisibilityOption;

namespace CelesteDeathTracker
{
    [Tracked]
    public class DeathDisplay : Entity
    {
        private const float TextPadLeft = 144;
        private const float TextPadRight = 6;

        private readonly MTexture _bg;
        private readonly MTexture _skull;
        private readonly MTexture _x;
        private readonly Level _level;

        private readonly float _skullOffsetX;

        private string _text = "skull_emoji";
        private float _timer;
        private float _lerp;

        private float _width;

        private int _deathsSinceLevelLoad;
        private int _deathsSinceScreenTransition;

        public DeathDisplay(Level level)
        {
            _level = level;
            _bg = GFX.Gui["strawberryCountBG"];

            _skull = GFX.Gui[GetSkullPath(_level.Session.Area)];
            _skullOffsetX = 0.5f * (66 - _skull.Width);
            
            _x = GFX.Gui["x"];

            Y = GetYPosition();

            Depth = -101;
            Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate;
        }

        public void UpdateDisplayText(bool canShow = true)
        {
            var mode = (int)_level.Session.Area.Mode;
            var stats = _level.Session.OldStats.Modes[mode];

            var newText = new StringBuilder(DeathTrackerModule.Settings!.DisplayFormat)
                .Replace("$C", _level.Session.Deaths.ToString())
                .Replace("$B", stats.SingleRunCompleted ? stats.BestDeaths.ToString() : "-")
                .Replace("$A", SaveData.Instance.Areas_Safe.First(a => a.ID_Safe == _level.Session.Area.ID).Modes[mode].Deaths.ToString())
                .Replace("$T", SaveData.Instance.TotalDeaths.ToString())
                .Replace("$L", _deathsSinceLevelLoad.ToString())
                .Replace("$S", _deathsSinceScreenTransition.ToString())
                .ToString();

            _text = newText;
            _width = ActiveFont.Measure(_text).X + TextPadLeft + TextPadRight;

            if (canShow && DeathTrackerModule.Settings.DisplayVisibility is AfterDeath or AfterDeathAndInMenu)
            {
                _timer = 3f;
            }
        }

        public void OnDeath()
        {
            _deathsSinceLevelLoad++;
            _deathsSinceScreenTransition++;
        }

        public void OnScreenTransition()
        {
            _deathsSinceScreenTransition = 0;

            UpdateDisplayText(false);
        }
        
        public override void Update()
        {
            base.Update();

            Y = Calc.Approach(Y, GetYPosition(), Engine.DeltaTime * 800f);

            var visibility = DeathTrackerModule.Settings!.DisplayVisibility;

            if (visibility == Always
                || _timer > 0f 
                || (_level is { Paused: true, PauseMainMenuOpen: true } && visibility is InMenu or AfterDeathAndInMenu))
            {
                _lerp = Calc.Approach(_lerp, 1f, 1.2f * Engine.RawDeltaTime);
            }
            else
            {
                _lerp = Calc.Approach(_lerp, 0f, 2f * Engine.RawDeltaTime);
            }

            if (_timer > 0f)
            {
                _timer -= Engine.RawDeltaTime;
            }
        }

        public override void Render()
        {
            var basePos = Vector2.Lerp(new Vector2(0 - _width, Y), new Vector2(0, Y), Ease.CubeOut(_lerp)).Round();

            _bg.Draw(new Vector2(_width - _bg.Width + basePos.X, Y));

            if (_width > _bg.Width + basePos.X)
            {
                Draw.Rect(0, Y, _width - _bg.Width + basePos.X, 38f, Color.Black);
            }

            _skull.Draw(new Vector2(basePos.X + 26 + _skullOffsetX, Y - 24));
            _x.Draw(new Vector2(basePos.X + 94, Y - 15));

            ActiveFont.DrawOutline(_text, new Vector2(basePos.X + TextPadLeft, Y - 25f), Vector2.Zero, Vector2.One, Color.White, 2f, Color.Black);
        }

        private float GetYPosition()
        {
            var posY = 10f * DeathTrackerModule.Settings!.DisplayYPosition;

            if (!_level.TimerHidden && !DeathTrackerModule.Settings.FixedYPosition)
            {
                if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
                {
                    posY += 58f;
                }
                else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
                {
                    posY += 78f;
                }
            }

            return posY;
        }

        private static string GetSkullPath(AreaKey area)
        {
            if (area is { LevelSet: "Celeste", SID: "Celeste/LostLevels", Mode: AreaMode.Normal })
            {
                return "collectables/skullGold";
            }
            
            var skullProbePath = "CollabUtils2/skulls/" + (CollabUtilsInterop.GetLobbyLevelSet?.Invoke(area.SID) ?? area.LevelSet);

            if (GFX.Gui.Has(skullProbePath))
            {
                return skullProbePath;
            }

            skullProbePath = AltSidesHelperInterop.GetOverrideSkullIcon(AreaData.Get(area));

            if (skullProbePath != null && GFX.Gui.Has(skullProbePath))
            {
                return skullProbePath;
            }
            
            return area.Mode switch
            {
                AreaMode.Normal => "collectables/skullBlue",
                AreaMode.BSide => "collectables/skullRed",
                _ => "collectables/skullGold"
            };
        }
    }
}
