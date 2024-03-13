using Microsoft.Azure.Cosmos;
namespace Claims.Services;

public class CosmosDbService: ICosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(CosmosClient dbClient,
        string databaseName,
        string containerName)
    {
        if (dbClient == null) throw new ArgumentNullException(nameof(dbClient));
        _container = dbClient.GetContainer(databaseName, containerName);
    }

    public async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        var query = _container.GetItemQueryIterator<Claim>(new QueryDefinition("SELECT * FROM c"));
        var results = new List<Claim>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();

            results.AddRange(response.ToList());
        }
        return results;
    }
    
    public async Task<IEnumerable<Cover>> GetCoversAsync()
    {
        var query = _container.GetItemQueryIterator<Cover>(new QueryDefinition("SELECT * FROM c"));
        var results = new List<Cover>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();

            results.AddRange(response.ToList());
        }
        return results;
    }


    public async Task<Claim> GetClaimAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Claim>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
    
    public async Task<Cover> GetCoverAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Cover>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task AddClaimAsync(Claim item)
    {
        return _container.CreateItemAsync(item, new PartitionKey(item.Id));
    }
    
    public Task AddCoverAsync(Cover item)
    {
        return _container.CreateItemAsync(item, new PartitionKey(item.Id));
    }

    public Task DeleteClaimAsync(string id)
    {
        return _container.DeleteItemAsync<Claim>(id, new PartitionKey(id));
    }
    
    public Task DeleteCoverAsync(string id)
    {
        return _container.DeleteItemAsync<Cover>(id, new PartitionKey(id));
    }
}
