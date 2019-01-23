using System.Collections.Generic;
using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.Factories.FilterFactory
{
    public class FilterFactory : IFilterFactory
    {
        private readonly Dictionary<FilterType, IFilterService> _filters = new Dictionary<FilterType, IFilterService>();

        public FilterFactory(IEnumerable<IFilterService> filters)
        {
            foreach (var filter in filters)
            {
                var filterType = filter.GetFilterMetaData().FilterType;

                _filters[filterType] = filter;
            }
        }

        /// <inheritdoc />
        public IFilterService GetFilter(FilterType type)
        {
            return _filters[type];
        }
    }
}