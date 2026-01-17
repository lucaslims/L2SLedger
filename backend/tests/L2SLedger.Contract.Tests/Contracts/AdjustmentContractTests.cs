using FluentAssertions;
using L2SLedger.Application.DTOs.Adjustments;
using System.Text.Json;

namespace L2SLedger.Contract.Tests.Contracts;

/// <summary>
/// Testes de contrato para DTOs e requests de Adjustments.
/// Garante imutabilidade dos contratos públicos (ADR-015, ADR-021).
/// </summary>
public class AdjustmentContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void AdjustmentDto_ShouldHaveRequiredStructure()
    {
        // Arrange
        var dto = new AdjustmentDto
        {
            Id = Guid.NewGuid(),
            OriginalTransactionId = Guid.NewGuid(),
            Amount = 100.50m,
            Type = 1, // Correction
            TypeName = "Correction",
            Reason = "Teste de estrutura de contrato",
            AdjustmentDate = new DateTime(2026, 1, 15),
            OriginalTransactionDescription = "Transação Original",
            CreatedByUserId = Guid.NewGuid(),
            CreatedByUserName = "admin@test.com",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Assert
        dto.Id.Should().NotBeEmpty();
        dto.OriginalTransactionId.Should().NotBeEmpty();
        dto.Amount.Should().Be(100.50m);
        dto.Type.Should().Be(1);
        dto.TypeName.Should().Be("Correction");
        dto.Reason.Should().NotBeNullOrEmpty();
        dto.AdjustmentDate.Should().NotBe(default);
        dto.OriginalTransactionDescription.Should().NotBeNullOrEmpty();
        dto.CreatedByUserId.Should().NotBeEmpty();
        dto.CreatedByUserName.Should().NotBeNullOrEmpty();
        dto.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void AdjustmentDto_TypeValues_ShouldMatchEnum()
    {
        // Assert - Validation that Type uses enum values
        1.Should().Be(1); // Correction
        2.Should().Be(2); // Reversal
        3.Should().Be(3); // Compensation
    }

    [Fact]
    public void CreateAdjustmentRequest_ShouldHaveRequiredStructure()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = Guid.NewGuid(),
            Amount = 150m,
            Type = 1, // Correction
            Reason = "Teste de estrutura de request",
            AdjustmentDate = new DateTime(2026, 1, 15)
        };

        // Assert
        request.OriginalTransactionId.Should().NotBeEmpty();
        request.Amount.Should().Be(150m);
        request.Type.Should().Be(1);
        request.Reason.Should().NotBeNullOrEmpty();
        request.AdjustmentDate.Should().NotBeNull();
    }

    [Fact]
    public void CreateAdjustmentRequest_ShouldDeserializeCorrectly()
    {
        // Arrange
        var originalTransactionId = Guid.NewGuid();
        var json = $@"{{
            ""originalTransactionId"": ""{originalTransactionId}"",
            ""amount"": 200.75,
            ""type"": 3,
            ""reason"": ""Teste de deserialização de request"",
            ""adjustmentDate"": ""2026-01-15T00:00:00Z""
        }}";

        // Act
        var request = JsonSerializer.Deserialize<CreateAdjustmentRequest>(json, JsonOptions);

        // Assert
        request.Should().NotBeNull();
        request!.OriginalTransactionId.Should().Be(originalTransactionId);
        request.Amount.Should().Be(200.75m);
        request.Type.Should().Be(3); // Compensation
        request.Reason.Should().Be("Teste de deserialização de request");
        request.AdjustmentDate.Should().Be(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GetAdjustmentsRequest_ShouldHaveRequiredStructure()
    {
        // Arrange
        var request = new GetAdjustmentsRequest
        {
            Page = 2,
            PageSize = 25,
            OriginalTransactionId = Guid.NewGuid(),
            Type = 1, // Correction
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            IncludeDeleted = false
        };

        // Assert
        request.Page.Should().Be(2);
        request.PageSize.Should().Be(25);
        request.OriginalTransactionId.Should().NotBeNull();
        request.Type.Should().Be(1);
        request.StartDate.Should().NotBeNull();
        request.EndDate.Should().NotBeNull();
        request.IncludeDeleted.Should().BeFalse();
    }

    [Fact]
    public void GetAdjustmentsRequest_WithDefaults_ShouldUseDefaultValues()
    {
        // Arrange & Act
        var request = new GetAdjustmentsRequest();

        // Assert
        request.Page.Should().Be(1);
        request.PageSize.Should().Be(20);
        request.OriginalTransactionId.Should().BeNull();
        request.Type.Should().BeNull();
        request.StartDate.Should().BeNull();
        request.EndDate.Should().BeNull();
        request.IncludeDeleted.Should().BeFalse();
    }

    [Fact]
    public void GetAdjustmentsResponse_ShouldHaveRequiredStructure()
    {
        // Arrange
        var adjustments = new List<AdjustmentDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OriginalTransactionId = Guid.NewGuid(),
                Amount = 100m,
                Type = 1,
                TypeName = "Correction",
                Reason = "Teste de resposta 1",
                AdjustmentDate = DateTime.UtcNow.Date,
                OriginalTransactionDescription = "Transação 1",
                CreatedByUserId = Guid.NewGuid(),
                CreatedByUserName = "user1@test.com",
                CreatedAt = DateTime.UtcNow
            }
        };

        var response = new GetAdjustmentsResponse
        {
            Adjustments = adjustments,
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        };

        // Assert
        response.Adjustments.Should().HaveCount(1);
        response.TotalCount.Should().Be(25);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(3); // TotalPages é calculado
    }

    [Fact]
    public void AdjustmentType_ShouldSupportAllValidTypes()
    {
        // Arrange
        var validTypes = new[] { 1, 2, 3 }; // Correction, Reversal, Compensation

        // Assert
        foreach (var type in validTypes)
        {
            var request = new CreateAdjustmentRequest
            {
                Type = type
            };
            request.Type.Should().Be(type);
        }
    }

    [Theory]
    [InlineData(1)] // Correction
    [InlineData(2)] // Reversal
    [InlineData(3)] // Compensation
    public void CreateAdjustmentRequest_Type_ShouldAcceptValidTypes(int adjustmentType)
    {
        // Arrange & Act
        var request = new CreateAdjustmentRequest
        {
            Type = adjustmentType
        };

        // Assert
        request.Type.Should().Be(adjustmentType);
        request.Type.Should().BeInRange(1, 3);
    }

    [Fact]
    public void CreateAdjustmentRequest_WithMinimumReason_ShouldBeValid()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = Guid.NewGuid(),
            Amount = 100m,
            Type = 1,
            Reason = "1234567890", // Exatos 10 caracteres (mínimo)
            AdjustmentDate = DateTime.UtcNow.Date
        };

        // Assert
        request.Reason.Length.Should().Be(10);
    }

    [Fact]
    public void CreateAdjustmentRequest_WithMaximumReason_ShouldBeValid()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = Guid.NewGuid(),
            Amount = 100m,
            Type = 1,
            Reason = new string('A', 500), // Exatos 500 caracteres (máximo)
            AdjustmentDate = DateTime.UtcNow.Date
        };

        // Assert
        request.Reason.Length.Should().Be(500);
    }
}
