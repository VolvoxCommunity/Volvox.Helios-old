using System;
using System.Collections.Generic;
using System.Text;

namespace Volvox.Helios.Domain.Module.ModerationModule.Common
{
    public enum PunishType { None = 0, Mute = 1, Kick = 2, Ban = 3 }

    public enum WarningType { General = 0, Profanity = 1, Link = 2 }

    public enum WhitelistType { Global = 0, Profanity = 1, Link = 2}
}
