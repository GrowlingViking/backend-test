namespace Claims.Services;

public interface IClaimService
{
    public Task<IEnumerable<Claim>> GetClaimsAsync();
    public Task CreateClaimAsync(Claim claim);
    public Task DeleteClaimAsync(string id);
    public Task<Claim> GetClaimAsync(string id);
}
