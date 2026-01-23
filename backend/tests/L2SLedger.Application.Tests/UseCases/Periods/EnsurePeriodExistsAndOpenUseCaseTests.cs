using FluentAssertions;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Tests.UseCases.Periods;

public class EnsurePeriodExistsAndOpenUseCaseTests
{
    private readonly Mock<IFinancialPeriodRepository> _mockRepository;
    private readonly Mock<ILogger<EnsurePeriodExistsAndOpenUseCase>> _mockLogger;
    private readonly EnsurePeriodExistsAndOpenUseCase _useCase;

    public EnsurePeriodExistsAndOpenUseCaseTests()
    {
        _mockRepository = new Mock<IFinancialPeriodRepository>();
        _mockLogger = new Mock<ILogger<EnsurePeriodExistsAndOpenUseCase>>();

        _useCase = new EnsurePeriodExistsAndOpenUseCase(
            _mockRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_PeriodDoesNotExist_CreatesAutomatically()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 3, 15);
        var createdPeriod = new FinancialPeriod(2026, 3);

        _mockRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialPeriod?)null);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPeriod);

        // Act
        await _useCase.ExecuteAsync(transactionDate);

        // Assert
        _mockRepository.Verify(
            r => r.AddAsync(It.Is<FinancialPeriod>(p => p.Year == 2026 && p.Month == 3), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PeriodExistsAndOpen_PassesSuccessfully()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 3, 15);
        var existingPeriod = new FinancialPeriod(2026, 3); // Open by default

        _mockRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPeriod);

        // Act
        await _useCase.ExecuteAsync(transactionDate);

        // Assert - No exception should be thrown
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_PeriodExistsButClosed_ThrowsException()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 3, 15);
        var closedPeriod = new FinancialPeriod(2026, 3);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);

        // Close the period
        closedPeriod.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        _mockRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedPeriod);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _useCase.ExecuteAsync(transactionDate));

        exception.Code.Should().Be("FIN_PERIOD_CLOSED");
        exception.Message.Should().Contain("2026/03");
        exception.Message.Should().Contain("está fechado");
    }

    [Fact]
    public async Task ExecuteAsync_AutoCreation_LogsInformation()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 3, 15);
        var createdPeriod = new FinancialPeriod(2026, 3);

        _mockRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialPeriod?)null);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPeriod);

        // Act
        await _useCase.ExecuteAsync(transactionDate);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Auto-created financial period")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UsedForCreateOperation_ValidatesPeriod()
    {
        // Arrange - Creating a new transaction requires open period
        var transactionDate = new DateTime(2026, 3, 15);
        var openPeriod = new FinancialPeriod(2026, 3);

        _mockRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openPeriod);

        // Act & Assert - Should pass without exceptions
        await _useCase.ExecuteAsync(transactionDate);

        openPeriod.Status.Should().Be(PeriodStatus.Open);
    }

    [Fact]
    public async Task ExecuteAsync_UsedForUpdateOperation_ValidatesPeriod()
    {
        // Arrange - Updating a transaction requires open period
        var transactionDate = new DateTime(2026, 3, 15);
        var openPeriod = new FinancialPeriod(2026, 3);

        _mockRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openPeriod);

        // Act & Assert - Should pass without exceptions
        await _useCase.ExecuteAsync(transactionDate);

        openPeriod.Status.Should().Be(PeriodStatus.Open);
    }

    [Fact]
    public async Task ExecuteAsync_UsedForDeleteOperation_ValidatesPeriod()
    {
        // Arrange - Deleting a transaction requires open period
        var transactionDate = new DateTime(2026, 3, 15);
        var openPeriod = new FinancialPeriod(2026, 3);

        _mockRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openPeriod);

        // Act & Assert - Should pass without exceptions
        await _useCase.ExecuteAsync(transactionDate);

        openPeriod.Status.Should().Be(PeriodStatus.Open);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationToken_CancelsOperation()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 3, 15);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _useCase.ExecuteAsync(transactionDate, cts.Token));
    }
}
