namespace Volvox.Helios.Core.Modules.Common
{
    public interface IDocumented
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }
        bool Configurable { get; }
        ReleaseState ReleaseState { get; }
    }
}