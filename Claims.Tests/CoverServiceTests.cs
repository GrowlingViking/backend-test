using Claims.Auditing;
using Claims.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Claims.Tests;

public class CoverServiceTests
{
    private readonly ICoverService _coverService;
    
    private readonly Mock<ILogger<ICoverService>> _loggerMock;
    private readonly Mock<IAuditer> _auditerMock;
    private readonly Mock<ICosmosDbService> _cosmosDbServiceMock;

    public CoverServiceTests()
    {
        _loggerMock = new Mock<ILogger<ICoverService>>();
        _auditerMock = new Mock<IAuditer>();
        _cosmosDbServiceMock = new Mock<ICosmosDbService>();

        _coverService = new CoverService(_loggerMock.Object, _auditerMock.Object, _cosmosDbServiceMock.Object);
    }

    [Fact]
    public async Task CreateCoverAsync_ValidCover_OK()
    {
        // ARRANGE
        // set up the test data
        var testCover = new Cover
        {
            Premium = 950000,
            Type = CoverType.PassengerShip,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(360))
        };
        
        // ACT 
        var exception = await Record.ExceptionAsync(() => _coverService.CreateCoverAsync(testCover));

        // ASSERT
        Assert.Null(exception);
    }
    
    [Fact]
    public async Task CreateCoverAsync_StartDateInThePast_ThrowsArgumentException()
    {
        // ARRANGE
        // set up the test data
        var testCover = new Cover
        {
            Premium = 950000,
            Type = CoverType.Yacht,
            StartDate = DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(1)),
            EndDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(360))
        };
        
        // ACT 
        var exception = await Record.ExceptionAsync(() => _coverService.CreateCoverAsync(testCover));
        
        // ASSERT
        Assert.NotNull(exception);
        Assert.Equal("Cover StartDate is in the past", exception.Message);
    }
    
    [Fact]
    public async Task CreateCoverAsync_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        // ARRANGE
        // set up the test data
        var testCover = new Cover
        {
            Premium = 950000,
            Type = CoverType.Yacht,
            StartDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(61)),
            EndDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(60))
        };
        
        // ACT 
        var exception = await Record.ExceptionAsync(() => _coverService.CreateCoverAsync(testCover));
        
        // ASSERT
        Assert.NotNull(exception);
        Assert.Equal("EndDate is before Startdate", exception.Message);
    }
    
    [Fact]
    public async Task CreateCoverAsync_InsurancePeriodExceedsOneYear_ThrowsArgumentException()
    {
        // ARRANGE
        // set up the test data
        var testCover = new Cover
        {
            Premium = 950000,
            Type = CoverType.Yacht,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(460))
        };
        
        // ACT 
        var exception = await Record.ExceptionAsync(() => _coverService.CreateCoverAsync(testCover));
        
        // ASSERT
        Assert.NotNull(exception);
        Assert.Equal("The insurance period exceeds 1 year", exception.Message);
    }
    
    //
    // Unit tests for the CalculatePremium method
    //
    
    
    /// <summary>
    /// The rest of the methods in CoverService should be tested in Integration test
    /// </summary>
}
