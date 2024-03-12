using Claims.Auditing;
using Claims.Controllers;
namespace Claims.Services;

public class ClaimService: IClaimService
{
    private readonly ILogger<ClaimsController> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly Auditer _auditer;
    private readonly ICoverService _coverService;

    public ClaimService(
        ILogger<ClaimsController> logger, 
        ICosmosDbService cosmosDbService, 
        AuditContext auditContext,
        ICoverService coverService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _auditer = new Auditer(auditContext);
        _coverService = coverService;
    }

    public async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        return await _cosmosDbService.GetClaimsAsync();
    }

    public async Task CreateClaimAsync(Claim claim)
    {
        await ValidateClaim(claim);
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

    private async Task ValidateClaim(Claim claim)
    {
        if (claim.DamageCost > 100000)
        {
            throw new ArgumentException("DamageCost of Claim exceeds maximum");
        }

        var relatedCover = await _coverService.GetCoverAsync(claim.CoverId);
        var createdTime = DateOnly.FromDateTime(claim.Created);
        if (createdTime < relatedCover.StartDate || createdTime > relatedCover.EndDate)
        {
            throw new ArgumentException("The Claim is created outside the Cover period");
        }
    }
}
