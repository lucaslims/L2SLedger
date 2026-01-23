using FluentAssertions;
using L2SLedger.API.Contracts;
using System.Text.Json;

namespace L2SLedger.Contract.Tests.Contracts;

/// <summary>
/// Testes de contrato para ErrorResponse e ErrorCodes.
/// Garante imutabilidade do modelo de erros (ADR-021).
/// </summary>
public class ErrorContractTests
{
    [Fact]
    public void ErrorResponse_ShouldHaveRequiredStructure()
    {
        // Arrange & Act
        var errorResponse = ErrorResponse.Create(
            "TEST_CODE",
            "Test message",
            "Additional details");

        // Assert
        errorResponse.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("TEST_CODE");
        errorResponse.Error.Message.Should().Be("Test message");
        errorResponse.Error.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        errorResponse.Error.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.Error.Details.Should().NotBeNull();

        // Validar estrutura do contrato
        var errorType = typeof(ErrorResponse);
        var errorProperty = errorType.GetProperty("Error");
        errorProperty.Should().NotBeNull();
        errorProperty!.PropertyType.Name.Should().Be("ErrorDetail");
    }

    [Fact]
    public void ErrorResponse_ShouldSerializeCorrectly()
    {
        // Arrange
        var errorResponse = ErrorResponse.Create("AUTH_INVALID_TOKEN", "Token inválido");

        // Act
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        json.Should().Contain("\"code\":\"AUTH_INVALID_TOKEN\"");
        json.Should().Contain("\"message\":");
        json.Should().Contain("Token");
    }

    [Fact]
    public void ErrorCodes_ShouldHaveAuthenticationCodes()
    {
        // Assert
        ErrorCodes.AUTH_INVALID_TOKEN.Should().Be("AUTH_INVALID_TOKEN");
        ErrorCodes.AUTH_UNAUTHORIZED.Should().Be("AUTH_UNAUTHORIZED");
        ErrorCodes.AUTH_EMAIL_NOT_VERIFIED.Should().Be("AUTH_EMAIL_NOT_VERIFIED");
        ErrorCodes.AUTH_SESSION_EXPIRED.Should().Be("AUTH_SESSION_EXPIRED");
    }

    [Fact]
    public void ErrorCodes_ShouldHaveValidationCodes()
    {
        // Assert
        ErrorCodes.VAL_REQUIRED_FIELD.Should().Be("VAL_REQUIRED_FIELD");
        ErrorCodes.VAL_INVALID_FORMAT.Should().Be("VAL_INVALID_FORMAT");
        ErrorCodes.VAL_INVALID_DATE.Should().Be("VAL_INVALID_DATE");
    }

    [Fact]
    public void ErrorCodes_ShouldHaveFinancialCodes()
    {
        // Assert
        ErrorCodes.FIN_INSUFFICIENT_BALANCE.Should().Be("FIN_INSUFFICIENT_BALANCE");
        ErrorCodes.FIN_PERIOD_CLOSED.Should().Be("FIN_PERIOD_CLOSED");
        ErrorCodes.FIN_DUPLICATE_ENTRY.Should().Be("FIN_DUPLICATE_ENTRY");
    }

    [Fact]
    public void ErrorCodes_ShouldHavePermissionCodes()
    {
        // Assert
        ErrorCodes.PERM_ACCESS_DENIED.Should().Be("PERM_ACCESS_DENIED");
        ErrorCodes.PERM_ROLE_REQUIRED.Should().Be("PERM_ROLE_REQUIRED");
    }

    [Fact]
    public void ErrorCodes_ShouldHaveSystemCodes()
    {
        // Assert
        ErrorCodes.SYS_INTERNAL_ERROR.Should().Be("SYS_INTERNAL_ERROR");
        ErrorCodes.SYS_UNAVAILABLE.Should().Be("SYS_UNAVAILABLE");
    }

    [Fact]
    public void ErrorCodes_ShouldHaveIntegrationCodes()
    {
        // Assert
        ErrorCodes.INT_FIREBASE_UNAVAILABLE.Should().Be("INT_FIREBASE_UNAVAILABLE");
        ErrorCodes.INT_DB_CONNECTION.Should().Be("INT_DB_CONNECTION");
    }

    [Fact]
    public void ErrorCodes_ShouldBeImmutable()
    {
        // Arrange
        var errorCodesType = typeof(ErrorCodes);

        // Act
        var fields = errorCodesType.GetFields();

        // Assert - Todos os campos devem ser const ou static readonly
        foreach (var field in fields)
        {
            (field.IsLiteral || (field.IsStatic && field.IsInitOnly)).Should().BeTrue(
                $"Campo {field.Name} deve ser const ou static readonly para garantir imutabilidade");
        }
    }

    [Fact]
    public void ErrorResponse_WithDetails_ShouldSerializeDetails()
    {
        // Arrange
        var details = "{\"field\":\"email\",\"reason\":\"Invalid format\"}";
        var errorResponse = ErrorResponse.Create("VAL_INVALID_FORMAT", "Formato inválido", details);

        // Act
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        json.Should().Contain("\"details\":");
        json.Should().Contain("\\u0022field\\u0022");
        json.Should().Contain("email");
    }
}
