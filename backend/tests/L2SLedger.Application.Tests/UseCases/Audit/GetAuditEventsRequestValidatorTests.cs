using FluentAssertions;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Validators.Audit;

namespace L2SLedger.Application.Tests.UseCases.Audit;

/// <summary>
/// Testes para GetAuditEventsRequestValidator.
/// </summary>
public class GetAuditEventsRequestValidatorTests
{
    private readonly GetAuditEventsRequestValidator _validator;

    public GetAuditEventsRequestValidatorTests()
    {
        _validator = new GetAuditEventsRequestValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = 20
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WithInvalidPage_ShouldFail(int page)
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = page,
            PageSize = 20
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(-1)]
    public async Task Validate_WithInvalidPageSize_ShouldFail(int pageSize)
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = pageSize
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public async Task Validate_WithStartDateAfterEndDate_ShouldFail()
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = 20,
            StartDate = new DateTime(2026, 1, 31),
            EndDate = new DateTime(2026, 1, 1)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Data inicial"));
    }

    [Theory]
    [InlineData("Success")]
    [InlineData("Failed")]
    [InlineData("Denied")]
    public async Task Validate_WithValidResult_ShouldPass(string resultValue)
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = 20,
            Result = resultValue
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidResult_ShouldFail()
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = 20,
            Result = "InvalidResult"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Result");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task Validate_WithValidEventType_ShouldPass(int eventType)
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = 20,
            EventType = eventType
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    [InlineData(-1)]
    public async Task Validate_WithInvalidEventType_ShouldFail(int eventType)
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = 20,
            EventType = eventType
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EventType");
    }

    [Fact]
    public async Task Validate_WithEntityTypeTooLong_ShouldFail()
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = 20,
            EntityType = new string('A', 101) // Max is 100
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EntityType");
    }
}
