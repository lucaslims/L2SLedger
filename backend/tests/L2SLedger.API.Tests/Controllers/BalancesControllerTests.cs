using FluentAssertions;
using L2SLedger.API.Controllers;
using L2SLedger.Application.DTOs.Balances;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Balances;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace L2SLedger.API.Tests.Controllers;

/// <summary>
/// Testes para BalancesController.
/// Conforme ADR-016 (RBAC/ABAC): endpoints de leitura de saldo são acessíveis
/// por todos os roles autenticados (Admin, Financeiro, Leitura).
/// </summary>
public class BalancesControllerTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<BalancesController>> _loggerMock;
    private readonly BalancesController _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTime _startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly DateTime _endDate = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);

    public BalancesControllerTests()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<BalancesController>>();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(_userId);

        var getBalanceUseCase = new GetBalanceUseCase(
            _transactionRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _currentUserServiceMock.Object);

        var getDailyBalanceUseCase = new GetDailyBalanceUseCase(
            _transactionRepositoryMock.Object,
            _currentUserServiceMock.Object);

        _sut = new BalancesController(
            getBalanceUseCase,
            getDailyBalanceUseCase,
            _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    // ─── Autorização (ADR-016) ───────────────────────────────────────────────

    [Fact]
    public void BalancesController_ClassLevel_HasAuthorizeAttribute()
    {
        // Garantir que o controller exige autenticação (qualquer role)
        var classAttributes = typeof(BalancesController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

        classAttributes.Should().ContainSingle(
            because: "BalancesController deve ter exatamente um [Authorize] a nível de classe (ADR-016)");
    }

    [Fact]
    public void BalancesController_ClassLevel_HasNoRoleRestriction()
    {
        // ADR-016: endpoints de leitura de saldo são acessíveis por todos os roles autenticados
        var authorizeAttr = typeof(BalancesController)
            .GetCustomAttribute<AuthorizeAttribute>(inherit: true);

        authorizeAttr.Should().NotBeNull();
        authorizeAttr!.Roles.Should().BeNullOrEmpty(
            because: "saldos são dados de leitura acessíveis por qualquer role autenticado (ADR-016: Leitura, Financeiro, Admin)");
    }

    [Fact]
    public void GetBalance_ActionLevel_DoesNotHaveRedundantAuthorize()
    {
        // Garantir que o atributo [Authorize] redundante foi removido do action
        var method = typeof(BalancesController).GetMethod(nameof(BalancesController.GetBalance));
        var actionAuthorize = method?.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false);

        actionAuthorize.Should().BeEmpty(
            because: "a autorização já é herdada do nível de classe; [Authorize] duplicado no action é redundante");
    }

    [Fact]
    public void GetDailyBalance_ActionLevel_DoesNotHaveRedundantAuthorize()
    {
        // Garantir que o atributo [Authorize] redundante foi removido do action
        var method = typeof(BalancesController).GetMethod(nameof(BalancesController.GetDailyBalance));
        var actionAuthorize = method?.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false);

        actionAuthorize.Should().BeEmpty(
            because: "a autorização já é herdada do nível de classe; [Authorize] duplicado no action é redundante");
    }

    // ─── GetBalance ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBalance_WithValidPeriod_ShouldReturnOk()
    {
        // Arrange
        var balanceData = new Dictionary<(Guid CategoryId, TransactionType Type), decimal>
        {
            { (Guid.NewGuid(), TransactionType.Income), 1000m },
            { (Guid.NewGuid(), TransactionType.Expense), 400m }
        };

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(
                _userId, _startDate, _endDate, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balanceData);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _sut.GetBalance(_startDate, _endDate, null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<BalanceSummaryDto>().Subject;

        dto.TotalIncome.Should().Be(1000m);
        dto.TotalExpense.Should().Be(400m);
        dto.NetBalance.Should().Be(600m);
        dto.StartDate.Should().Be(_startDate);
        dto.EndDate.Should().Be(_endDate);
    }

    [Fact]
    public async Task GetBalance_WithNullDates_ShouldUseCurrentMonthDefaults()
    {
        // Arrange: passar null para datas — use case usa defaults do mês atual
        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(
                _userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<(Guid, TransactionType), decimal>());

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _sut.GetBalance(null, null, null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>(
            because: "quando datas são null o use case usa o mês atual como default");
    }

    [Fact]
    public async Task GetBalance_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange: data inicial > data final
        var invalidStart = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);
        var invalidEnd = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _sut.GetBalance(invalidStart, invalidEnd, null, CancellationToken.None);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBalance_WithCategoryFilter_ShouldPassCategoryIdToUseCase()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceByCategoryAsync(
                _userId, _startDate, _endDate, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<(Guid, TransactionType), decimal>());

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _sut.GetBalance(_startDate, _endDate, categoryId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        _transactionRepositoryMock.Verify(
            x => x.GetBalanceByCategoryAsync(_userId, _startDate, _endDate, categoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─── GetDailyBalance ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetDailyBalance_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(
                _userId, _startDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        _transactionRepositoryMock
            .Setup(x => x.GetDailyBalancesAsync(
                _userId, _startDate, _endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<DateTime, (decimal Income, decimal Expense)>
            {
                { _startDate, (500m, 200m) }
            });

        // Act
        var result = await _sut.GetDailyBalance(_startDate, _endDate, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = okResult.Value.Should().BeAssignableTo<List<DailyBalanceDto>>().Subject;

        dtos.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDailyBalance_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange: data inicial > data final
        var invalidStart = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);
        var invalidEnd = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _sut.GetDailyBalance(invalidStart, invalidEnd, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetDailyBalance_WithPeriodExceeding365Days_ShouldReturnBadRequest()
    {
        // Arrange: período maior que 365 dias (limite definido no use case)
        var longStart = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var longEnd = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _sut.GetDailyBalance(longStart, longEnd, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>(
            because: "período máximo permitido é 365 dias");
    }

    [Fact]
    public async Task GetDailyBalance_ResultContainsDailyEntries_ForEachDayInPeriod()
    {
        // Arrange: período de 3 dias
        var start = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc);

        _transactionRepositoryMock
            .Setup(x => x.GetBalanceBeforeDateAsync(
                _userId, start, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1000m);

        _transactionRepositoryMock
            .Setup(x => x.GetDailyBalancesAsync(
                _userId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<DateTime, (decimal Income, decimal Expense)>());

        // Act
        var result = await _sut.GetDailyBalance(start, end, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = okResult.Value.Should().BeAssignableTo<List<DailyBalanceDto>>().Subject;

        dtos.Should().HaveCount(3, because: "um DailyBalanceDto deve ser gerado para cada dia do período");
    }
}
