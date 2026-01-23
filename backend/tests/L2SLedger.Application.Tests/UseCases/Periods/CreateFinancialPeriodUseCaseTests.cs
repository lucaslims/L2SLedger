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

namespace L2SLedger.Application.Tests.UseCases.Periods;

public class CreateFinancialPeriodUseCaseTests
{
    private readonly Mock<IFinancialPeriodRepository> _mockRepository;
    private readonly Mock<IValidator<CreatePeriodRequest>> _mockValidator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreateFinancialPeriodUseCase>> _mockLogger;
    private readonly CreateFinancialPeriodUseCase _useCase;

    public CreateFinancialPeriodUseCaseTests()
    {
        _mockRepository = new Mock<IFinancialPeriodRepository>();
        _mockValidator = new Mock<IValidator<CreatePeriodRequest>>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CreateFinancialPeriodUseCase>>();

        _useCase = new CreateFinancialPeriodUseCase(
            _mockRepository.Object,
            _mockValidator.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    private static FinancialPeriodDto CreateDto(FinancialPeriod p, string status = "Open") => new()
    {
        Id = p.Id,
        Year = p.Year,
        Month = p.Month,
        PeriodName = p.GetPeriodName(),
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = status,
        TotalIncome = 0,
        TotalExpense = 0,
        NetBalance = 0,
        CreatedAt = p.CreatedAt
    };

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreatePeriodRequest(2026, 1);
        var period = new FinancialPeriod(2026, 1);
        var expectedDto = CreateDto(period);

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.ExistsAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        _mockMapper
            .Setup(m => m.Map<FinancialPeriodDto>(period))
            .Returns(expectedDto);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(2026);
        result.Month.Should().Be(1);
        result.Status.Should().Be("Open");

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicatePeriod_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new CreatePeriodRequest(2026, 1);

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.ExistsAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _useCase.ExecuteAsync(request));

        exception.Code.Should().Be("FIN_PERIOD_ALREADY_EXISTS");
        exception.Message.Should().Contain("2026/01");

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidYear_ThrowsValidationException()
    {
        // Arrange
        var request = new CreatePeriodRequest(1999, 1);
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Year", "Ano deve estar entre 2000 e 2100")
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(request));

        _mockRepository.Verify(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidMonth_ThrowsValidationException()
    {
        // Arrange
        var request = new CreatePeriodRequest(2026, 13);
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Month", "Mês deve estar entre 1 e 12")
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(request));

        _mockRepository.Verify(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_LogsAuditInformation()
    {
        // Arrange
        var request = new CreatePeriodRequest(2026, 1);
        var period = new FinancialPeriod(2026, 1);

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.ExistsAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Financial period created")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CallsRepositoryAddAsync()
    {
        // Arrange
        var request = new CreatePeriodRequest(2026, 1);
        var period = new FinancialPeriod(2026, 1);

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.ExistsAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        _mockRepository.Verify(
            r => r.AddAsync(It.Is<FinancialPeriod>(p => p.Year == 2026 && p.Month == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationToken_CancelsOperation()
    {
        // Arrange
        var request = new CreatePeriodRequest(2026, 1);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _useCase.ExecuteAsync(request, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_CreatedPeriod_HasOpenStatus()
    {
        // Arrange
        var request = new CreatePeriodRequest(2026, 1);
        var period = new FinancialPeriod(2026, 1);

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.ExistsAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        _mockRepository.Verify(
            r => r.AddAsync(It.Is<FinancialPeriod>(p => p.Status == PeriodStatus.Open), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
