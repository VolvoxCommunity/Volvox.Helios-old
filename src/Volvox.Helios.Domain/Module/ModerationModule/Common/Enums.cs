using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    public enum PunishType {Kick = 0, Ban = 1, AddRole = 2 }

    // Global represents all filters, e.g. a punishment tier set to ban after 3 'Global' warnings will ban after 3 warnings of any type (i.e. 2 links and 1 profanity).
    public enum FilterType {Global = 0, Profanity = 1, Link = 2 }
}
