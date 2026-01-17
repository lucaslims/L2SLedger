using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Domain.ValueObjects;
using Moq;
using System.Text.Json;

namespace L2SLedger.Application.Tests.UseCases.Periods;

public class GetFinancialPeriodByIdUseCaseTests
{
    private readonly Mock<IFinancialPeriodRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetFinancialPeriodByIdUseCase _useCase;

    public GetFinancialPeriodByIdUseCaseTests()
    {
        _mockRepository = new Mock<IFinancialPeriodRepository>();
        _mockMapper = new Mock<IMapper>();

        _useCase = new GetFinancialPeriodByIdUseCase(
            _mockRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_ReturnsPeriod()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var expectedDto = new FinancialPeriodDto(
            period.Id, 2026, 1, "2026/01", period.StartDate, period.EndDate,
            "Open", null, null, null, null, null, null, null, 0, 0, 0, null, period.CreatedAt);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockMapper
            .Setup(m => m.Map<FinancialPeriodDto>(period))
            .Returns(expectedDto);

        // Act
        var result = await _useCase.ExecuteAsync(periodId);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(2026);
        result.Month.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ThrowsException()
    {
        // Arrange
        var periodId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialPeriod?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _useCase.ExecuteAsync(periodId));

        exception.Code.Should().Be("FIN_PERIOD_NOT_FOUND");
    }

    [Fact]
    public async Task ExecuteAsync_DeletedPeriod_ThrowsException()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);
        period.MarkAsDeleted(); // Soft delete

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _useCase.ExecuteAsync(periodId));

        exception.Code.Should().Be("FIN_PERIOD_NOT_FOUND");
    }

    [Fact]
    public async Task ExecuteAsync_WithBalanceSnapshot_DeserializesCorrectly()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var categoryBalances = new List<CategoryBalance>
        {
            new CategoryBalance(Guid.NewGuid(), "Salário", 5000m, 0m, 5000m),
            new CategoryBalance(Guid.NewGuid(), "Alimentação", 0m, 800m, -800m)
        };

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            categoryBalances.AsReadOnly(),
            5000m,
            800m,
            4200m);

        var snapshotJson = JsonSerializer.Serialize(snapshot);

        // Close period with snapshot
        period.Close(Guid.NewGuid(), 5000m, 800m, snapshotJson);

        var expectedDto = new FinancialPeriodDto(
            period.Id, 2026, 1, "2026/01", period.StartDate, period.EndDate,
            "Closed", period.ClosedAt, period.ClosedByUserId, null,
            null, null, null, null, 5000m, 800m, 4200m, snapshot, period.CreatedAt);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockMapper
            .Setup(m => m.Map<FinancialPeriodDto>(period))
            .Returns(expectedDto);

        // Act
        var result = await _useCase.ExecuteAsync(periodId);

        // Assert
        result.Should().NotBeNull();
        result.BalanceSnapshot.Should().NotBeNull();
        result.BalanceSnapshot!.TotalIncome.Should().Be(5000m);
        result.BalanceSnapshot!.TotalExpense.Should().Be(800m);
        result.BalanceSnapshot!.Categories.Should().HaveCount(2);
    }
}
