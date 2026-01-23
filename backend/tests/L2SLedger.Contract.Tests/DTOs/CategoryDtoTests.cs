using System.Text.Json;
using FluentAssertions;
using L2SLedger.Application.DTOs.Categories;

namespace L2SLedger.Contract.Tests.DTOs;

/// <summary>
/// Testes de contrato para DTOs de categorias.
/// Garante imutabilidade e estrutura dos contratos públicos (ADR-022).
/// </summary>
public class CategoryDtoTests
{
    [Fact]
    public void CategoryDto_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var dto = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Alimentação",
            Description = "Gastos com alimentação",
            IsActive = true,
            ParentCategoryId = null,
            ParentCategoryName = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Assert
        dto.Id.Should().NotBeEmpty();
        dto.Name.Should().Be("Alimentação");
        dto.Description.Should().Be("Gastos com alimentação");
        dto.IsActive.Should().BeTrue();
        dto.ParentCategoryId.Should().BeNull();
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Validar estrutura do contrato
        var properties = typeof(CategoryDto).GetProperties();
        properties.Should().HaveCount(8);
        properties.Should().Contain(p => p.Name == "Id" && p.PropertyType == typeof(Guid));
        properties.Should().Contain(p => p.Name == "Name" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "Description" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "IsActive" && p.PropertyType == typeof(bool));
        properties.Should().Contain(p => p.Name == "ParentCategoryId" && p.PropertyType == typeof(Guid?));
        properties.Should().Contain(p => p.Name == "ParentCategoryName" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "CreatedAt" && p.PropertyType == typeof(DateTime));
        properties.Should().Contain(p => p.Name == "UpdatedAt" && p.PropertyType == typeof(DateTime?));
    }

    [Fact]
    public void CategoryDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new CategoryDto
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            Name = "Alimentação",
            Description = "Gastos com alimentação",
            IsActive = true,
            ParentCategoryId = null,
            ParentCategoryName = null,
            CreatedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = null
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        json.Should().Contain("\"id\":\"12345678-1234-1234-1234-123456789012\"");
        json.Should().Contain("\\u00E7\\u00E3o"); // Aceitar Unicode escaping: Alimentação vira Alimenta\u00E7\u00E3o
        json.Should().Contain("\"isActive\":true");
        json.Should().Contain("\"parentCategoryId\":null");
    }

    [Fact]
    public void CategoryDto_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""id"": ""12345678-1234-1234-1234-123456789012"",
            ""name"": ""Alimentação"",
            ""description"": ""Gastos com alimentação"",
            ""isActive"": true,
            ""parentCategoryId"": null,
            ""parentCategoryName"": null,
            ""createdAt"": ""2026-01-15T10:00:00Z"",
            ""updatedAt"": null
        }";

        // Act
        var dto = JsonSerializer.Deserialize<CategoryDto>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(Guid.Parse("12345678-1234-1234-1234-123456789012"));
        dto.Name.Should().Be("Alimentação");
        dto.Description.Should().Be("Gastos com alimentação");
        dto.IsActive.Should().BeTrue();
        dto.ParentCategoryId.Should().BeNull();
    }

    [Fact]
    public void CreateCategoryRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var dto = new CreateCategoryRequest
        {
            Name = "Alimentação",
            Description = "Gastos com alimentação",
            ParentCategoryId = null
        };

        // Assert
        dto.Name.Should().Be("Alimentação");
        dto.Description.Should().Be("Gastos com alimentação");
        dto.ParentCategoryId.Should().BeNull();

        // Validar estrutura do contrato
        var properties = typeof(CreateCategoryRequest).GetProperties();
        properties.Should().HaveCount(3);
        properties.Should().Contain(p => p.Name == "Name" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "Description" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "ParentCategoryId" && p.PropertyType == typeof(Guid?));
    }

    [Fact]
    public void CreateCategoryRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new CreateCategoryRequest
        {
            Name = "Restaurantes",
            Description = "Gastos em restaurantes",
            ParentCategoryId = Guid.Parse("12345678-1234-1234-1234-123456789012")
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        json.Should().Contain("\"name\":\"Restaurantes\"");
        json.Should().Contain("\"description\":\"Gastos em restaurantes\"");
        json.Should().Contain("\"parentCategoryId\":\"12345678-1234-1234-1234-123456789012\"");
    }

    [Fact]
    public void UpdateCategoryRequest_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var dto = new UpdateCategoryRequest
        {
            Name = "Alimentação Atualizada",
            Description = "Nova descrição"
        };

        // Assert
        dto.Name.Should().Be("Alimentação Atualizada");
        dto.Description.Should().Be("Nova descrição");

        // Validar estrutura do contrato
        var properties = typeof(UpdateCategoryRequest).GetProperties();
        properties.Should().HaveCount(2); // Name e Description apenas
        properties.Should().Contain(p => p.Name == "Name" && p.PropertyType == typeof(string));
        properties.Should().Contain(p => p.Name == "Description" && p.PropertyType == typeof(string));
    }

    [Fact]
    public void UpdateCategoryRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new UpdateCategoryRequest
        {
            Name = "Categoria Atualizada",
            Description = "Descrição atualizada"
        };

        // Act
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Assert
        json.Should().Contain("\"name\":\"Categoria Atualizada\"");
        json.Should().Contain("\\u00E7\\u00E3o atualizada"); // Aceitar Unicode escaping: Descrição vira Descri\u00E7\u00E3o
    }

    [Fact]
    public void GetCategoriesResponse_ShouldHaveRequiredProperties()
    {
        // Arrange
        var categories = new List<CategoryDto>
        {
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Alimentação",
                Description = "Gastos",
                IsActive = true,
                ParentCategoryId = null,
                ParentCategoryName = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            }
        };

        // Act
        var dto = new GetCategoriesResponse
        {
            Categories = categories,
            TotalCount = categories.Count
        };

        // Assert
        dto.Categories.Should().HaveCount(1);
        dto.TotalCount.Should().Be(1);

        // Validar estrutura do contrato
        var properties = typeof(GetCategoriesResponse).GetProperties();
        properties.Should().HaveCount(2);
        properties.Should().Contain(p => p.Name == "Categories");
        properties.Should().Contain(p => p.Name == "TotalCount" && p.PropertyType == typeof(int));
    }
}
