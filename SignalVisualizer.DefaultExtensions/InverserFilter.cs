using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.DefaultExtensions
{
    [Export(typeof(ISignalFilterFactory))]
    public class InverserFilterFactory : ISignalFilterFactory
    {
        public string FilterName { get; } = "Inverser";
        public Guid UniqueIdentifier { get; } = new Guid("12690ecb-c7fc-4f00-8ae6-7fcc1734601a");

        public ISignalFilter ProduceSignalFilter()
        {
            return new InverserFilter(this);
        }
    }

    internal class InverserFilter : SignalFilterBase
    {
        internal InverserFilter(ISignalFilterFactory signalFactory)
            : base(signalFactory)
        {
        }

        public override double ProcessValue(double time, double value) => -value;
    }
}
