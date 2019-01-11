using Volvox.Helios.Core.Modules.ModerationModule.Filters;
using Volvox.Helios.Domain.Module.ModerationModule.Common;

namespace Volvox.Helios.Core.Modules.ModerationModule.Utils.FilterFactory
{
    public interface IFilterFactory
    {
        IFilterService GetFilter(FilterType type);
    }
}
