using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SignalVisualizer.Contracts;

namespace CpuDataSourceExtension
{
    [Export(typeof(IDataSource))]
    public class CpuDataSource : DataSourceBase
    {
        public override string Name { get; } = "CPU meter";
        public override uint Version { get; } = 1;
        public override Guid UniqueIdentifier { get; } = new Guid("3c77228a-f646-4ce2-836b-d68e707f68a8");
        public override string[] ComponentNames { get; } = new string[] { "Processor time (%)", "Idle time (%)" };

        public override async Task Start(CancellationToken cancellationToken)
        {
            // allocates the storage for the values of the two components of the data source
            var values = new double[2];

            // create performance counters
            var totalCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            var currentProcessCpu = new PerformanceCounter("Processor", "% Idle Time", "_Total", true);

            // iterate while not cancelled
            while (cancellationToken.IsCancellationRequested == false)
            {
                // sample the performance counters values
                values[0] = totalCpu.NextValue();
                values[1] = currentProcessCpu.NextValue();

                // notify SignalVisualizer that a new value has been produced
                NotifyNext(values);

                // awaits to avoid burn out, then loop and restart
                await Task.Delay(250);
            }
        }
    }
}
