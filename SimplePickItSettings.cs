using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.Attributes;
using System.Windows.Forms;

namespace SimplePickIt
{
    public class SimplePickItSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);

        [Menu("PickUp Hotkey")]
        public HotkeyNode PickUpKey { get; set; } = new HotkeyNode(Keys.Space);

        [Menu("Maximum Distance. 200 -> anywhere")]
        public RangeNode<int> MaxDistance { get; set; } = new RangeNode<int>(100, 10, 500);

        [Menu("Time between clicks in milliseconds")]
        public RangeNode<int> DelayClicksInMs { get; set; } = new RangeNode<int>(40, 0, 100);

        [Menu("Delay between search items to pick")]
        public RangeNode<int> DelayGetItemsToPick { get; set; } = new RangeNode<int>(200, 50, 500);

        [Menu("Activate extensive debug logging")]
        public ToggleNode DebugLogging { get; set; } = new ToggleNode(false);

    }
}
