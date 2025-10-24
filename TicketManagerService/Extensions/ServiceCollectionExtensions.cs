using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSCatch.Extensions;
using NSCatch.Interfaces;
using NSCore.DatabaseContext;
using NSCore.DbContextHelper;
using NSCore.Models;
using TicketManagerService.Data;
using TicketManagerService.Interfaces;

namespace TicketManagerService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTicketManager(
        this IServiceCollection services,
        IDatabaseConfig inputModel,
        DbSetupOptions? dbOptions = null,
        ICatchOption? catchOption = null,
        bool applyMigrationsAutomatically = true
    )
    {
        services.AddCustomDbContextFactory<TicketManagerDbContext>(inputModel, dbOptions);
        services.AddNSCache(catchOption, "TicketManager");
        services.AddSingleton<IManageTickets>(provider =>
        {
            var contextFactory = provider.GetRequiredService<IDbContextFactory<TicketManagerDbContext>>();

            ICacheManager? cacheManager = provider.GetKeyedService<ICacheManager>("TicketManager");
            ICacheKeyBuilder? keyBuilder = provider.GetKeyedService<ICacheKeyBuilder>("TicketManager");

            if (cacheManager is null || keyBuilder is null) throw new InvalidOperationException("Cache services are not properly configured for TicketManager.");
            return new ManageTickets(inputModel, contextFactory, cacheManager, keyBuilder, applyMigrationsAutomatically);
        });

        services.AddSingleton<IHostedService>(provider =>
        {
            var ManageTicketsService = provider.GetRequiredService<IManageTickets>();
            return (ManageTickets)ManageTicketsService;
        });
        
        return services;
    } 
}
