using ExileCore;
using ExileCore.PoEMemory.Elements;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace SimplePickIt
{
    public class SimplePickIt : BaseSettingsPlugin<SimplePickItSettings>
    {
        private LabelOnGround[] _itemsToPick = new LabelOnGround[10];
        private readonly Stopwatch _clickTimer = new Stopwatch();
        public override bool Initialise()
        {
            _clickTimer.Start();
            return base.Initialise();
        }

        public override Job Tick()
        {
            return new Job("SimplePickIt", PickItems, 60000);
        }

        private bool IsRunConditionMet()
        {
            if (!Input.GetKeyState(Settings.PickUpKey.Value)) return false;
            if (!GameController.Window.IsForeground()) return false;

            return true;
        }

        private void PickItems()
        {
            var gameWindow = GameController.Window.GetWindowRectangle();
            var lootableGameWindow = new RectangleF(150, 150, gameWindow.Width - 150, gameWindow.Height - 150);

            if (!IsRunConditionMet()) return;
            _itemsToPick = GetItemsToPick(lootableGameWindow);

            while (_itemsToPick.Any() && IsRunConditionMet())
            {
                var nextItem = _itemsToPick[0];
                var onlyMoveMouse = ((long)Settings.DelayClicksInMs > _clickTimer.ElapsedMilliseconds);

                PickItem(nextItem, gameWindow, onlyMoveMouse);
                if (!onlyMoveMouse)
                {
                    _clickTimer.Restart();
                    _itemsToPick = GetItemsToPick(lootableGameWindow);
                }
            }
        }

        private void PickItem(LabelOnGround itemToPick, RectangleF window, bool onlyMoveMouse)
        {
            var centerOfLabel = itemToPick?.Label?.GetClientRect().Center + window.TopLeft;

            if (!centerOfLabel.HasValue) return;
            if (centerOfLabel.Value.X <= 0 || centerOfLabel.Value.Y <= 0) return;
            if (centerOfLabel.Value.X > 10000 || centerOfLabel.Value.Y > 10000) return;
            if (float.IsNaN(centerOfLabel.Value.X) || float.IsNaN(centerOfLabel.Value.Y)) return;

            Input.SetCursorPos(centerOfLabel.Value);

            if (onlyMoveMouse) return;
            Input.Click(MouseButtons.Left);

            if (Settings.DebugLogging?.Value == true) DebugWindow.LogDebug($"SimplePickIt.PickItem -> {DateTime.Now:mm:ss.fff} clicked position x: {centerOfLabel.Value.X} y: {centerOfLabel.Value.Y}");
        }

        private LabelOnGround[] GetItemsToPick(RectangleF window, int maxAmount = 10)
        {
            var windowSize = new RectangleF(150, 150, window.Width-150, window.Height-150);

            var itemsToPick = GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels
                ?.Where(label => label.Address != 0
                    && label.ItemOnGround?.Type != null
                    && label.ItemOnGround.Type == EntityType.WorldItem
                    && label.IsVisible
                    && (label.CanPickUp || label.MaxTimeForPickUp.TotalSeconds <= 0)
                    && label.ItemOnGround.DistancePlayer <= Settings.MaxDistance.Value
                    && (label.Label.GetClientRect().Center).PointInRectangle(windowSize)
                    )
                .OrderBy(label => label.ItemOnGround.DistancePlayer)
                .Take(maxAmount)
                .ToArray();

            return itemsToPick;
        }
    }
}
