namespace Volvox.Helios.Core.Modules.Common
{
    public interface IDocumented
    {
        string Name { get; }
        string Version { get; }
        string Synopsis { get; }
        ReleaseState ReleaseState { get; }
    }
}