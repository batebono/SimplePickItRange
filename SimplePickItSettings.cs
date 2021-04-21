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

        [Menu("Maximum Distance. 100 -> anywhere")]
        public RangeNode<int> MaxDistance { get; set; } = new RangeNode<int>(50, 10, 100);

        [Menu("Time between clicks in milliseconds")]
        public RangeNode<int> DelayClicksInMs { get; set; } = new RangeNode<int>(40, 25, 100);
        [Menu("Extra delay for consecutive clicks on the same item in milliseconds")]
        public RangeNode<int> ExtraDelaySameItemInMs { get; set; } = new RangeNode<int>(100, 0, 300);

    }
}
