using Azure.Core;
using Microsoft.Azure.Cosmos;

namespace AzureXConnect.Models;

public class CosmosDbOptions
{
    public IEnumerable<CosmosDbAccountOptions> Accounts { get; set; } = [];
}

public class CosmosDbAccountOptions
{
    public string AccountName { get; set; } = string.Empty;

    public IEnumerable<CosmosDbDatabase> Databases { get; set; } = [];

    public CosmosClientOptions? CosmosClientOptions { get; set; }

    public TokenCredential? Credential { get; set; }

    public string ConnectionString { get; set; } = string.Empty;
}

public class CosmosDbDatabase
{
    public string Name { get; set; } = string.Empty;
    public IEnumerable<CosmosDbContainer> Containers { get; set; } = [];
}

public class CosmosDbContainer
{
    public string Name { get; set; } = string.Empty;

    public Type? Model { get; set; }
}
