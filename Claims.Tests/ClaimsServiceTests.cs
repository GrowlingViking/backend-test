using Claims.Auditing;
using Claims.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Claims.Tests;

public class ClaimsServiceTests
{
    private readonly IClaimService _claimService;
    
    private Mock<ILogger<IClaimService>> _loggerMock;
    private Mock<ICosmosDbService> _cosmosDbServiceMock;
    private Mock<IAuditer> _auditerMock;
    private Mock<ICoverService> _coverServiceMock;
    
    public ClaimsServiceTests()
    {
        _loggerMock = new Mock<ILogger<IClaimService>>();
        _cosmosDbServiceMock = new Mock<ICosmosDbService>();
        _auditerMock = new Mock<IAuditer>();
        _coverServiceMock = new Mock<ICoverService>();

        _claimService = new ClaimService(_loggerMock.Object, _cosmosDbServiceMock.Object, _auditerMock.Object, _coverServiceMock.Object);
    }

    [Fact]
    public async Task CreateClaimAsync_ValidClaims_OK()
    {
        // ARRANGE
        // set up test data
        var testClaim = new Claim
        {
            Created = DateTime.Now,
            Name = "Test claim",
            Type = ClaimType.Collision,
            DamageCost = 25000,
            CoverId = "test"
        };

        var testCover = new Cover
        {
            Id = "test",
            Type = CoverType.Yacht,
            Premium = 90000,
            StartDate = new DateOnly(2023, 04, 12),
            EndDate = new DateOnly(2025, 01, 12)
        };
        
        // set up returns from mocked services
        _coverServiceMock.Setup(x => x.GetCoverAsync(testCover.Id)).ReturnsAsync(testCover);
        
        // ACT
        var exception = await Record.ExceptionAsync(() => _claimService.CreateClaimAsync(testClaim));
        
        // ASSERT
        Assert.Null(exception);
    }

    [Fact]
    public async Task CreateClaimAsync_DamageCostTooHigh_ThrowArgumentException()
    {
        // ARRANGE
        // set up the testdata
        var testClaim = new Claim
        {
            Created = DateTime.Now,
            Name = "Test claim",
            Type = ClaimType.Grounding,
            DamageCost = 250000,
            CoverId = "test"
        };
        
        var testCover = new Cover
        {
            Id = "test",
            Type = CoverType.Yacht,
            Premium = 90000,
            StartDate = new DateOnly(2023, 04, 12),
            EndDate = new DateOnly(2025, 01, 12)
        };
        
        // set up returns from mocked services
        _coverServiceMock.Setup(x => x.GetCoverAsync(testCover.Id)).ReturnsAsync(testCover);
        
        // ACT
        // ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => _claimService.CreateClaimAsync(testClaim));
    }
    
    [Theory, MemberData(nameof(_createClaimsOutsideCoverDate))]
    public async Task CreateClaimAsync_ClaimCreatedOutsideCoverDate_ThrowArgumentException(DateTime created)
    {
        // ARRANGE
        // set up the testdata
        var testClaim = new Claim
        {
            Created = created,
            Name = "Test claim",
            Type = ClaimType.Grounding,
            DamageCost = 250000,
            CoverId = "test"
        };
        
        var testCover = new Cover
        {
            Id = "test",
            Type = CoverType.Yacht,
            Premium = 90000,
            StartDate = new DateOnly(2023, 04, 12),
            EndDate = new DateOnly(2025, 01, 12)
        };
        
        // set up returns from mocked services
        _coverServiceMock.Setup(x => x.GetCoverAsync(testCover.Id)).ReturnsAsync(testCover);
        
        // ACT
        // ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => _claimService.CreateClaimAsync(testClaim));
    }
    
    
    /// <summary>
    /// The rest of the methods in ClaimsService should be tested in Integration test
    /// </summary>
    
    
    // Test data for 
    public static TheoryData<DateTime> _createClaimsOutsideCoverDate =
        new()
        {
            new DateTime(2023, 2, 25),
            new DateTime(2025, 8, 5)
        };
}
