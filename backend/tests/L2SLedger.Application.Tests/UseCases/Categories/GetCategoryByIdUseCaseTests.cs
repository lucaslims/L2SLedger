using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Enums;
using L2SLedger.Domain.Exceptions;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Categories;

public class GetCategoryByIdUseCaseTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly GetCategoryByIdUseCase _sut;

    public GetCategoryByIdUseCaseTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CategoryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _sut = new GetCategoryByIdUseCase(_categoryRepositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidId_ShouldReturnCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("Alimentação", CategoryType.Expense, "Gastos com alimentação");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _sut.ExecuteAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(category.Id);
        result.Name.Should().Be(category.Name);
        result.Description.Should().Be(category.Description);
        result.IsActive.Should().Be(category.IsActive);

        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentId_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var act = async () => await _sut.ExecuteAsync(categoryId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Categoria não encontrada*")
            .Where(ex => ex.Code == "FIN_CATEGORY_NOT_FOUND");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryGetByIdAsync()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("Categoria", CategoryType.Expense);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        await _sut.ExecuteAsync(categoryId);

        // Assert
        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnMappedCategoryDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var parentCategory = new Category("Despesas", CategoryType.Expense, "Categoria pai");
        var category = new Category("Restaurantes", CategoryType.Expense, "Gastos em restaurantes", parentId);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _sut.ExecuteAsync(categoryId);

        // Assert
        result.Should().BeOfType<CategoryDto>();
        result.Id.Should().Be(category.Id);
        result.Name.Should().Be(category.Name);
        result.Description.Should().Be(category.Description);
        result.IsActive.Should().Be(category.IsActive);
        result.ParentCategoryId.Should().Be(parentId);
        result.CreatedAt.Should().BeCloseTo(category.CreatedAt, TimeSpan.FromSeconds(1));
    }
}
