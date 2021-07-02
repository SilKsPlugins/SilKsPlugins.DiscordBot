using Microsoft.EntityFrameworkCore;
using SilKsPlugins.DiscordBot.Databases.Plugins;
using SilKsPlugins.DiscordBot.Databases.Plugins.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Plugins.Services
{
    public class PluginManager
    {
        private readonly PluginsDbContext _dbContext;

        public PluginManager(PluginsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PluginInfo?> GetPlugin(string pluginId)
        {
            return await _dbContext.Plugins.Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == pluginId);
        }

        public async Task<ICollection<PluginInfo>> GetPlugins()
        {
            return await _dbContext.Plugins.Include(x => x.Category)
                .ToListAsync();
        }

        public async Task<bool> AddPlugin(PluginInfo plugin)
        {
            var existingPlugin = await GetPlugin(plugin.Id);

            if (existingPlugin != null)
            {
                return false;
            }

            await _dbContext.Plugins.AddAsync(plugin);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemovePlugin(string pluginId)
        {
            var plugin = await GetPlugin(pluginId);

            if (plugin == null)
            {
                return false;
            }

            _dbContext.Plugins.Remove(plugin);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePlugin(string pluginId, Action<PluginInfo> updateQuery)
        {
            var plugin = await GetPlugin(pluginId);

            if (plugin == null)
            {
                return false;
            }

            updateQuery(plugin);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<CategoryInfo?> GetCategory(string categoryId)
        {
            return await _dbContext.Categories.Include(x => x.Plugins)
                .FirstOrDefaultAsync(x => x.Id == categoryId);
        }

        public async Task<ICollection<CategoryInfo>> GetCategories()
        {
            return await _dbContext.Categories.Include(x => x.Plugins)
                .ToListAsync();
        }

        public async Task<bool> AddCategory(CategoryInfo category)
        {
            var existingCategory = await GetCategory(category.Id);

            if (existingCategory != null)
            {
                return false;
            }

            await _dbContext.Categories.AddAsync(category);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveCategory(string categoryId)
        {
            var category = await GetCategory(categoryId);

            if (category == null)
            {
                return false;
            }

            _dbContext.Categories.Remove(category);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateCategory(string categoryId, Action<CategoryInfo> updateQuery)
        {
            var category = await GetCategory(categoryId);

            if (category == null)
            {
                return false;
            }

            updateQuery(category);

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
