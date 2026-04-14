using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Periods;

public class ClosePeriodUseCaseTests
{
    private readonly Mock<IFinancialPeriodRepository> _mockRepository;
    private readonly Mock<IPeriodBalanceService> _mockBalanceService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ClosePeriodUseCase>> _mockLogger;
    private readonly ClosePeriodUseCase _useCase;

    public ClosePeriodUseCaseTests()
    {
        _mockRepository = new Mock<IFinancialPeriodRepository>();
        _mockBalanceService = new Mock<IPeriodBalanceService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ClosePeriodUseCase>>();

        _useCase = new ClosePeriodUseCase(
            _mockRepository.Object,
            _mockBalanceService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidPeriod_UpdatesStatus()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockBalanceService
            .Setup(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId);

        // Assert
        period.Status.Should().Be(PeriodStatus.Closed);
        _mockRepository.Verify(r => r.UpdateAsync(period, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyClosedPeriod_ThrowsException()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);

        // Close the period first
        period.Close(userId, 1000m, 500m, JsonSerializer.Serialize(snapshot));

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _useCase.ExecuteAsync(periodId, userId));

        exception.Code.Should().Be("FIN_PERIOD_ALREADY_CLOSED");
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CalculatesSnapshotCorrectly()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
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

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockBalanceService
            .Setup(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId);

        // Assert
        _mockBalanceService.Verify(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SavesTotalIncomeAndExpense()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1500m,
            750m,
            750m);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockBalanceService
            .Setup(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId);

        // Assert
        period.TotalIncome.Should().Be(1500m);
        period.TotalExpense.Should().Be(750m);
    }

    [Fact]
    public async Task ExecuteAsync_CalculatesNetBalance()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            3000m,
            1200m,
            1800m);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockBalanceService
            .Setup(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId);

        // Assert
        period.NetBalance.Should().Be(1800m);
    }

    [Fact]
    public async Task ExecuteAsync_SerializesBalanceSnapshot()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockBalanceService
            .Setup(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId);

        // Assert
        period.BalanceSnapshotJson.Should().NotBeNullOrEmpty();
        period.BalanceSnapshotJson.Should().Contain("TotalIncome");
    }

    [Fact]
    public async Task ExecuteAsync_RecordsClosedAtAndUserId()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockBalanceService
            .Setup(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId);

        // Assert
        period.ClosedAt.Should().NotBeNull();
        period.ClosedByUserId.Should().Be(userId);
    }

    [Fact]
    public async Task ExecuteAsync_LogsCriticalAudit()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockBalanceService
            .Setup(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Período financeiro FECHADO")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CallsRepositoryUpdateAsync()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockBalanceService
            .Setup(s => s.CalculateBalanceSnapshotAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(period, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationToken_CancelsOperation()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _useCase.ExecuteAsync(periodId, userId, cts.Token));
    }
}
