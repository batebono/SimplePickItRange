using ExileCore;
using ExileCore.PoEMemory.Elements;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

namespace SimplePickIt
{
    public class SimplePickIt : BaseSettingsPlugin<SimplePickItSettings>
    {
        private Random Random { get; } = new Random();

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
            var window = GameController.Window.GetWindowRectangle();
            var nextItem = GetItemToPick(window);
            long lastItemAddress;
            while (nextItem != null && Input.GetKeyState(Settings.PickUpKey.Value))
            {
                lastItemAddress = nextItem.Address;
                yield return PickItem(nextItem, window);
                yield return new WaitTime(Settings.DelayClicksInMs.Value);

                nextItem = GetItemToPick(window);
                if (nextItem.Address == lastItemAddress) yield return new WaitTime(Settings.ExtraDelaySameItemInMs.Value);
            }
        }

        private IEnumerator PickItem(LabelOnGround itemToPick, RectangleF window)
        {
            var centerOfLabel = itemToPick?.Label?.GetClientRect().Center 
                + window.TopLeft
                + new Vector2(Random.Next(0, 2), Random.Next(0, 2));

            if (!centerOfLabel.HasValue) yield break;

            Input.SetCursorPos(centerOfLabel.Value);
            Input.Click(MouseButtons.Left);
        }

        private LabelOnGround GetItemToPick(RectangleF window)
        {
            var windowSize = new RectangleF(150, 150, window.Width-150, window.Height-150);

            var closestLabel = GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels
                ?.Where(label => label.Address != 0
                    && label.ItemOnGround?.Type != null
                    && label.ItemOnGround.Type == EntityType.WorldItem
                    && label.IsVisible
                    && (label.CanPickUp || label.MaxTimeForPickUp.TotalSeconds <= 0)
                    && label.ItemOnGround.DistancePlayer <= Settings.MaxDistance.Value
                    && (label.Label.GetClientRect().Center).PointInRectangle(windowSize)
                    )
                .OrderBy(label => label.ItemOnGround.DistancePlayer)
                .FirstOrDefault();

            return closestLabel;
        }
    }
}
