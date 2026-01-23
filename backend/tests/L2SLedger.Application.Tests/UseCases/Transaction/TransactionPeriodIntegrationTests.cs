using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using L2SLedger.Application.DTOs.Transaction;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Application.UseCases.Transaction;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace L2SLedger.Application.Tests.UseCases.Transaction;

/// <summary>
/// Testes de integração entre Use Cases de Transaction e validação de Períodos Financeiros (ADR-015).
/// </summary>
public class TransactionPeriodIntegrationTests
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IFinancialPeriodRepository> _mockPeriodRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IValidator<CreateTransactionRequest>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateTransactionRequest>> _mockUpdateValidator;
    private readonly Mock<ILogger<EnsurePeriodExistsAndOpenUseCase>> _mockLogger;
    private readonly Guid _testUserId;
    private readonly Guid _testCategoryId;

    public TransactionPeriodIntegrationTests()
    {
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockPeriodRepository = new Mock<IFinancialPeriodRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCreateValidator = new Mock<IValidator<CreateTransactionRequest>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateTransactionRequest>>();
        _mockLogger = new Mock<ILogger<EnsurePeriodExistsAndOpenUseCase>>();

        _testUserId = Guid.NewGuid();
        _testCategoryId = Guid.NewGuid();

        // Setup defaults
        _mockCurrentUserService.Setup(s => s.GetUserId()).Returns(_testUserId);

        var testCategory = new Category("Test Category", "Test Description", null);
        _mockCategoryRepository
            .Setup(r => r.GetByIdAsync(_testCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testCategory);

        _mockCreateValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateTransactionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockUpdateValidator
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateTransactionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task CreateTransaction_WithOpenPeriod_ShouldSucceed()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 1, 15);
        var createdPeriod = new FinancialPeriod(2026, 1);

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialPeriod?)null); // Período não existe, será auto-criado

        _mockPeriodRepository
            .Setup(r => r.AddAsync(It.IsAny<FinancialPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPeriod);

        var ensurePeriodUseCase = new EnsurePeriodExistsAndOpenUseCase(
            _mockPeriodRepository.Object,
            _mockLogger.Object);

        var createUseCase = new CreateTransactionUseCase(
            _mockTransactionRepository.Object,
            _mockCategoryRepository.Object,
            _mockCreateValidator.Object,
            _mockCurrentUserService.Object,
            ensurePeriodUseCase);

        var request = new CreateTransactionRequest
        {
            Description = "Test Income",
            Amount = 100m,
            Type = (int)TransactionType.Income,
            TransactionDate = transactionDate,
            CategoryId = _testCategoryId,
            Notes = null,
            IsRecurring = false,
            RecurringDay = null
        };

        // Act
        var result = await createUseCase.ExecuteAsync(request, default);

        // Assert
        result.Should().NotBeEmpty();
        _mockTransactionRepository.Verify(
            r => r.AddAsync(It.IsAny<Domain.Entities.Transaction>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockPeriodRepository.Verify(
            r => r.AddAsync(It.Is<FinancialPeriod>(p => p.Year == 2026 && p.Month == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateTransaction_WithClosedPeriod_ShouldThrowException()
    {
        // Arrange
        var transactionDate = new DateTime(2026, 1, 15);
        var closedPeriod = new FinancialPeriod(2026, 1);

        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);

        closedPeriod.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedPeriod);

        var ensurePeriodUseCase = new EnsurePeriodExistsAndOpenUseCase(
            _mockPeriodRepository.Object,
            _mockLogger.Object);

        var createUseCase = new CreateTransactionUseCase(
            _mockTransactionRepository.Object,
            _mockCategoryRepository.Object,
            _mockCreateValidator.Object,
            _mockCurrentUserService.Object,
            ensurePeriodUseCase);

        var request = new CreateTransactionRequest
        {
            Description = "Test Income",
            Amount = 100m,
            Type = (int)TransactionType.Income,
            TransactionDate = transactionDate,
            CategoryId = _testCategoryId,
            Notes = null,
            IsRecurring = false,
            RecurringDay = null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => createUseCase.ExecuteAsync(request, default));

        exception.Code.Should().Be("FIN_PERIOD_CLOSED");
        exception.Message.Should().Contain("2026/01");
        exception.Message.Should().Contain("está fechado");

        _mockTransactionRepository.Verify(
            r => r.AddAsync(It.IsAny<Domain.Entities.Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateTransaction_WithClosedPeriod_ShouldThrowException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transactionDate = new DateTime(2026, 1, 15);

        var existingTransaction = new Domain.Entities.Transaction(
            description: "Original",
            amount: 100m,
            type: TransactionType.Expense,
            transactionDate: transactionDate,
            categoryId: _testCategoryId,
            userId: _testUserId,
            notes: null,
            isRecurring: false,
            recurringDay: null);

        var closedPeriod = new FinancialPeriod(2026, 1);
        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);
        closedPeriod.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        _mockTransactionRepository
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedPeriod);

        var ensurePeriodUseCase = new EnsurePeriodExistsAndOpenUseCase(
            _mockPeriodRepository.Object,
            _mockLogger.Object);

        var updateUseCase = new UpdateTransactionUseCase(
            _mockTransactionRepository.Object,
            _mockCategoryRepository.Object,
            _mockUpdateValidator.Object,
            _mockCurrentUserService.Object,
            ensurePeriodUseCase);

        var request = new UpdateTransactionRequest
        {
            Description = "Updated",
            Amount = 200m,
            Type = (int)TransactionType.Expense,
            TransactionDate = transactionDate, // Same date
            CategoryId = _testCategoryId,
            Notes = null,
            IsRecurring = false,
            RecurringDay = null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => updateUseCase.ExecuteAsync(transactionId, request, default));

        exception.Code.Should().Be("FIN_PERIOD_CLOSED");
        _mockTransactionRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateTransaction_ChangingDateToClosedPeriod_ShouldThrowException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalDate = new DateTime(2026, 2, 15);
        var newDate = new DateTime(2026, 1, 15);

        var existingTransaction = new Domain.Entities.Transaction(
            description: "Original",
            amount: 100m,
            type: TransactionType.Expense,
            transactionDate: originalDate,
            categoryId: _testCategoryId,
            userId: _testUserId,
            notes: null,
            isRecurring: false,
            recurringDay: null);

        var openPeriodFeb = new FinancialPeriod(2026, 2); // Open
        var closedPeriodJan = new FinancialPeriod(2026, 1);
        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);
        closedPeriodJan.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        _mockTransactionRepository
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(originalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openPeriodFeb);

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(newDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedPeriodJan);

        var ensurePeriodUseCase = new EnsurePeriodExistsAndOpenUseCase(
            _mockPeriodRepository.Object,
            _mockLogger.Object);

        var updateUseCase = new UpdateTransactionUseCase(
            _mockTransactionRepository.Object,
            _mockCategoryRepository.Object,
            _mockUpdateValidator.Object,
            _mockCurrentUserService.Object,
            ensurePeriodUseCase);

        var request = new UpdateTransactionRequest
        {
            Description = "Updated",
            Amount = 200m,
            Type = (int)TransactionType.Expense,
            TransactionDate = newDate, // Changing to closed period
            CategoryId = _testCategoryId,
            Notes = null,
            IsRecurring = false,
            RecurringDay = null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => updateUseCase.ExecuteAsync(transactionId, request, default));

        exception.Code.Should().Be("FIN_PERIOD_CLOSED");
        exception.Message.Should().Contain("2026/01");
    }

    [Fact]
    public async Task UpdateTransaction_ChangingDateBetweenOpenPeriods_ShouldSucceed()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalDate = new DateTime(2026, 2, 15);
        var newDate = new DateTime(2026, 3, 15);

        var existingTransaction = new Domain.Entities.Transaction(
            description: "Original",
            amount: 100m,
            type: TransactionType.Expense,
            transactionDate: originalDate,
            categoryId: _testCategoryId,
            userId: _testUserId,
            notes: null,
            isRecurring: false,
            recurringDay: null);

        var openPeriodFeb = new FinancialPeriod(2026, 2); // Open
        var openPeriodMar = new FinancialPeriod(2026, 3); // Open

        _mockTransactionRepository
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(originalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openPeriodFeb);

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(newDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openPeriodMar);

        var ensurePeriodUseCase = new EnsurePeriodExistsAndOpenUseCase(
            _mockPeriodRepository.Object,
            _mockLogger.Object);

        var updateUseCase = new UpdateTransactionUseCase(
            _mockTransactionRepository.Object,
            _mockCategoryRepository.Object,
            _mockUpdateValidator.Object,
            _mockCurrentUserService.Object,
            ensurePeriodUseCase);

        var request = new UpdateTransactionRequest
        {
            Description = "Updated",
            Amount = 200m,
            Type = (int)TransactionType.Expense,
            TransactionDate = newDate, // Changing to another open period
            CategoryId = _testCategoryId,
            Notes = null,
            IsRecurring = false,
            RecurringDay = null
        };

        // Act
        await updateUseCase.ExecuteAsync(transactionId, request, default);

        // Assert
        _mockTransactionRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.Transaction>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTransaction_WithClosedPeriod_ShouldThrowException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transactionDate = new DateTime(2026, 1, 15);

        var existingTransaction = new Domain.Entities.Transaction(
            description: "To Delete",
            amount: 100m,
            type: TransactionType.Expense,
            transactionDate: transactionDate,
            categoryId: _testCategoryId,
            userId: _testUserId,
            notes: null,
            isRecurring: false,
            recurringDay: null);

        var closedPeriod = new FinancialPeriod(2026, 1);
        var snapshot = new BalanceSnapshot(
            DateTime.UtcNow,
            new List<CategoryBalance>().AsReadOnly(),
            1000m,
            500m,
            500m);
        closedPeriod.Close(Guid.NewGuid(), 1000m, 500m, JsonSerializer.Serialize(snapshot));

        _mockTransactionRepository
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedPeriod);

        var ensurePeriodUseCase = new EnsurePeriodExistsAndOpenUseCase(
            _mockPeriodRepository.Object,
            _mockLogger.Object);

        var deleteUseCase = new DeleteTransactionUseCase(
            _mockTransactionRepository.Object,
            _mockCurrentUserService.Object,
            ensurePeriodUseCase);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => deleteUseCase.ExecuteAsync(transactionId, default));

        exception.Code.Should().Be("FIN_PERIOD_CLOSED");
        exception.Message.Should().Contain("2026/01");
        exception.Message.Should().Contain("está fechado");
    }

    [Fact]
    public async Task DeleteTransaction_WithOpenPeriod_ShouldSucceed()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transactionDate = new DateTime(2026, 1, 15);

        var existingTransaction = new Domain.Entities.Transaction(
            description: "To Delete",
            amount: 100m,
            type: TransactionType.Expense,
            transactionDate: transactionDate,
            categoryId: _testCategoryId,
            userId: _testUserId,
            notes: null,
            isRecurring: false,
            recurringDay: null);

        var openPeriod = new FinancialPeriod(2026, 1); // Open

        _mockTransactionRepository
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        _mockPeriodRepository
            .Setup(r => r.GetPeriodForDateAsync(transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openPeriod);

        var ensurePeriodUseCase = new EnsurePeriodExistsAndOpenUseCase(
            _mockPeriodRepository.Object,
            _mockLogger.Object);

        var deleteUseCase = new DeleteTransactionUseCase(
            _mockTransactionRepository.Object,
            _mockCurrentUserService.Object,
            ensurePeriodUseCase);

        // Act
        await deleteUseCase.ExecuteAsync(transactionId, default);

        // Assert
        _mockTransactionRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.Transaction>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
