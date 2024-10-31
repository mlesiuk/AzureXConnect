using AzureXConnect.Models;
using AzureXConnect.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace AzureXConnect;

public static class DependencyInjection
{
    public static IServiceCollection AddCosmosDbRepositories(
        this IServiceCollection serviceCollection,
        Action<CosmosDbAccountOptions> options)
    {
        var cosmosDbAccountOptions = new CosmosDbAccountOptions();
        options.Invoke(cosmosDbAccountOptions);

        if (string.IsNullOrEmpty(cosmosDbAccountOptions?.AccountName))
        {
            return serviceCollection;
        }

        serviceCollection.TryAddKeyedSingleton(cosmosDbAccountOptions.AccountName, (sp, obj) =>
        {
            if (!string.IsNullOrEmpty(cosmosDbAccountOptions.ConnectionString))
            {
                return new CosmosClient(cosmosDbAccountOptions.ConnectionString);
            }
            else
            {
                if (cosmosDbAccountOptions.Credential != null && !string.IsNullOrEmpty(cosmosDbAccountOptions.AccountName))
                {
                    return new CosmosClient($"https://{cosmosDbAccountOptions.AccountName}.documents.azure.com:443/", cosmosDbAccountOptions.Credential);
                }
                else
                {
                    throw new Exception($"Cannot create CosmosClient: {cosmosDbAccountOptions.AccountName}.");
                }
            }
        });

        foreach (var database in cosmosDbAccountOptions.Databases)
        {
            var containers = database.Containers;
            foreach (var container in containers)
            {
                var containerName = container.Name;
                var containerModelName = container.Model?.Name;
                if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(containerModelName))
                {
                    continue;
                }

                var serviceDescriptor = Assembly.GetExecutingAssembly().DefinedTypes
                    .Where(s =>
                        s.IsAssignableTo(typeof(IRepository)) &&
                        s.Name.EndsWith("Repository") &&
                        s.Name.Contains(containerModelName))
                    .ToList();

                var repositoryInterface = serviceDescriptor
                    .FirstOrDefault(sd => sd.IsInterface);

                var repositoryImplementation = serviceDescriptor
                    .FirstOrDefault(sd => !sd.IsInterface);

                if (repositoryInterface is not null && repositoryImplementation is not null)
                {
                    if (serviceCollection.Any(s => s.ServiceType.Name.Equals(repositoryInterface.Name)))
                    {
                        throw new Exception($"Service already registered: {repositoryImplementation.Name}.");
                    }

                    serviceCollection.TryAddSingleton(repositoryInterface, (serviceProvider) =>
                    {
                        var cosmosClient = serviceProvider.GetKeyedService<CosmosClient>(cosmosDbAccountOptions.AccountName);
                        var cont = cosmosClient?
                            .GetDatabase(database.Name)
                            .GetContainer(containerName) ?? throw new Exception($"Lack of registered CosmosClient: {cosmosDbAccountOptions.AccountName}.");

                        var repository = Activator.CreateInstance(repositoryImplementation, [cont]) ?? throw new Exception($"Lack of repository implementation {repositoryImplementation.Name}.");
                        return repository;
                    });
                }
            }
        }
        return serviceCollection;
    }

    public static IServiceCollection AddCosmosDbRepositories(
        this IServiceCollection serviceCollection, 
        Action<CosmosDbOptions> options)
    {
        var cosmosDbOptions = new CosmosDbOptions();
        options.Invoke(cosmosDbOptions);

        foreach (var cosmosDb in cosmosDbOptions.Accounts)
        {
            serviceCollection.AddCosmosDbRepositories(options =>
            {
                options.AccountName = cosmosDb.AccountName;
                options.ConnectionString = cosmosDb.ConnectionString;
                options.Credential = cosmosDb.Credential;
                options.Databases = cosmosDb.Databases;
            });                
        }
        return serviceCollection;
    }
}
