using System.Collections.Generic;

using FluentValidation;

using MediatR.CommandQuery.Behaviors;
using MediatR.CommandQuery.Commands;
using MediatR.CommandQuery.Definitions;
using MediatR.CommandQuery.Extensions;
using MediatR.CommandQuery.MongoDB.Handlers;
using MediatR.CommandQuery.Queries;
using MediatR.CommandQuery.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoDB.Abstracts;

namespace MediatR.CommandQuery.MongoDB
{
    public static class DomainServiceExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            // Register MediatR
            services.TryAddScoped<ServiceFactory>(p => p.GetService);
            services.TryAddScoped<IMediator, Mediator>();
            services.TryAddScoped<ISender, Mediator>();
            services.TryAddScoped<IPublisher, Mediator>();

            return services;
        }

        public static IServiceCollection AddValidatorsFromAssembly<T>(this IServiceCollection services)
        {
            // Register validators
            var scanner = AssemblyScanner.FindValidatorsInAssemblyContaining<T>();
            foreach (var scanResult in scanner)
            {
                //Register as interface
                services.TryAdd(new ServiceDescriptor(scanResult.InterfaceType, scanResult.ValidatorType, ServiceLifetime.Singleton));
                //Register as self
                services.TryAdd(new ServiceDescriptor(scanResult.ValidatorType, scanResult.ValidatorType, ServiceLifetime.Singleton));
            }

            return services;
        }


        public static IServiceCollection AddEntityQueries<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
        {
            // standard queries
            services.TryAddScoped<IRequestHandler<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, EntityIdentifierQueryHandler<TRepository, TEntity, TKey, TReadModel>>();
            services.TryAddScoped<IRequestHandler<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, EntityIdentifiersQueryHandler<TRepository, TEntity, TKey, TReadModel>>();
            services.TryAddScoped<IRequestHandler<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, EntityPagedQueryHandler<TRepository, TEntity, TKey, TReadModel>>();
            services.TryAddScoped<IRequestHandler<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, EntitySelectQueryHandler<TRepository, TEntity, TKey, TReadModel>>();

            // pipeline registration, run in order registered
            bool supportsTenant = typeof(TReadModel).Implements<IHaveTenant<TKey>>();
            if (supportsTenant)
            {
                services.AddScoped<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, TenantPagedQueryBehavior<TKey, TReadModel>>();
                services.AddScoped<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, TenantSelectQueryBehavior<TKey, TReadModel>>();
            }

            bool supportsDeleted = typeof(TReadModel).Implements<ITrackDeleted>();
            if (supportsDeleted)
            {
                services.AddScoped<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, DeletedPagedQueryBehavior<TReadModel>>();
                services.AddScoped<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, DeletedSelectQueryBehavior<TReadModel>>();
            }

            return services;
        }

        public static IServiceCollection AddEntityQueryMemoryCache<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
        {
            services.AddScoped<IPipelineBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, MemoryCacheQueryBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>>();
            services.AddScoped<IPipelineBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, MemoryCacheQueryBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>>();
            services.AddScoped<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, MemoryCacheQueryBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>>();
            services.AddScoped<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, MemoryCacheQueryBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>>();

            return services;
        }

        public static IServiceCollection AddEntityQueryDistributedCache<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
        {
            services.AddScoped<IPipelineBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>, DistributedCacheQueryBehavior<EntityIdentifierQuery<TKey, TReadModel>, TReadModel>>();
            services.AddScoped<IPipelineBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>, DistributedCacheQueryBehavior<EntityIdentifiersQuery<TKey, TReadModel>, IReadOnlyCollection<TReadModel>>>();
            services.AddScoped<IPipelineBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>, DistributedCacheQueryBehavior<EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>>();
            services.AddScoped<IPipelineBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, DistributedCacheQueryBehavior<EntitySelectQuery<TReadModel>, IReadOnlyCollection<TReadModel>>>();

            return services;
        }


        public static IServiceCollection AddEntityCommands<TRepository, TEntity, TKey, TReadModel, TCreateModel, TUpdateModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
            where TCreateModel : class
            where TUpdateModel : class
        {
            services.TryAddSingleton<IPrincipalReader, PrincipalReader>();

            services
                .AddEntityCreateCommand<TRepository, TEntity, TKey, TReadModel, TCreateModel>()
                .AddEntityUpdateCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>()
                .AddEntityUpsertCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>()
                .AddEntityPatchCommand<TRepository, TEntity, TKey, TReadModel>()
                .AddEntityDeleteCommand<TRepository, TEntity, TKey, TReadModel>();

            return services;
        }


        public static IServiceCollection AddEntityCreateCommand<TRepository, TEntity, TKey, TReadModel, TCreateModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
            where TCreateModel : class
        {

            // standard crud commands
            services.TryAddTransient<IRequestHandler<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, EntityCreateCommandHandler<TRepository, TEntity, TKey, TCreateModel, TReadModel>>();

            // pipeline registration, run in order registered
            var createType = typeof(TCreateModel);
            bool supportsTenant = createType.Implements<IHaveTenant<TKey>>();
            if (supportsTenant)
            {
                services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, TenantDefaultCommandBehavior<TKey, TCreateModel, TReadModel>>();
                services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, TenantAuthenticateCommandBehavior<TKey, TCreateModel, TReadModel>>();
            }

            bool supportsTracking = createType.Implements<ITrackCreated>();
            if (supportsTracking)
                services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, TrackChangeCommandBehavior<TCreateModel, TReadModel>>();

            services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, ValidateEntityModelCommandBehavior<TCreateModel, TReadModel>>();
            services.AddTransient<IPipelineBehavior<EntityCreateCommand<TCreateModel, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TCreateModel, TReadModel>>();

            return services;
        }

        public static IServiceCollection AddEntityUpdateCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
            where TUpdateModel : class
        {

            // allow query for update models
            services.TryAddTransient<IRequestHandler<EntityIdentifierQuery<TKey, TUpdateModel>, TUpdateModel>, EntityIdentifierQueryHandler<TRepository, TEntity, TKey, TUpdateModel>>();
            services.TryAddTransient<IRequestHandler<EntityIdentifiersQuery<TKey, TUpdateModel>, IReadOnlyCollection<TUpdateModel>>, EntityIdentifiersQueryHandler<TRepository, TEntity, TKey, TUpdateModel>>();

            // standard crud commands
            services.TryAddTransient<IRequestHandler<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, EntityUpdateCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>>();

            // pipeline registration, run in order registered
            var updateType = typeof(TUpdateModel);
            bool supportsTenant = updateType.Implements<IHaveTenant<TKey>>();
            if (supportsTenant)
            {
                services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TenantDefaultCommandBehavior<TKey, TUpdateModel, TReadModel>>();
                services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TenantAuthenticateCommandBehavior<TKey, TUpdateModel, TReadModel>>();
            }

            bool supportsTracking = updateType.Implements<ITrackUpdated>();
            if (supportsTracking)
                services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TrackChangeCommandBehavior<TUpdateModel, TReadModel>>();

            services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, ValidateEntityModelCommandBehavior<TUpdateModel, TReadModel>>();
            services.AddTransient<IPipelineBehavior<EntityUpdateCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TUpdateModel, TReadModel>>();

            return services;
        }

        public static IServiceCollection AddEntityUpsertCommand<TRepository, TEntity, TKey, TReadModel, TUpdateModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
            where TUpdateModel : class
        {
            // standard crud commands
            services.TryAddTransient<IRequestHandler<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, EntityUpsertCommandHandler<TRepository, TEntity, TKey, TUpdateModel, TReadModel>>();

            // pipeline registration, run in order registered
            var updateType = typeof(TUpdateModel);
            bool supportsTenant = updateType.Implements<IHaveTenant<TKey>>();
            if (supportsTenant)
            {
                services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TenantDefaultCommandBehavior<TKey, TUpdateModel, TReadModel>>();
                services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TenantAuthenticateCommandBehavior<TKey, TUpdateModel, TReadModel>>();
            }

            bool supportsTracking = updateType.Implements<ITrackUpdated>();
            if (supportsTracking)
                services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, TrackChangeCommandBehavior<TUpdateModel, TReadModel>>();

            services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, ValidateEntityModelCommandBehavior<TUpdateModel, TReadModel>>();
            services.AddTransient<IPipelineBehavior<EntityUpsertCommand<TKey, TUpdateModel, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TUpdateModel, TReadModel>>();

            return services;
        }

        public static IServiceCollection AddEntityPatchCommand<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
        {
            // standard crud commands
            services.TryAddTransient<IRequestHandler<EntityPatchCommand<TKey, TReadModel>, TReadModel>, EntityPatchCommandHandler<TRepository, TEntity, TKey, TReadModel>>();

            // pipeline registration, run in order registered
            services.AddTransient<IPipelineBehavior<EntityPatchCommand<TKey, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TEntity, TReadModel>>();

            return services;
        }

        public static IServiceCollection AddEntityDeleteCommand<TRepository, TEntity, TKey, TReadModel>(this IServiceCollection services)
            where TRepository : IMongoRepository<TEntity, TKey>
            where TEntity : class, IHaveIdentifier<TKey>, new()
        {

            // standard crud commands
            services.TryAddTransient<IRequestHandler<EntityDeleteCommand<TKey, TReadModel>, TReadModel>, EntityDeleteCommandHandler<TRepository, TEntity, TKey, TReadModel>>();

            // pipeline registration, run in order registered
            services.AddTransient<IPipelineBehavior<EntityDeleteCommand<TKey, TReadModel>, TReadModel>, EntityChangeNotificationBehavior<TKey, TEntity, TReadModel>>();

            return services;
        }
    }
}