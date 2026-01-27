using FluentAssertions;
using FluentValidation;
using L2SLedger.Application.DTOs.Balances;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Balances;
using L2SLedger.Domain.Exceptions;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Balances;

/// <summary>
/// Testes para GetDailyBalanceUseCase.
/// Validar evolução diária de saldos com acumulados.
/// </summary>
public class GetDailyBalanceUseCaseTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetDailyBalanceUseCase _sut;

    private readonly Guid _userId = Guid.NewGuid();

    public GetDailyBalanceUseCaseTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(_userId);

        _sut = new GetDailyBalanceUseCase(
            _transactionRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetDailyBalance_WithValidPeriod_ReturnsCorrectDailyBalances()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 10);
        var endDate = new DateTime(2026, 1, 12);

        // Saldo antes do período: 500
        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(500m);

        // Transações diárias
        var dailyBalances = new Dictionary<DateTime, (decimal Income, decimal Expense)>
        {
            { new DateTime(2026, 1, 10), (200m, 50m) },  // +150 -> 650
            { new DateTime(2026, 1, 11), (0m, 100m) },   // -100 -> 550
            { new DateTime(2026, 1, 12), (300m, 0m) }    // +300 -> 850
        };

        _transactionRepositoryMock
            .Setup(x => x.GetDailyBalancesAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalances);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Dia 10
        result[0].Date.Should().Be(new DateTime(2026, 1, 10));
        result[0].OpeningBalance.Should().Be(500m);
        result[0].Income.Should().Be(200m);
        result[0].Expense.Should().Be(50m);
        result[0].ClosingBalance.Should().Be(650m);

        // Dia 11
        result[1].Date.Should().Be(new DateTime(2026, 1, 11));
        result[1].OpeningBalance.Should().Be(650m);
        result[1].Income.Should().Be(0m);
        result[1].Expense.Should().Be(100m);
        result[1].ClosingBalance.Should().Be(550m);

        // Dia 12
        result[2].Date.Should().Be(new DateTime(2026, 1, 12));
        result[2].OpeningBalance.Should().Be(550m);
        result[2].Income.Should().Be(300m);
        result[2].Expense.Should().Be(0m);
        result[2].ClosingBalance.Should().Be(850m);
    }

    [Fact]
    public async Task GetDailyBalance_WithNoPreviousTransactions_StartsWithZero()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var dailyBalances = new Dictionary<DateTime, (decimal Income, decimal Expense)>
        {
            { new DateTime(2026, 1, 1), (1000m, 0m) }
        };

        _transactionRepositoryMock
            .Setup(x => x.GetDailyBalancesAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalances);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result[0].OpeningBalance.Should().Be(0m);
        result[0].Income.Should().Be(1000m);
        result[0].ClosingBalance.Should().Be(1000m);
    }

    [Fact]
    public async Task GetDailyBalance_OrdersByDate_Ascending()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 5);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var dailyBalances = new Dictionary<DateTime, (decimal Income, decimal Expense)>
        {
            { new DateTime(2026, 1, 3), (300m, 0m) },
            { new DateTime(2026, 1, 1), (100m, 0m) }
        };

        _transactionRepositoryMock
            .Setup(x => x.GetDailyBalancesAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalances);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().BeInAscendingOrder(d => d.Date);
        result[0].Date.Should().Be(new DateTime(2026, 1, 1));
        result[4].Date.Should().Be(new DateTime(2026, 1, 5));
    }

    [Fact]
    public async Task GetDailyBalance_WithPeriodTooLong_ThrowsValidationException()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2026, 1, 2); // Mais de 365 dias

        // Act
        Func<Task> act = async () => await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*período máximo permitido é de 365 dias*");
    }

    [Fact]
    public async Task GetDailyBalance_CalculatesAccumulatedBalances()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 3);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100m);

        var dailyBalances = new Dictionary<DateTime, (decimal Income, decimal Expense)>
        {
            { new DateTime(2026, 1, 1), (200m, 50m) },  // Opening: 100, Closing: 250
            { new DateTime(2026, 1, 2), (100m, 100m) }, // Opening: 250, Closing: 250
            { new DateTime(2026, 1, 3), (0m, 50m) }     // Opening: 250, Closing: 200
        };

        _transactionRepositoryMock
            .Setup(x => x.GetDailyBalancesAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalances);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Verifica que ClosingBalance do dia N = OpeningBalance do dia N+1
        result[0].ClosingBalance.Should().Be(250m);
        result[1].OpeningBalance.Should().Be(250m);
        result[1].ClosingBalance.Should().Be(250m);
        result[2].OpeningBalance.Should().Be(250m);
        result[2].ClosingBalance.Should().Be(200m);
    }

    [Fact]
    public async Task GetDailyBalance_ExcludesDeletedTransactions()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 1);

        // Repository já filtra is_deleted = false
        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(_userId, startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var dailyBalances = new Dictionary<DateTime, (decimal Income, decimal Expense)>
        {
            { new DateTime(2026, 1, 1), (500m, 0m) }
        };

        _transactionRepositoryMock
            .Setup(x => x.GetDailyBalancesAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalances);

        // Act
        var result = await _sut.ExecuteAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result[0].Income.Should().Be(500m);

        // Verifica que repository foi chamado (ele faz o filtro)
        _transactionRepositoryMock.Verify(
            x => x.GetDailyBalancesAsync(_userId, startDate, endDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
