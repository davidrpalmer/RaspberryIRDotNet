using System;
using System.Collections.Generic;

namespace RaspberryIRDotNet.RX.Filters
{
    public static class FilterExtensions
    {
        public static IEnumerable<IReadOnlyPulseSpaceDurationList> Filter(this IEnumerable<IReadOnlyPulseSpaceDurationList> bursts, IRXFilter filter)
        {
            foreach (var burst in bursts)
            {
                if (filter.Check(burst))
                {
                    yield return burst;
                }
            }
        }
    }
}
