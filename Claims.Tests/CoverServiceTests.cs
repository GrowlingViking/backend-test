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
    
    // Unit tests for the ComputePremium method
    [Theory]
    [InlineData(CoverType.PassengerShip)]
    [InlineData(CoverType.Yacht)]
    [InlineData(CoverType.Tanker)]
    [InlineData(CoverType.ContainerShip)]
    [InlineData(CoverType.BulkCarrier)]
    public void ComputePremium_30DaysPeriod_CorrectPremium(CoverType coverType)
    {
        // ARRANGE
        const int nrOfDays = 30;
        var multiplier = coverType switch
        {
            CoverType.Yacht => 1.1m,
            CoverType.PassengerShip => 1.2m,
            CoverType.Tanker => 1.5m,
            _ => 1.3m
        };
        
        var expectedPremium = 1250 * multiplier * nrOfDays;
        
        // ACT
        var result = _coverService.ComputePremium(DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(nrOfDays)), coverType);
        
        // ASSERT
        Assert.Equal(expectedPremium, result);
    }
    
    [Theory]
    [InlineData(CoverType.PassengerShip)]
    [InlineData(CoverType.Yacht)]
    [InlineData(CoverType.Tanker)]
    [InlineData(CoverType.ContainerShip)]
    [InlineData(CoverType.BulkCarrier)]
    public void ComputePremium_180DaysPeriod_CorrectPremium(CoverType coverType)
    {
        // ARRANGE
        const int nrOfDays = 180;
        var multiplier = coverType switch
        {
            CoverType.Yacht => 1.1m,
            CoverType.PassengerShip => 1.2m,
            CoverType.Tanker => 1.5m,
            _ => 1.3m
        };
        var premiumPerDay = 1250 * multiplier;

        var expectedPremium = 0m;
        if (coverType == CoverType.Yacht)
        {
            expectedPremium += 30 * premiumPerDay + (premiumPerDay - premiumPerDay * 0.05m) * (nrOfDays - 30);
        }
        else
        {
            expectedPremium += 30 * premiumPerDay + (premiumPerDay - premiumPerDay * 0.02m) * (nrOfDays - 30);
        }
        
        // ACT
        var result = _coverService.ComputePremium(DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(nrOfDays)), coverType);
        
        // ASSERT
        Assert.Equal(expectedPremium, result);
    }
    
    [Theory]
    [InlineData(CoverType.PassengerShip)]
    [InlineData(CoverType.Yacht)]
    [InlineData(CoverType.Tanker)]
    [InlineData(CoverType.ContainerShip)]
    [InlineData(CoverType.BulkCarrier)]
    public void ComputePremium_365DaysPeriod_CorrectPremium(CoverType coverType)
    {
        // ARRANGE
        const int nrOfDays = 365;
        var multiplier = coverType switch
        {
            CoverType.Yacht => 1.1m,
            CoverType.PassengerShip => 1.2m,
            CoverType.Tanker => 1.5m,
            _ => 1.3m
        };
        var premiumPerDay = 1250 * multiplier;

        var expectedPremium = 0m;
        if (coverType == CoverType.Yacht)
        {
            expectedPremium += 30 * premiumPerDay + (premiumPerDay - premiumPerDay * 0.05m) * 150 + (premiumPerDay - premiumPerDay * 0.08m) * (nrOfDays - 180);
        }
        else
        {
            expectedPremium += 30 * premiumPerDay + (premiumPerDay - premiumPerDay * 0.02m) * 150 + (premiumPerDay - premiumPerDay * 0.03m) * (nrOfDays - 180);
        }
        
        // ACT
        var result = _coverService.ComputePremium(DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(nrOfDays)), coverType);
        
        // ASSERT
        Assert.Equal(expectedPremium, result);
    }

    [Fact]
    public void ComputePremium_LongerThan365Days_DoesntComputePast365()
    {
        // ARRANGE
        const int nrOfDays = 365;
        const CoverType coverType = CoverType.Tanker;
        const decimal premiumPerDay = 1250 * 1.5m;
        
        const decimal expectedPremium = 30 * premiumPerDay + (premiumPerDay - premiumPerDay * 0.02m) * 150 + (premiumPerDay - premiumPerDay * 0.03m) * (nrOfDays - 180);
        
        // ACT
        var result = _coverService.ComputePremium(DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(nrOfDays + 50)), coverType);
        
        // ASSERT
        Assert.Equal(expectedPremium, result);
    }
    
    /// <summary>
    /// The rest of the methods in CoverService should be tested in Integration tests
    /// </summary>
}
