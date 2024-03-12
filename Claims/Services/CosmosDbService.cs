using Microsoft.Azure.Cosmos;
namespace Claims.Services;

public class CosmosDbService: ICosmosDbService
{
    private readonly Container _claimsContainer;
    private readonly Container _coversContainer;

    public CosmosDbService(CosmosClient dbClient,
        string databaseName,
        string containerName)
    {
        if (dbClient == null) throw new ArgumentNullException(nameof(dbClient));
        _claimsContainer = dbClient.GetContainer(databaseName, containerName);
        _coversContainer = dbClient.GetContainer(databaseName, "Cover");
    }

    public async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        var query = _claimsContainer.GetItemQueryIterator<Claim>(new QueryDefinition("SELECT * FROM c"));
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
        var query = _coversContainer.GetItemQueryIterator<Cover>(new QueryDefinition("SELECT * FROM c"));
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
            var response = await _claimsContainer.ReadItemAsync<Claim>(id, new PartitionKey(id));
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
            var response = await _coversContainer.ReadItemAsync<Cover>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task AddClaimAsync(Claim item)
    {
        return _claimsContainer.CreateItemAsync(item, new PartitionKey(item.Id));
    }
    
    public Task AddCoverAsync(Cover item)
    {
        return _coversContainer.CreateItemAsync(item, new PartitionKey(item.Id));
    }

    public Task DeleteClaimAsync(string id)
    {
        return _claimsContainer.DeleteItemAsync<Claim>(id, new PartitionKey(id));
    }
    
    public Task DeleteCoverAsync(string id)
    {
        return _coversContainer.DeleteItemAsync<Claim>(id, new PartitionKey(id));
    }
}
