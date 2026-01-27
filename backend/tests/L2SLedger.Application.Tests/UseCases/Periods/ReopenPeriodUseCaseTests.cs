using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Tests.UseCases.Periods;

public class ReopenPeriodUseCaseTests
{
    private readonly Mock<IFinancialPeriodRepository> _mockRepository;
    private readonly Mock<IValidator<ReopenPeriodRequest>> _mockValidator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ReopenPeriodUseCase>> _mockLogger;
    private readonly ReopenPeriodUseCase _useCase;

    public ReopenPeriodUseCaseTests()
    {
        _mockRepository = new Mock<IFinancialPeriodRepository>();
        _mockValidator = new Mock<IValidator<ReopenPeriodRequest>>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ReopenPeriodUseCase>>();

        _useCase = new ReopenPeriodUseCase(
            _mockRepository.Object,
            _mockValidator.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ClosedPeriod_UpdatesStatus()
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

        var request = new ReopenPeriodRequest("Necessário ajustar lançamento incorreto");

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId, request);

        // Assert
        period.Status.Should().Be(PeriodStatus.Open);
        _mockRepository.Verify(r => r.UpdateAsync(period, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyOpenPeriod_ThrowsException()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var period = new FinancialPeriod(2026, 1); // Already open

        var request = new ReopenPeriodRequest("Tentando reabrir período já aberto");

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _useCase.ExecuteAsync(periodId, userId, request));

        exception.Code.Should().Be("FIN_PERIOD_ALREADY_OPENED");
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MissingJustification_ThrowsValidationException()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new ReopenPeriodRequest("");

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Reason", "Justificativa é obrigatória")
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(periodId, userId, request));

        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShortJustification_ThrowsValidationException()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new ReopenPeriodRequest("Curta");

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Reason", "Justificativa deve ter pelo menos 10 caracteres")
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(periodId, userId, request));
    }

    [Fact]
    public async Task ExecuteAsync_RecordsReopenedAtAndUserId()
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

        period.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        var request = new ReopenPeriodRequest("Necessário corrigir categorização");

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId, request);

        // Assert
        period.ReopenedAt.Should().NotBeNull();
        period.ReopenedByUserId.Should().Be(userId);
        period.ReopenReason.Should().Be(request.Reason);
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

        period.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        var request = new ReopenPeriodRequest("Operação crítica de reabertura");

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId, request);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Financial period REOPENED")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_AdminOnly_DocumentedInComment()
    {
        // This test documents that Admin authorization is validated in the controller layer
        // The use case itself doesn't validate roles (ADR-020: Clean Architecture)
        // ADR-016: RBAC - Only Admin can reopen periods (enforced by controller)
        
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

        period.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        var request = new ReopenPeriodRequest("Admin operation test");

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId, request);

        // Assert - Use case executes successfully
        // Authorization is handled at the controller level with [Authorize(Roles = "Admin")]
        period.Status.Should().Be(PeriodStatus.Open);
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

        period.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        var request = new ReopenPeriodRequest("Repository update test");

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(periodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(periodId, userId, request);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(period, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationToken_CancelsOperation()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new ReopenPeriodRequest("Cancellation test");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _useCase.ExecuteAsync(periodId, userId, request, cts.Token));
    }
}
