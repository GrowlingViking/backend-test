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
        var multiplier = 1.3m;
        if (coverType == CoverType.Yacht)
        {
            multiplier = 1.1m;
        }

        if (coverType == CoverType.PassengerShip)
        {
            multiplier = 1.2m;
        }

        if (coverType == CoverType.Tanker)
        {
            multiplier = 1.5m;
        }

        var premiumPerDay = 1250 * multiplier;
        var insuranceLength = endDate.DayNumber - startDate.DayNumber;
        var totalPremium = 0m;

        for (var i = 0; i < insuranceLength; i++)
        {
            if (i < 30) totalPremium += premiumPerDay;
            if (i < 180 && coverType == CoverType.Yacht) totalPremium += premiumPerDay - premiumPerDay * 0.05m;
            else if (i < 180) totalPremium += premiumPerDay - premiumPerDay * 0.02m;
            if (i < 365 && coverType != CoverType.Yacht) totalPremium += premiumPerDay - premiumPerDay * 0.03m;
            else if (i < 365) totalPremium += premiumPerDay - premiumPerDay * 0.08m;
        }

        return totalPremium;
    }

    private void ValidateCover(Cover cover)
    {
        if (cover.StartDate > DateOnly.FromDateTime(DateTime.Now))
        {
            throw new ArgumentException("Cover StartDate is in the past");
        }

        if (cover.StartDate > cover.EndDate)
        {
            throw new ArgumentException("EndDate is before Startdate");
        }

        if (cover.StartDate.DayNumber - cover.EndDate.DayNumber > 365)
        {
            throw new ArgumentException("The insurance period exceeds 1 year");
        }
    }
}
