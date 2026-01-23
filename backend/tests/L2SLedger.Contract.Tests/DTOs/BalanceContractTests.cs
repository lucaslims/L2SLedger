using System.Text.Json;
using FluentAssertions;
using L2SLedger.Application.DTOs.Balances;
using L2SLedger.Application.DTOs.Reports;

namespace L2SLedger.Contract.Tests.DTOs;

/// <summary>
/// Testes de contrato para DTOs de saldos e relatórios.
/// Valida estrutura, serialização e regras de negócio dos contratos públicos.
/// </summary>
public class BalanceContractTests
{
    [Fact]
    public void BalanceSummaryDto_ShouldHaveRequiredStructure()
    {
        // Arrange & Act
        var properties = typeof(BalanceSummaryDto).GetProperties();

        // Assert
        properties.Should().HaveCount(6);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "TotalIncome", "TotalExpense", "NetBalance",
            "StartDate", "EndDate", "ByCategory"
        });
    }

    [Fact]
    public void BalanceSummaryDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new BalanceSummaryDto
        {
            TotalIncome = 5000m,
            TotalExpense = 3000m,
            NetBalance = 2000m,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            ByCategory = new List<CategoryBalanceDto>
            {
                new CategoryBalanceDto
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Salário",
                    Income = 5000m,
                    Expense = 0m,
                    NetBalance = 5000m
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var deserialized = JsonSerializer.Deserialize<BalanceSummaryDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert
        json.Should().Contain("totalIncome");
        json.Should().Contain("totalExpense");
        json.Should().Contain("netBalance");
        json.Should().Contain("byCategory");
        deserialized.Should().NotBeNull();
        deserialized!.TotalIncome.Should().Be(5000m);
        deserialized.NetBalance.Should().Be(2000m);
        deserialized.ByCategory.Should().HaveCount(1);
    }

    [Fact]
    public void CategoryBalanceDto_ShouldHaveRequiredStructure()
    {
        // Arrange & Act
        var properties = typeof(CategoryBalanceDto).GetProperties();

        // Assert
        properties.Should().HaveCount(5);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "CategoryId", "CategoryName", "Income", "Expense", "NetBalance"
        });
    }

    [Fact]
    public void CategoryBalanceDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new CategoryBalanceDto
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = "Alimentação",
            Income = 0m,
            Expense = 1200m,
            NetBalance = -1200m
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var deserialized = JsonSerializer.Deserialize<CategoryBalanceDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert
        json.Should().Contain("categoryId");
        json.Should().Contain("categoryName");
        json.Should().Contain("netBalance");
        deserialized.Should().NotBeNull();
        deserialized!.CategoryName.Should().Be("Alimentação");
        deserialized.NetBalance.Should().Be(-1200m);
    }

    [Fact]
    public void DailyBalanceDto_ShouldHaveRequiredStructure()
    {
        // Arrange & Act
        var properties = typeof(DailyBalanceDto).GetProperties();

        // Assert
        properties.Should().HaveCount(5);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Date", "OpeningBalance", "Income", "Expense", "ClosingBalance"
        });
    }

    [Fact]
    public void DailyBalanceDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new DailyBalanceDto
        {
            Date = new DateTime(2026, 1, 15),
            OpeningBalance = 1000m,
            Income = 500m,
            Expense = 200m,
            ClosingBalance = 1300m
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var deserialized = JsonSerializer.Deserialize<DailyBalanceDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert
        json.Should().Contain("date");
        json.Should().Contain("openingBalance");
        json.Should().Contain("closingBalance");
        deserialized.Should().NotBeNull();
        deserialized!.OpeningBalance.Should().Be(1000m);
        deserialized.ClosingBalance.Should().Be(1300m);
    }

    [Fact]
    public void DailyBalanceDto_CalculatesClosingBalance()
    {
        // Arrange
        var dto = new DailyBalanceDto
        {
            Date = new DateTime(2026, 1, 10),
            OpeningBalance = 2000m,
            Income = 800m,
            Expense = 300m,
            ClosingBalance = 2500m // OpeningBalance + Income - Expense
        };

        // Act & Assert
        var expectedClosingBalance = dto.OpeningBalance + dto.Income - dto.Expense;
        dto.ClosingBalance.Should().Be(expectedClosingBalance);
        dto.ClosingBalance.Should().Be(2500m);
    }

    [Fact]
    public void CashFlowReportDto_ShouldHaveRequiredStructure()
    {
        // Arrange & Act
        var properties = typeof(CashFlowReportDto).GetProperties();

        // Assert
        properties.Should().HaveCount(6);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "StartDate", "EndDate", "OpeningBalance",
            "Movements", "ClosingBalance", "NetChange"
        });
    }

    [Fact]
    public void CashFlowReportDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new CashFlowReportDto
        {
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            OpeningBalance = 5000m,
            Movements = new List<MovementDto>
            {
                new MovementDto
                {
                    Date = new DateTime(2026, 1, 5),
                    Description = "Salário",
                    Category = "Receitas",
                    Amount = 3000m,
                    Type = "Income"
                }
            },
            ClosingBalance = 8000m,
            NetChange = 3000m
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var deserialized = JsonSerializer.Deserialize<CashFlowReportDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert
        json.Should().Contain("openingBalance");
        json.Should().Contain("movements");
        json.Should().Contain("closingBalance");
        json.Should().Contain("netChange");
        deserialized.Should().NotBeNull();
        deserialized!.OpeningBalance.Should().Be(5000m);
        deserialized.Movements.Should().HaveCount(1);
        deserialized.NetChange.Should().Be(3000m);
    }

    [Fact]
    public void CashFlowReportDto_CalculatesNetChange()
    {
        // Arrange
        var dto = new CashFlowReportDto
        {
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            OpeningBalance = 1000m,
            Movements = new List<MovementDto>(),
            ClosingBalance = 3500m,
            NetChange = 2500m // ClosingBalance - OpeningBalance
        };

        // Act & Assert
        var expectedNetChange = dto.ClosingBalance - dto.OpeningBalance;
        dto.NetChange.Should().Be(expectedNetChange);
        dto.NetChange.Should().Be(2500m);
    }

    [Fact]
    public void MovementDto_ShouldHaveRequiredStructure()
    {
        // Arrange & Act
        var properties = typeof(MovementDto).GetProperties();

        // Assert
        properties.Should().HaveCount(5);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Date", "Description", "Category", "Amount", "Type"
        });
    }

    [Fact]
    public void MovementDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new MovementDto
        {
            Date = new DateTime(2026, 1, 10),
            Description = "Compra no supermercado",
            Category = "Alimentação",
            Amount = -150m,
            Type = "Expense"
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var deserialized = JsonSerializer.Deserialize<MovementDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert
        json.Should().Contain("date");
        json.Should().Contain("description");
        json.Should().Contain("category");
        json.Should().Contain("amount");
        json.Should().Contain("type");
        deserialized.Should().NotBeNull();
        deserialized!.Description.Should().Be("Compra no supermercado");
        deserialized.Amount.Should().Be(-150m);
    }

    [Fact]
    public void MovementDto_IncomeAmount_ShouldBePositive()
    {
        // Arrange
        var incomeMovement = new MovementDto
        {
            Date = new DateTime(2026, 1, 5),
            Description = "Salário",
            Category = "Receitas",
            Amount = 5000m,
            Type = "Income"
        };

        // Assert
        incomeMovement.Type.Should().Be("Income");
        incomeMovement.Amount.Should().BePositive();
        incomeMovement.Amount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MovementDto_ExpenseAmount_ShouldBeNegative()
    {
        // Arrange
        var expenseMovement = new MovementDto
        {
            Date = new DateTime(2026, 1, 10),
            Description = "Aluguel",
            Category = "Moradia",
            Amount = -800m,
            Type = "Expense"
        };

        // Assert
        expenseMovement.Type.Should().Be("Expense");
        expenseMovement.Amount.Should().BeNegative();
        expenseMovement.Amount.Should().BeLessThan(0);
    }

    [Fact]
    public void MovementDto_ShouldIncludeCategoryName()
    {
        // Arrange
        var movement = new MovementDto
        {
            Date = new DateTime(2026, 1, 15),
            Description = "Compra online",
            Category = "Compras",
            Amount = -250m,
            Type = "Expense"
        };

        // Assert
        movement.Category.Should().NotBeNullOrEmpty();
        movement.Category.Should().Be("Compras");
    }
}
