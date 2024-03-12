namespace Claims.Services;

public interface ICoverService
{
    public decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType);
    public Task<IEnumerable<Cover>> GetCoversAsync();
    public Task<Cover> GetCoverAsync(string id);
    public Task CreateCoverAsync(Cover cover);
    public Task DeleteCoverAsync(string id);
}
