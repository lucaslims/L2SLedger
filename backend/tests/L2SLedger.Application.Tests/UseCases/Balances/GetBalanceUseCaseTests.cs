using FluentAssertions;
using FluentValidation;
using L2SLedger.Application.DTOs.Balances;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Balances;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Balances;

/// <summary>
/// Testes para GetBalanceUseCase.
/// Validar cálculos de saldos consolidados por período e categoria.
/// </summary>
public class GetBalanceUseCaseTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetBalanceUseCase _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _category1Id = Guid.NewGuid();
    private readonly Guid _category2Id = Guid.NewGuid();

    public GetBalanceUseCaseTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(_userId);

        _sut = new GetBalanceUseCase(
            _transactionRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetBalance_WithValidPeriod_ReturnsCorrectSummary()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

        // Simulando: Cat1 - 3 receitas (1000) / 1 despesa (300) = 700
        //            Cat2 - 2 despesas (500) = -500
        var balanceData = new Dictionary<(Guid CategoryId, TransactionType Type), decimal>
        {
            { (_category1Id, TransactionType.Income), 1000m },
            { (_category1Id, TransactionType.Expense), 300m },
            { (_category2Id, TransactionType.Expense), 500m }
        };

        var categories = new List<Category>
        {
            CreateCategory(_category1Id, "Salário"),
            CreateCategory(_category2Id, "Alimentação")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(_userId, startDate, endDate, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balanceData);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate, null);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(1000m);
        result.TotalExpense.Should().Be(800m);
        result.NetBalance.Should().Be(200m);
        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(endDate);
        result.ByCategory.Should().HaveCount(2);

        var cat1 = result.ByCategory.First(c => c.CategoryId == _category1Id);
        cat1.Income.Should().Be(1000m);
        cat1.Expense.Should().Be(300m);
        cat1.NetBalance.Should().Be(700m);
        cat1.CategoryName.Should().Be("Salário");

        var cat2 = result.ByCategory.First(c => c.CategoryId == _category2Id);
        cat2.Income.Should().Be(0m);
        cat2.Expense.Should().Be(500m);
        cat2.NetBalance.Should().Be(-500m);
        cat2.CategoryName.Should().Be("Alimentação");
    }

    [Fact]
    public async Task GetBalance_WithCategoryFilter_ReturnsOnlyThatCategory()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

        var balanceData = new Dictionary<(Guid CategoryId, TransactionType Type), decimal>
        {
            { (_category1Id, TransactionType.Income), 1500m }
        };

        var categories = new List<Category>
        {
            CreateCategory(_category1Id, "Salário")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(_userId, startDate, endDate, _category1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balanceData);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate, _category1Id);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(1500m);
        result.TotalExpense.Should().Be(0m);
        result.NetBalance.Should().Be(1500m);
        result.ByCategory.Should().HaveCount(1);
        result.ByCategory[0].CategoryId.Should().Be(_category1Id);

        _transactionRepositoryMock.Verify(
            x => x.GetBalanceByCategoryAsync(_userId, startDate, endDate, _category1Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBalance_WithNullDates_UsesCurrentMonth()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expectedStart = new DateTime(now.Year, now.Month, 1);
        var expectedEnd = now.Date;

        var balanceData = new Dictionary<(Guid CategoryId, TransactionType Type), decimal>();

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(
                _userId,
                It.Is<DateTime>(d => d == expectedStart),
                It.Is<DateTime>(d => d == expectedEnd),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(balanceData);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _sut.ExecuteAsync(null, null, null);

        // Assert
        result.Should().NotBeNull();
        result.StartDate.Should().Be(expectedStart);
        result.EndDate.Should().Be(expectedEnd);

        _transactionRepositoryMock.Verify(
            x => x.GetBalanceByCategoryAsync(
                _userId,
                expectedStart,
                expectedEnd,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBalance_WithNoTransactions_ReturnsZeroBalances()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);
        var balanceData = new Dictionary<(Guid CategoryId, TransactionType Type), decimal>();

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(_userId, startDate, endDate, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balanceData);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate, null);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(0m);
        result.TotalExpense.Should().Be(0m);
        result.NetBalance.Should().Be(0m);
        result.ByCategory.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBalance_WithInvalidDates_ThrowsValidationException()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 31);
        var endDate = new DateTime(2026, 1, 1);

        // Act
        Func<Task> act = async () => await _sut.ExecuteAsync(startDate, endDate, null);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*data inicial não pode ser maior*");
    }

    [Fact]
    public async Task GetBalance_ExcludesDeletedTransactions()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

        // Repository já filtra is_deleted = false, então só retorna ativas
        var balanceData = new Dictionary<(Guid CategoryId, TransactionType Type), decimal>
        {
            { (_category1Id, TransactionType.Income), 1000m }
        };

        var categories = new List<Category>
        {
            CreateCategory(_category1Id, "Salário")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(_userId, startDate, endDate, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balanceData);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate, null);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncome.Should().Be(1000m);
        // Confirma que repository foi chamado (ele faz o filtro de is_deleted)
        _transactionRepositoryMock.Verify(
            x => x.GetBalanceByCategoryAsync(_userId, startDate, endDate, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBalance_OnlyIncludesUserTransactions()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

        var balanceData = new Dictionary<(Guid CategoryId, TransactionType Type), decimal>
        {
            { (_category1Id, TransactionType.Income), 2000m }
        };

        var categories = new List<Category>
        {
            CreateCategory(_category1Id, "Salário")
        };

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(_userId, startDate, endDate, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balanceData);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate, null);

        // Assert
        result.Should().NotBeNull();
        // Verifica que foi chamado com o userId correto
        _transactionRepositoryMock.Verify(
            x => x.GetBalanceByCategoryAsync(_userId, startDate, endDate, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private Category CreateCategory(Guid id, string name)
    {
        var category = new Category(name, L2SLedger.Domain.Enums.CategoryType.Expense, null);
        typeof(Category).GetProperty("Id")!.SetValue(category, id);
        return category;
    }
}
