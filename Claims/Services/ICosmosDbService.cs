namespace Claims.Services;

public interface ICosmosDbService
{
    public Task<IEnumerable<Claim>> GetClaimsAsync();
    public Task<IEnumerable<Cover>> GetCoversAsync();
    public Task<Claim> GetClaimAsync(string id);
    public Task<Cover> GetCoverAsync(string id);
    public Task AddClaimAsync(Claim item);
    public Task AddCoverAsync(Cover item);
    public Task DeleteClaimAsync(string id);
    public Task DeleteCoverAsync(string id);
}
