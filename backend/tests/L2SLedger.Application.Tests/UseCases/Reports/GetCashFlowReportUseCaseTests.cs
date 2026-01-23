using FluentAssertions;
using FluentValidation;
using L2SLedger.Application.DTOs.Reports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Reports;
using L2SLedger.Domain.Entities;
using Moq;
using DomainTransaction = L2SLedger.Domain.Entities.Transaction;

namespace L2SLedger.Application.Tests.UseCases.Reports;

/// <summary>
/// Testes para GetCashFlowReportUseCase.
/// Validar geração de relatório de fluxo de caixa com movimentações.
/// </summary>
public class GetCashFlowReportUseCaseTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetCashFlowReportUseCase _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public GetCashFlowReportUseCaseTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(_userId);

        _sut = new GetCashFlowReportUseCase(
            _transactionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetCashFlowReport_WithValidPeriod_ReturnsCompleteReport()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 10);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1000m);

        var transactions = new List<DomainTransaction>
        {
            CreateTransaction("Salário", 3000m, TransactionType.Income, new DateTime(2026, 1, 5), "Receitas"),
            CreateTransaction("Aluguel", 800m, TransactionType.Expense, new DateTime(2026, 1, 7), "Moradia"),
            CreateTransaction("Freelance", 500m, TransactionType.Income, new DateTime(2026, 1, 9), "Receitas")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetTransactionsWithCategoryAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(endDate);
        result.OpeningBalance.Should().Be(1000m);
        result.ClosingBalance.Should().Be(3700m); // 1000 + 3000 + 500 - 800
        result.NetChange.Should().Be(2700m); // 3700 - 1000
        result.Movements.Should().HaveCount(3);

        result.Movements[0].Description.Should().Be("Salário");
        result.Movements[0].Amount.Should().Be(3000m); // Positivo para Income
        result.Movements[0].Type.Should().Be("Income");

        result.Movements[1].Description.Should().Be("Aluguel");
        result.Movements[1].Amount.Should().Be(-800m); // Negativo para Expense
        result.Movements[1].Type.Should().Be("Expense");

        result.Movements[2].Description.Should().Be("Freelance");
        result.Movements[2].Amount.Should().Be(500m);
    }

    [Fact]
    public async Task GetCashFlowReport_OrdersMovementsByDate()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 10);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var transactions = new List<DomainTransaction>
        {
            CreateTransaction("T3", 300m, TransactionType.Income, new DateTime(2026, 1, 9), "Cat"),
            CreateTransaction("T1", 100m, TransactionType.Income, new DateTime(2026, 1, 1), "Cat"),
            CreateTransaction("T2", 200m, TransactionType.Expense, new DateTime(2026, 1, 5), "Cat")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetTransactionsWithCategoryAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Movements.Should().HaveCount(3);
        result.Movements.Should().BeInAscendingOrder(m => m.Date);
        result.Movements[0].Description.Should().Be("T1");
        result.Movements[1].Description.Should().Be("T2");
        result.Movements[2].Description.Should().Be("T3");
    }

    [Fact]
    public async Task GetCashFlowReport_CalculatesOpeningBalance()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 15);
        var endDate = new DateTime(2026, 1, 20);

        // Saldo antes do período: 5000
        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5000m);

        _transactionRepositoryMock
            .Setup(x => x.GetTransactionsWithCategoryAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainTransaction>());

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.OpeningBalance.Should().Be(5000m);

        _transactionRepositoryMock.Verify(
            x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCashFlowReport_CalculatesNetChange()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 10);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2000m);

        var transactions = new List<DomainTransaction>
        {
            CreateTransaction("Receita", 1500m, TransactionType.Income, new DateTime(2026, 1, 5), "Cat"),
            CreateTransaction("Despesa", 500m, TransactionType.Expense, new DateTime(2026, 1, 7), "Cat")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetTransactionsWithCategoryAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.OpeningBalance.Should().Be(2000m);
        result.ClosingBalance.Should().Be(3000m); // 2000 + 1500 - 500
        result.NetChange.Should().Be(1000m); // 3000 - 2000
    }

    [Fact]
    public async Task GetCashFlowReport_FormatsAmountsByType()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 10);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var transactions = new List<DomainTransaction>
        {
            CreateTransaction("Salário", 5000m, TransactionType.Income, new DateTime(2026, 1, 5), "Receitas"),
            CreateTransaction("Conta", 200m, TransactionType.Expense, new DateTime(2026, 1, 7), "Contas")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetTransactionsWithCategoryAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Movements.Should().HaveCount(2);

        // Income deve ser positivo
        var incomeMovement = result.Movements.First(m => m.Type == "Income");
        incomeMovement.Amount.Should().Be(5000m);
        incomeMovement.Amount.Should().BePositive();

        // Expense deve ser negativo
        var expenseMovement = result.Movements.First(m => m.Type == "Expense");
        expenseMovement.Amount.Should().Be(-200m);
        expenseMovement.Amount.Should().BeNegative();
    }

    [Fact]
    public async Task GetCashFlowReport_WithPeriodTooLong_ThrowsValidationException()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 4, 2); // Mais de 90 dias

        // Act
        Func<Task> act = async () => await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*período máximo permitido é de 90 dias*");
    }

    [Fact]
    public async Task GetCashFlowReport_IncludesCategoryNames()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 10);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var transactions = new List<DomainTransaction>
        {
            CreateTransaction("Transação 1", 100m, TransactionType.Income, new DateTime(2026, 1, 5), "Salário"),
            CreateTransaction("Transação 2", 50m, TransactionType.Expense, new DateTime(2026, 1, 7), "Alimentação")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetTransactionsWithCategoryAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Movements.Should().HaveCount(2);
        result.Movements[0].Category.Should().Be("Salário");
        result.Movements[1].Category.Should().Be("Alimentação");
    }

    private DomainTransaction CreateTransaction(string description, decimal amount, TransactionType type, DateTime date, string categoryName)
    {
        var category = new Category(categoryName, null);
        typeof(Category).GetProperty("Id")!.SetValue(category, _categoryId);

        var transaction = new DomainTransaction(
            description,
            amount,
            type,
            date,
            _categoryId,
            _userId);

        // Setar a navigation property Category manualmente
        typeof(DomainTransaction).GetProperty("Category")!.SetValue(transaction, category);

        return transaction;
    }
}
