using System;
using System.Threading.Tasks;

namespace Volvox.Helios.Core.Modules.Common
{
    public interface IModule
    {
        Task Execute();
    }
}