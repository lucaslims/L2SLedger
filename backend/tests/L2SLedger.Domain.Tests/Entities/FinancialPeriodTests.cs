using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Domain.Tests.Entities;

public class FinancialPeriodTests
{
    [Fact]
    public void Constructor_ShouldCreateValidPeriod_WithStatusOpen()
    {
        // Arrange & Act
        var period = new FinancialPeriod(2026, 1);

        // Assert
        Assert.Equal(2026, period.Year);
        Assert.Equal(1, period.Month);
        Assert.Equal(PeriodStatus.Open, period.Status);
        Assert.True(period.IsOpen());
        Assert.False(period.IsClosed());
    }

    [Fact]
    public void Constructor_ShouldCalculateStartDateAndEndDate_Correctly()
    {
        // Arrange & Act
        var period = new FinancialPeriod(2026, 2);

        // Assert
        Assert.Equal(new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), period.StartDate);
        Assert.Equal(new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), period.EndDate);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    [InlineData(1800)]
    public void Constructor_WithInvalidYear_ShouldThrowArgumentException(int invalidYear)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FinancialPeriod(invalidYear, 1));
        Assert.Equal("year", exception.ParamName);
        Assert.Contains("2000 e 2100", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Constructor_WithInvalidMonth_ShouldThrowArgumentException(int invalidMonth)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FinancialPeriod(2026, invalidMonth));
        Assert.Equal("month", exception.ParamName);
        Assert.Contains("1 e 12", exception.Message);
    }

    [Fact]
    public void Close_ShouldUpdateStatusToClosed()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"SnapshotDate\":\"2026-01-31T00:00:00Z\",\"Categories\":[],\"TotalIncome\":1000.0,\"TotalExpense\":500.0,\"NetBalance\":500.0}";

        // Act
        period.Close(userId, 1000m, 500m, snapshotJson);

        // Assert
        Assert.Equal(PeriodStatus.Closed, period.Status);
        Assert.True(period.IsClosed());
        Assert.False(period.IsOpen());
    }

    [Fact]
    public void Close_ShouldRegisterUserAndTimestamp()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"test\":\"data\"}";
        var beforeClose = DateTime.UtcNow;

        // Act
        period.Close(userId, 1000m, 500m, snapshotJson);

        // Assert
        Assert.Equal(userId, period.ClosedByUserId);
        Assert.NotNull(period.ClosedAt);
        Assert.True(period.ClosedAt >= beforeClose);
        Assert.True(period.ClosedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Close_ShouldCalculateBalancesCorrectly()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"test\":\"data\"}";

        // Act
        period.Close(userId, 2500.50m, 1200.30m, snapshotJson);

        // Assert
        Assert.Equal(2500.50m, period.TotalIncome);
        Assert.Equal(1200.30m, period.TotalExpense);
        Assert.Equal(1300.20m, period.NetBalance);
        Assert.Equal(snapshotJson, period.BalanceSnapshotJson);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"test\":\"data\"}";
        period.Close(userId, 1000m, 500m, snapshotJson);

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() =>
            period.Close(userId, 2000m, 800m, snapshotJson));
        Assert.Equal("FIN_PERIOD_ALREADY_CLOSED", exception.Code);
        Assert.Contains("2026/01", exception.Message);
        Assert.Contains("já está fechado", exception.Message);
    }

    [Fact]
    public void Close_WithEmptySnapshot_ShouldThrowArgumentException()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            period.Close(userId, 1000m, 500m, ""));
        Assert.Equal("balanceSnapshotJson", exception.ParamName);
        Assert.Contains("Snapshot de saldos é obrigatório", exception.Message);
    }

    [Fact]
    public void Reopen_ShouldUpdateStatusToOpen()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"test\":\"data\"}";
        period.Close(userId, 1000m, 500m, snapshotJson);

        // Act
        period.Reopen(userId, "Correção de lançamento necessária");

        // Assert
        Assert.Equal(PeriodStatus.Open, period.Status);
        Assert.True(period.IsOpen());
        Assert.False(period.IsClosed());
    }

    [Fact]
    public void Reopen_ShouldRegisterReasonAndUser()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var closeUserId = Guid.NewGuid();
        var reopenUserId = Guid.NewGuid();
        var snapshotJson = "{\"test\":\"data\"}";
        var reason = "Ajuste de saldo necessário";
        period.Close(closeUserId, 1000m, 500m, snapshotJson);
        var beforeReopen = DateTime.UtcNow;

        // Act
        period.Reopen(reopenUserId, reason);

        // Assert
        Assert.Equal(reopenUserId, period.ReopenedByUserId);
        Assert.Equal(reason, period.ReopenReason);
        Assert.NotNull(period.ReopenedAt);
        Assert.True(period.ReopenedAt >= beforeReopen);
        Assert.True(period.ReopenedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Reopen_WhenAlreadyOpen_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() =>
            period.Reopen(userId, "Tentativa de reabrir período já aberto"));
        Assert.Equal("FIN_PERIOD_ALREADY_OPEN", exception.Code);
        Assert.Contains("2026/01", exception.Message);
        Assert.Contains("já está aberto", exception.Message);
    }

    [Fact]
    public void Reopen_WithoutReason_ShouldThrowArgumentException()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"test\":\"data\"}";
        period.Close(userId, 1000m, 500m, snapshotJson);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            period.Reopen(userId, ""));
        Assert.Equal("reason", exception.ParamName);
        Assert.Contains("Justificativa é obrigatória", exception.Message);
    }

    [Fact]
    public void Reopen_WithReasonLessThan10Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var userId = Guid.NewGuid();
        var snapshotJson = "{\"test\":\"data\"}";
        period.Close(userId, 1000m, 500m, snapshotJson);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            period.Reopen(userId, "Curto"));
        Assert.Equal("reason", exception.ParamName);
        Assert.Contains("pelo menos 10 caracteres", exception.Message);
    }

    [Fact]
    public void ContainsDate_ShouldReturnTrue_ForDateInPeriod()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var dateInPeriod = new DateTime(2026, 1, 15);

        // Act
        var result = period.ContainsDate(dateInPeriod);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(2025, 12, 31)]
    [InlineData(2026, 2, 1)]
    [InlineData(2027, 1, 1)]
    public void ContainsDate_ShouldReturnFalse_ForDateOutsidePeriod(int year, int month, int day)
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);
        var dateOutsidePeriod = new DateTime(year, month, day);

        // Act
        var result = period.ContainsDate(dateOutsidePeriod);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOpen_ShouldReturnTrue_ForOpenPeriod()
    {
        // Arrange
        var period = new FinancialPeriod(2026, 1);

        // Act & Assert
        Assert.True(period.IsOpen());
        Assert.False(period.IsClosed());
    }

    [Fact]
    public void GetPeriodName_ShouldReturnCorrectFormat()
    {
        // Arrange
        var period1 = new FinancialPeriod(2026, 1);
        var period2 = new FinancialPeriod(2026, 12);

        // Act
        var name1 = period1.GetPeriodName();
        var name2 = period2.GetPeriodName();

        // Assert
        Assert.Equal("2026/01", name1);
        Assert.Equal("2026/12", name2);
    }
}
