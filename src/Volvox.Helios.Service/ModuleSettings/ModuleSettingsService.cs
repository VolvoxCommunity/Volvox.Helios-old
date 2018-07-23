using System.Threading.Tasks;

namespace Volvox.Helios.Service.ModuleSettings
{
    /// <summary>
    /// Service to handle module settings.
    /// </summary>
    /// <typeparam name="T">Type of module setting.</typeparam>
    public class ModduleSettingsService<T> : IModuleSettingsService<T> where T : Domain.ModuleSettings.ModuleSettings
    {
        private readonly VolvoxHeliosContext _context;

        public ModduleSettingsService(VolvoxHeliosContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Save the module settings to the database.
        /// </summary>
        /// <param name="settings">Module settings to save.</param>
        public async Task SaveSettings(T settings)
        {
            await _context.AddAsync(settings);
            await _context.SaveChangesAsync();
        }
    }
}