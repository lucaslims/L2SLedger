using System.Text.Json;
using FluentAssertions;
using L2SLedger.Application.DTOs.Auth;

namespace L2SLedger.Contract.Tests.DTOs;

/// <summary>
/// Testes de contrato para DTOs de autenticação.
/// Garante imutabilidade e estrutura dos contratos públicos (ADR-019).
/// </summary>
public class AuthDtoContractTests
{
    [Fact]
    public void LoginRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var dto = new LoginRequest { FirebaseIdToken = "test-token" };

        // Assert
        dto.FirebaseIdToken.Should().Be("test-token");

        // Validar estrutura do contrato
        var properties = typeof(LoginRequest).GetProperties();
        properties.Should().HaveCount(1);
        properties.Should().Contain(p => p.Name == "FirebaseIdToken" && p.PropertyType == typeof(string));
    }

    [Fact]
    public void LoginRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new LoginRequest { FirebaseIdToken = "test-token" };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        json.Should().Contain("\"firebaseIdToken\":\"test-token\"");
    }

    [Fact]
    public void LoginResponse_ShouldHaveRequiredProperties()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            Status = "Active",
            Roles = new List<string> { "Leitura" },
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var dto = new LoginResponse { User = userDto };

        // Assert
        dto.User.Should().NotBeNull();
        dto.User.Email.Should().Be("test@example.com");

        // Validar estrutura do contrato
        var properties = typeof(LoginResponse).GetProperties();
        properties.Should().Contain(p => p.Name == "User" && p.PropertyType == typeof(UserDto));
    }

    [Fact]
    public void UserDto_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var dto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            Status = "Active",
            Roles = new List<string> { "Leitura", "Financeiro" },
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Id.Should().NotBeEmpty();
        dto.Email.Should().Be("test@example.com");
        dto.DisplayName.Should().Be("Test User");
        dto.Roles.Should().HaveCount(2);
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Validar estrutura do contrato
        var properties = typeof(UserDto).GetProperties();
        properties.Should().HaveCount(6);
        properties.Should().Contain(p => p.Name == "Id" && p.PropertyType == typeof(Guid));
        properties.Should().Contain(p => p.Name == "Email" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "DisplayName" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "Status" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "Roles" && p.PropertyType.IsAssignableTo(typeof(IEnumerable<string>)));
        properties.Should().Contain(p => p.Name == "CreatedAt" && p.PropertyType == typeof(DateTime));
    }

    [Fact]
    public void UserDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new UserDto
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            Email = "test@example.com",
            DisplayName = "Test User",
            Status = "Active",
            Roles = new List<string> { "Leitura" },
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        json.Should().Contain("\"id\":\"12345678-1234-1234-1234-123456789012\"");
        json.Should().Contain("\"email\":\"test@example.com\"");
        json.Should().Contain("\"displayName\":\"Test User\"");
        json.Should().Contain("\"roles\":[\"Leitura\"]");
    }

    [Fact]
    public void CurrentUserResponse_ShouldHaveRequiredProperties()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            Status = "Active",
            Roles = new List<string> { "Admin" },
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var dto = new CurrentUserResponse { User = userDto };

        // Assert
        dto.User.Should().NotBeNull();
        dto.User.Roles.Should().Contain("Admin");

        // Validar estrutura do contrato
        var properties = typeof(CurrentUserResponse).GetProperties();
        properties.Should().Contain(p => p.Name == "User" && p.PropertyType == typeof(UserDto));
    }
}
