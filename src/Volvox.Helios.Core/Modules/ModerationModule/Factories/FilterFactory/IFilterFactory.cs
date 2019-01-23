using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.Factories.FilterFactory
{
    public interface IFilterFactory
    {
        /// <summary>
        ///     Gets filter service by type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IFilterService GetFilter(FilterType type);
    }
}