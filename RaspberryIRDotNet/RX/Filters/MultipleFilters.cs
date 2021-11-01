using System;
using System.Collections.Generic;
using System.Linq;

namespace RaspberryIRDotNet.RX.Filters
{
    public class MultipleFilters : IRXFilter
    {
        /// <summary>
        /// If one or more of these filters blocks IR then it won't pass. If none block it then it will pass.
        /// </summary>
        public List<IRXFilter> Filters { get; set; } = new List<IRXFilter>();

        /// <summary>
        /// Define if having 0 filters will cause <see cref="AssertConfigOK"/> to throw.
        /// </summary>
        public bool ZeroFiltersIsOK { get; set; } = true;

        /// <summary>
        /// Remove all other filters of this type and add this one.
        /// </summary>
        /// <typeparam name="T">The type of filter to replace.</typeparam>
        /// <param name="filter">The new filter to add, or null to just remove existing filters of this type.</param>
        /// <param name="addToTop">TRUE to add the filter so it will be processed before all others that are currently here, FALSE to process it after all other filters.</param>
        public void SetSingleInstanceFilter<T>(T filter, bool addToTop = false) where T : IRXFilter
        {
            Filters.RemoveAll(f => f is T);
            if (filter != null)
            {
                if (addToTop)
                {
                    Filters.Insert(0, filter);
                }
                else
                {
                    Filters.Add(filter);
                }
            }
        }

        public void AssertConfigOK()
        {
            if (Filters == null)
            {
                throw new ArgumentNullException(nameof(Filters));
            }

            if (!ZeroFiltersIsOK && Filters.Count == 0)
            {
                throw new ArgumentException("No filters have been added.");
            }

            if (Filters.Any(f => f == null))
            {
                throw new ArgumentException("Cannot have a null filter.");
            }

            Filters.ForEach(x => x.AssertConfigOK());
        }

        public bool Check(IReadOnlyPulseSpaceDurationList pulseSpaceDurations)
        {
            return Filters.All(x => x.Check(pulseSpaceDurations));
        }

        /// <summary>
        /// Don't filter IR, just allow it all through. (Clears the filters.)
        /// </summary>
        public void PassAll()
        {
            ZeroFiltersIsOK = true;
            Filters.Clear();
        }
    }
}
