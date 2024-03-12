using Claims.Auditing;
using Claims.Controllers;
namespace Claims.Services;

public class ClaimService: IClaimService
{
    private readonly ILogger<ClaimsController> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly Auditer _auditer;

    public ClaimService(ILogger<ClaimsController> logger, ICosmosDbService cosmosDbService, AuditContext auditContext)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _auditer = new Auditer(auditContext);
    }

    public async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        return await _cosmosDbService.GetClaimsAsync();
    }

    public async Task CreateClaimAsync(Claim claim)
    {
        claim.Id = Guid.NewGuid().ToString();
        await _cosmosDbService.AddClaimAsync(claim);
        _auditer.AuditClaim(claim.Id, "POST");
        _logger.LogInformation($"Claim with id: {claim.Id} created");
    }

    public Task DeleteClaimAsync(string id)
    {
        _auditer.AuditClaim(id, "DELETE");
        _logger.LogInformation($"Claim with id: {id} deleted");
        return _cosmosDbService.DeleteClaimAsync(id);
    }

    public Task<Claim> GetClaimAsync(string id)
    {
        return _cosmosDbService.GetClaimAsync(id);
    }
}
