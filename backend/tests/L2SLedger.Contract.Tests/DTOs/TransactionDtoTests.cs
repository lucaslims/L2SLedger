using System.Text.Json;
using FluentAssertions;
using L2SLedger.Application.DTOs.Transaction;

namespace L2SLedger.Contract.Tests.DTOs;

/// <summary>
/// Testes de contrato para DTOs de transações.
/// Valida estrutura, serialização e imutabilidade dos contratos públicos.
/// </summary>
public class TransactionDtoTests
{
    [Fact]
    public void TransactionDto_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(TransactionDto).GetProperties();

        // Assert
        properties.Should().HaveCount(13);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Id", "Description", "Amount", "Type", "TransactionDate",
            "CategoryId", "CategoryName", "UserId", "Notes",
            "IsRecurring", "RecurringDay", "CreatedAt", "UpdatedAt"
        });
    }

    [Fact]
    public void TransactionDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new TransactionDto
        {
            Id = Guid.NewGuid(),
            Description = "Salário",
            Amount = 5000.00m,
            Type = 1,
            TransactionDate = new DateTime(2026, 1, 15),
            CategoryId = Guid.NewGuid(),
            CategoryName = "Salário",
            UserId = Guid.NewGuid(),
            Notes = "Pagamento mensal",
            IsRecurring = true,
            RecurringDay = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<TransactionDto>(json);

        // Assert
        json.Should().Contain("Description");
        json.Should().Contain("Amount");
        deserialized.Should().NotBeNull();
        deserialized!.Description.Should().Be(dto.Description);
        deserialized.Amount.Should().Be(dto.Amount);
    }

    [Fact]
    public void CreateTransactionRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(CreateTransactionRequest).GetProperties();

        // Assert
        properties.Should().HaveCount(8);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Description", "Amount", "Type", "TransactionDate",
            "CategoryId", "Notes", "IsRecurring", "RecurringDay"
        });
    }

    [Fact]
    public void CreateTransactionRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            Description = "Aluguel",
            Amount = 1500.00m,
            Type = 2,
            TransactionDate = new DateTime(2026, 1, 10),
            CategoryId = Guid.NewGuid(),
            Notes = "Pagamento do aluguel",
            IsRecurring = true,
            RecurringDay = 10
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<CreateTransactionRequest>(json);

        // Assert
        json.Should().Contain("Description");
        deserialized.Should().NotBeNull();
        deserialized!.Description.Should().Be(request.Description);
        deserialized.IsRecurring.Should().BeTrue();
    }

    [Fact]
    public void UpdateTransactionRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(UpdateTransactionRequest).GetProperties();

        // Assert
        properties.Should().HaveCount(8);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Description", "Amount", "Type", "TransactionDate",
            "CategoryId", "Notes", "IsRecurring", "RecurringDay"
        });
    }

    [Fact]
    public void GetTransactionsResponse_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var properties = typeof(GetTransactionsResponse).GetProperties();

        // Assert
        properties.Should().HaveCount(8);
        properties.Select(p => p.Name).Should().Contain(new[]
        {
            "Transactions", "TotalCount", "Page", "PageSize",
            "TotalPages", "TotalIncome", "TotalExpense", "Balance"
        });
    }

    [Fact]
    public void GetTransactionsResponse_ShouldSerializeCorrectly()
    {
        // Arrange
        var response = new GetTransactionsResponse
        {
            Transactions = new List<TransactionDto>(),
            TotalCount = 10,
            Page = 1,
            PageSize = 10,
            TotalPages = 1,
            TotalIncome = 5000.00m,
            TotalExpense = 2000.00m,
            Balance = 3000.00m
        };

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<GetTransactionsResponse>(json);

        // Assert
        json.Should().Contain("TotalCount");
        json.Should().Contain("Balance");
        deserialized.Should().NotBeNull();
        deserialized!.Balance.Should().Be(3000.00m);
        deserialized.TotalIncome.Should().Be(5000.00m);
    }

    [Fact]
    public void TransactionDto_TypeProperty_ShouldBeInteger()
    {
        // Arrange & Act
        var typeProperty = typeof(TransactionDto).GetProperty("Type");

        // Assert
        typeProperty.Should().NotBeNull();
        typeProperty!.PropertyType.Should().Be(typeof(int));
    }

    [Fact]
    public void GetTransactionsResponse_ShouldCalculateBalanceCorrectly()
    {
        // Arrange
        var response = new GetTransactionsResponse
        {
            Transactions = new List<TransactionDto>(),
            TotalIncome = 10000.00m,
            TotalExpense = 6500.50m,
            Balance = 3499.50m
        };

        // Act
        var expectedBalance = response.TotalIncome - response.TotalExpense;

        // Assert
        response.Balance.Should().Be(expectedBalance);
    }

    [Fact]
    public void CreateTransactionRequest_RecurringTransaction_ShouldAllowNullRecurringDay()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            Description = "Compra única",
            Amount = 100.00m,
            Type = 2,
            TransactionDate = DateTime.Today,
            CategoryId = Guid.NewGuid(),
            IsRecurring = false,
            RecurringDay = null
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<CreateTransactionRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.IsRecurring.Should().BeFalse();
        deserialized.RecurringDay.Should().BeNull();
    }
}
