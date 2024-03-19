using Claims.Auditing;
namespace Claims.Services;

public class CoverService: ICoverService
{
    private readonly ILogger<ICoverService> _logger;
    private readonly IAuditer _auditer;
    private readonly ICosmosDbService _cosmosDbService;

    public CoverService(
        ILogger<ICoverService> logger, 
        IAuditer auditer, 
        ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _auditer = auditer;
        _cosmosDbService = cosmosDbService;
    }

    public async Task<IEnumerable<Cover>> GetCoversAsync()
    {
        return await _cosmosDbService.GetCoversAsync();
    }

    public Task<Cover> GetCoverAsync(string id)
    {
        return _cosmosDbService.GetCoverAsync(id);
    }

    public async Task CreateCoverAsync(Cover cover)
    {
        ValidateCover(cover);
        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = ComputePremium(cover.StartDate, cover.EndDate, cover.Type);
        await _cosmosDbService.AddCoverAsync(cover);
        _auditer.AuditCover(cover.Id, "POST");
        _logger.LogInformation($"Cover with id: {cover.Id} created");
    }

    public Task DeleteCoverAsync(string id)
    {
        _auditer.AuditCover(id, "DELETE");
        _logger.LogInformation($"Cover with id: {id} deleted");
        return _cosmosDbService.DeleteCoverAsync(id);
    }
    
    public decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        var multiplier = coverType switch
        {
            CoverType.Yacht => 1.1m,
            CoverType.PassengerShip => 1.2m,
            CoverType.Tanker => 1.5m,
            _ => 1.3m
        };

        var premiumPerDay = 1250 * multiplier;
        var insuranceLength = endDate.DayNumber - startDate.DayNumber;
        var totalPremium = 0m;

        for (var i = 0; i < insuranceLength; i++)
        {
            switch (i)
            {
                // First 30 days unchanged
                case < 30:
                    totalPremium += premiumPerDay;
                    break;
                // the following 150 days discounted 5% for yachts
                case < 180 when coverType == CoverType.Yacht:
                    totalPremium += premiumPerDay - premiumPerDay * 0.05m;
                    break;
                // the following 150 days discounted 2% for others
                case < 180:
                    totalPremium += premiumPerDay - premiumPerDay * 0.02m;
                    break;
                // the remaining days are discounted additional 3% for yachts
                case < 365 when coverType == CoverType.Yacht:
                    totalPremium += premiumPerDay - premiumPerDay * 0.08m;
                    break;
                // the remaining days are discounted additional 1% for others
                case < 365:
                    totalPremium += premiumPerDay - premiumPerDay * 0.03m;
                    break;
            }
        }

        return totalPremium;
    }

    private void ValidateCover(Cover cover)
    {
        if (cover.StartDate < DateOnly.FromDateTime(DateTime.Now))
        {
            throw new ArgumentException("Cover StartDate is in the past");
        }

        if (cover.StartDate > cover.EndDate)
        {
            throw new ArgumentException("EndDate is before Startdate");
        }

        if (cover.EndDate.DayNumber - cover.StartDate.DayNumber > 365)
        {
            throw new ArgumentException("The insurance period exceeds 1 year");
        }
    }
}
