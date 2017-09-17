using System.Windows.Controls;
using System.Windows.Input;

namespace SignalVisualizer
{
    public class UnclickableMenuItem : MenuItem
    {
        public UnclickableMenuItem()
        {
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            //base.OnMouseEnter(e);
        }

        protected override void OnClick()
        {
            //base.OnClick();
        }
    }
}
