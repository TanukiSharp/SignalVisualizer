using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalVisualizer.Contracts;
using System.Threading;
using System.ComponentModel.Composition;

namespace SignalVisualizer.DefaultExtensions
{
    [Export(typeof(IDataSource))]
    public class Test1DataSource : DataSourceBase
    {
        private double valueX = 0.0;
        private double valueY = 0.0;
        private double valueZ = 0.0;

        public override uint Version { get; } = 2;
        public override string Name { get; } = "Test1 Data Source";
        public override Guid UniqueIdentifier { get; } = new Guid("d8dd66f9-f5e8-4bc5-bbf8-66e1e97b24e8");
        public override string[] ComponentNames { get; } = new string[] { "Sawtooth [-1;+3]", "Random [-1;+3]", "Random [-1;+1]" };

        private readonly double[] values = new double[3];

        public async override Task Start(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(1);

                if (cancellationToken.IsCancellationRequested)
                {
                    NotifyCompleted();
                    break;
                }

                valueX += 0.05;
                if (valueX > 3.0)
                    valueX = -1.0;

                GenerateValue(ref valueY, -1.0, 3.0);
                GenerateValue(ref valueZ);

                values[0] = valueX;
                values[1] = valueY;
                values[2] = valueZ;

                NotifyNext(values);
            }
        }

        private static readonly Random random = new Random(Environment.TickCount);

        private void GenerateValue(ref double value, double min = -1.0, double max = 1.0)
        {
            var delta = random.NextDouble() * 0.1 - 0.05;

            var temp = value + delta;
            if (temp > min && temp < max)
                value = temp;
        }
    }

    // ====================================================================================

    [Export(typeof(IDataSource))]
    public class Test2DataSource : DataSourceBase
    {
        private double value = 0.0;

        public override string Name { get; } = "Test2 Data Source";
        public override Guid UniqueIdentifier { get; } = new Guid("b25ba6a1-cbba-4976-afea-df6ad779fd48");
        public override uint Version { get; } = 2;
        public override string[] ComponentNames { get; } = new string[] { "Random HF [-1;+1]" };

        private readonly double[] values = new double[1];

        public async override Task Start(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(1);

                if (cancellationToken.IsCancellationRequested)
                {
                    NotifyCompleted();
                    break;
                }

                GenerateValue(ref value);

                values[0] = value;

                NotifyNext(values);
            }
        }

        private static readonly Random random = new Random(Environment.TickCount);

        private void GenerateValue(ref double value)
        {
            var delta = random.NextDouble() * 0.6 - 0.3;

            var temp = value + delta;
            if (temp > -1.0 && temp < 1.0)
                value = temp;
        }
    }
}
