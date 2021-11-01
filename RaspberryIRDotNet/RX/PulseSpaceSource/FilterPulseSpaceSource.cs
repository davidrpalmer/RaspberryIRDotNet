using System;
using System.Collections.Generic;
using System.Linq;
using RaspberryIRDotNet.RX.Filters;

namespace RaspberryIRDotNet.RX.PulseSpaceSource
{
    /// <summary>
    /// Apply filtering to a PULSE/SPACE source.
    /// </summary>
    /// <remarks>
    /// Sometimes filtering is implemented in the class that processed the IR burst so that good and bad messages can
    /// both be processed. For those classes, using this class is unnecessary and a bad idea. However some classes
    /// don't provide their own filtering, for those classes this can be used to add filtering transparently.
    /// </remarks>
    public class FilterPulseSpaceSource : IPulseSpaceSource
    {
        public MultipleFilters Filters { get; set; } = new MultipleFilters();

        private readonly IPulseSpaceSource _source;

        public bool RealTime => _source.RealTime;

        public event EventHandler<ReceivedPulseSpaceBurstEventArgs> ReceivedPulseSpaceBurst;

        public FilterPulseSpaceSource(IPulseSpaceSource source, params IRXFilter[] filters) : this(source, (IEnumerable<IRXFilter>)filters)
        {
        }

        public FilterPulseSpaceSource(IPulseSpaceSource source, IEnumerable<IRXFilter> filters)
        {
            _source = source ?? throw new ArgumentNullException();
            _source.ReceivedPulseSpaceBurst += OnReceivedPulseSpaceBurst;

            if (filters?.Any() == true)
            {
                Filters.Filters.AddRange(filters);
                Filters.AssertConfigOK();
            }
        }

        private void OnReceivedPulseSpaceBurst(object sender, ReceivedPulseSpaceBurstEventArgs e)
        {
            if (ReceivedPulseSpaceBurst != null && Filters.Check(e.Buffer))
            {
                ReceivedPulseSpaceBurst?.Invoke(this, e);
            }
        }

        public void Capture(ReadCancellationToken cancellationToken)
        {
            if (Filters == null)
            {
                throw new ArgumentNullException(nameof(Filters));
            }
            Filters.AssertConfigOK();

            _source.Capture(cancellationToken);
        }
    }
}
