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
        private Stopwatch _getItemsToPickTimer = new Stopwatch();

        public override bool Initialise()
        {
            _getItemsToPickTimer.Start();
            return base.Initialise();
        }

        public override Job Tick()
        {
            var gameWindow = GameController.Window.GetWindowRectangle();
            var lootableGameWindow = new RectangleF(150, 150, gameWindow.Width - 150, gameWindow.Height - 150);

            if (!Input.GetKeyState(Settings.PickUpKey.Value)) return null;
            if (!_getItemsToPickTimer.IsRunning 
                || _getItemsToPickTimer.ElapsedMilliseconds < Settings.DelayGetItemsToPick?.Value) return null;

            _itemsToPick = GetItemsToPick(lootableGameWindow, 10);
            _getItemsToPickTimer.Restart();
            return null;
        }

        public override void Render()
        {
            if (!IsRunConditionMet()) return;

            var coroutineWorker = new Coroutine(PickItems(), this, "SimplePickIt.PickItems");
            Core.ParallelRunner.Run(coroutineWorker);
        }

        private bool IsRunConditionMet()
        {
            if (!Input.GetKeyState(Settings.PickUpKey.Value)) return false;
            if (!GameController.Window.IsForeground()) return false;

            return true;
        }

        private IEnumerator PickItems()
        {
            var gameWindow = GameController.Window.GetWindowRectangle();

            var clickTimer = new Stopwatch();
            clickTimer.Start();
            var firstRun = true;
            while (_itemsToPick.Any() && Input.GetKeyState(Settings.PickUpKey.Value))
            {
                var nextItem = _itemsToPick[0];
                var onlyMoveMouse = ((long)Settings.DelayClicksInMs > clickTimer.ElapsedMilliseconds) && !firstRun;

                yield return PickItem(nextItem, gameWindow, onlyMoveMouse);
                if (!onlyMoveMouse)
                {
                    clickTimer.Restart();
                    firstRun = false;
                }
            }
        }

        private IEnumerator PickItem(LabelOnGround itemToPick, RectangleF window, bool onlyMoveMouse)
        {
            var centerOfLabel = itemToPick?.Label?.GetClientRect().Center + window.TopLeft;

            if (!centerOfLabel.HasValue) yield break;
            if (centerOfLabel.Value.X <= 0 || centerOfLabel.Value.Y <= 0) yield break;
            if (centerOfLabel.Value.X > 10000 || centerOfLabel.Value.Y > 10000) yield break;
            if (float.IsNaN(centerOfLabel.Value.X) || float.IsNaN(centerOfLabel.Value.Y)) yield break;

            Input.SetCursorPos(centerOfLabel.Value);

            if (onlyMoveMouse) yield break;
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
                    && (label.CanPickUp || label.MaxTimeForPickUp.TotalSeconds <= 0)
                    )
                .OrderBy(label => label.ItemOnGround.DistancePlayer)
                .Take(maxAmount)
                .ToArray();

            return itemsToPick;
        }
    }
}
