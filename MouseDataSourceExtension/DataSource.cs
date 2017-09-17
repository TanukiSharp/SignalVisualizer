using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SignalVisualizer.Contracts;

namespace WiimoteSimExtension
{
    [Export(typeof(IDataSource))]
    public class DataSource : DataSourceBase
    {
        public override string Name { get; } = "Mouse";
        public override Guid UniqueIdentifier { get; } = new Guid("f12f7d8e-09b3-4c94-a079-b5b38c377b37");
        public override string[] ComponentNames { get; } = new string[] { "X", "Y", "Y bis" };
        public override uint Version { get; } = 3;

        private readonly double[] values = new double[3];

        public override async Task Start(CancellationToken cancellationToken)
        {
            int h = Screen.PrimaryScreen.Bounds.Height;
            int lastPosZ = h / 2;
            int lastPosY = h / 2;

            while (cancellationToken.IsCancellationRequested == false)
            {
                Point pos = Control.MousePosition;

                if (Control.MouseButtons == MouseButtons.Middle)
                {
                    GetMotionData(pos.X, lastPosY, pos.Y);
                    lastPosZ = pos.Y;
                }
                else
                {
                    GetMotionData(pos.X, pos.Y, lastPosZ);
                    lastPosY = pos.Y;
                }

                NotifyNext(values);

                await Task.Delay(30);
            }
        }

        private void GetMotionData(int ix, int iy, int iz)
        {
            const double amplitude = 2.0;

            double x = ix;
            double y = iy;
            double z = iz;

            double w = Screen.PrimaryScreen.Bounds.Width;
            double h = Screen.PrimaryScreen.Bounds.Height;

            // remove offset if the mouse is on the second screen
            if (x > w)
                x -= w;

            values[0] = +((x / w) - 0.5) * 2.0 * amplitude;
            values[1] = -((y / h) - 0.5) * 2.0 * amplitude;
            values[2] = -((z / h) - 0.5) * 2.0 * amplitude;
        }
    }
}
