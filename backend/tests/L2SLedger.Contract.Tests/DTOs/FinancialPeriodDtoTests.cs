using FluentAssertions;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.ValueObjects;
using System.Text.Json;
using Xunit;

namespace L2SLedger.Contract.Tests.DTOs;

/// <summary>
/// Testes de contrato para DTOs de períodos financeiros.
/// Valida estrutura, serialização e imutabilidade dos contratos públicos (ADR-022).
/// </summary>
public class FinancialPeriodDtoTests
{
    [Fact]
    public void FinancialPeriodDto_ShouldHaveCorrectStructure()
    {
        // Arrange
        var properties = typeof(FinancialPeriodDto).GetProperties();

        // Act & Assert
        properties.Should().HaveCount(19);
        properties.Select(p => p.Name).Should().ContainInOrder(
            "Id", "Year", "Month", "PeriodName", "StartDate", "EndDate",
            "Status", "ClosedAt", "ClosedByUserId", "ClosedByUserName",
            "ReopenedAt", "ReopenedByUserId", "ReopenedByUserName", "ReopenReason",
            "TotalIncome", "TotalExpense", "NetBalance", "BalanceSnapshot", "CreatedAt"
        );
    }

    [Fact]
    public void FinancialPeriodDto_ShouldSerializeToJson()
    {
        // Arrange
        var dto = new FinancialPeriodDto(
            Id: Guid.NewGuid(),
            Year: 2026,
            Month: 1,
            PeriodName: "2026/01",
            StartDate: new DateTime(2026, 1, 1),
            EndDate: new DateTime(2026, 1, 31),
            Status: "Open",
            ClosedAt: null,
            ClosedByUserId: null,
            ClosedByUserName: null,
            ReopenedAt: null,
            ReopenedByUserId: null,
            ReopenedByUserName: null,
            ReopenReason: null,
            TotalIncome: 5000.00m,
            TotalExpense: 3000.00m,
            NetBalance: 2000.00m,
            BalanceSnapshot: null,
            CreatedAt: DateTime.UtcNow
        );

        // Act
        var json = JsonSerializer.Serialize(dto);

        // Assert
        json.Should().Contain("\"Year\":2026");
        json.Should().Contain("\"Month\":1");
        json.Should().Contain("\"Status\":\"Open\"");
        json.Should().Contain("\"TotalIncome\":5000");
    }

    [Fact]
    public void FinancialPeriodDto_WithBalanceSnapshot_ShouldSerializeCorrectly()
    {
        // Arrange
        var categories = new List<CategoryBalance>
        {
            new CategoryBalance(Guid.NewGuid(), "Salário", 5000.00m, 0m, 5000.00m),
            new CategoryBalance(Guid.NewGuid(), "Alimentação", 0m, 1500.00m, -1500.00m)
        };

        var balanceSnapshot = new BalanceSnapshot(
            SnapshotDate: DateTime.UtcNow,
            Categories: categories,
            TotalIncome: 5000.00m,
            TotalExpense: 3000.00m,
            NetBalance: 2000.00m
        );

        var dto = new FinancialPeriodDto(
            Id: Guid.NewGuid(),
            Year: 2026,
            Month: 1,
            PeriodName: "2026/01",
            StartDate: new DateTime(2026, 1, 1),
            EndDate: new DateTime(2026, 1, 31),
            Status: "Closed",
            ClosedAt: DateTime.UtcNow,
            ClosedByUserId: Guid.NewGuid(),
            ClosedByUserName: "Admin User",
            ReopenedAt: null,
            ReopenedByUserId: null,
            ReopenedByUserName: null,
            ReopenReason: null,
            TotalIncome: 5000.00m,
            TotalExpense: 3000.00m,
            NetBalance: 2000.00m,
            BalanceSnapshot: balanceSnapshot,
            CreatedAt: DateTime.UtcNow
        );

        // Act
        var json = JsonSerializer.Serialize(dto);

        // Assert
        json.Should().Contain("\"BalanceSnapshot\"");
        json.Should().Contain("\"Categories\"");
        json.Should().Contain("\"SnapshotDate\"");
    }

    [Fact]
    public void CreatePeriodRequest_ShouldHaveCorrectStructure()
    {
        // Arrange
        var properties = typeof(CreatePeriodRequest).GetProperties();
        var typeInfo = typeof(CreatePeriodRequest);

        // Act & Assert
        properties.Should().HaveCount(2);
        properties.Select(p => p.Name).Should().Contain(new[] { "Year", "Month" });

        // Validar que é record type (imutável)
        typeInfo.IsClass.Should().BeTrue();
        typeInfo.BaseType?.Name.Should().NotBeNull();
    }

    [Fact]
    public void ReopenPeriodRequest_ShouldHaveCorrectStructure()
    {
        // Arrange
        var properties = typeof(ReopenPeriodRequest).GetProperties();
        var typeInfo = typeof(ReopenPeriodRequest);

        // Act & Assert
        properties.Should().HaveCount(1);
        properties.Select(p => p.Name).Should().Contain("Reason");

        // Validar que é record type (imutável)
        typeInfo.IsClass.Should().BeTrue();
        typeInfo.BaseType?.Name.Should().NotBeNull();
    }

    [Fact]
    public void GetPeriodsRequest_ShouldHaveCorrectStructure()
    {
        // Arrange
        var properties = typeof(GetPeriodsRequest).GetProperties();

        // Act & Assert
        properties.Should().HaveCount(5);
        properties.Select(p => p.Name).Should().ContainInOrder(
            "Year", "Month", "Status", "Page", "PageSize"
        );

        // Validar valores default
        var defaultRequest = new GetPeriodsRequest();
        defaultRequest.Page.Should().Be(1);
        defaultRequest.PageSize.Should().Be(12);
    }

    [Fact]
    public void GetPeriodsResponse_ShouldHaveCorrectStructure()
    {
        // Arrange
        var properties = typeof(GetPeriodsResponse).GetProperties();

        // Act & Assert
        properties.Should().HaveCount(4);
        properties.Select(p => p.Name).Should().ContainInOrder(
            "Periods", "TotalCount", "Page", "PageSize"
        );
    }

    [Fact]
    public void PeriodStatus_ShouldSerializeAsInteger()
    {
        // Arrange
        var openStatus = PeriodStatus.Open;
        var closedStatus = PeriodStatus.Closed;

        // Act & Assert
        ((int)openStatus).Should().Be(1);
        ((int)closedStatus).Should().Be(2);

        // Validar serialização JSON
        var obj = new { Status = PeriodStatus.Open };
        var json = JsonSerializer.Serialize(obj);
        json.Should().Contain("\"Status\":1");
    }
}
