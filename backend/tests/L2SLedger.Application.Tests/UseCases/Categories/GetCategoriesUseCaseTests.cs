using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Domain.Entities;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Categories;

public class GetCategoriesUseCaseTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly GetCategoriesUseCase _sut;

    public GetCategoriesUseCaseTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CategoryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _sut = new GetCategoriesUseCase(_categoryRepositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutFilters_ShouldReturnAllActiveCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category("Alimentação", "Gastos com alimentação"),
            new Category("Transporte", "Gastos com transporte"),
            new Category("Saúde", "Gastos com saúde")
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.ExecuteAsync(null, false);

        // Assert
        result.Should().NotBeNull();
        result.Categories.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Categories.Select(c => c.Name).Should().Contain(new[] { "Alimentação", "Transporte", "Saúde" });

        _categoryRepositoryMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithIncludeInactive_ShouldReturnAllCategories()
    {
        // Arrange
        var activeCategory = new Category("Ativa", "Categoria ativa");
        var inactiveCategory = new Category("Inativa", "Categoria inativa");
        inactiveCategory.Deactivate();

        var categories = new List<Category> { activeCategory, inactiveCategory };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.ExecuteAsync(null, true);

        // Assert
        result.Categories.Should().HaveCount(2);
        result.Categories.Should().Contain(c => c.IsActive == true);
        result.Categories.Should().Contain(c => c.IsActive == false);

        _categoryRepositoryMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithParentCategoryId_ShouldReturnSubCategories()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var subCategories = new List<Category>
        {
            new Category("Restaurantes", "Gastos em restaurantes", parentId),
            new Category("Mercado", "Gastos no mercado", parentId)
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByParentIdAsync(parentId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subCategories);

        // Act
        var result = await _sut.ExecuteAsync(parentId, false);

        // Assert
        result.Categories.Should().HaveCount(2);
        result.Categories.Should().AllSatisfy(c => c.ParentCategoryId.Should().Be(parentId));

        _categoryRepositoryMock.Verify(x => x.GetByParentIdAsync(parentId, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _sut.ExecuteAsync(null, false);

        // Assert
        result.Categories.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var includeInactive = true;

        _categoryRepositoryMock
            .Setup(x => x.GetByParentIdAsync(parentId, includeInactive, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        await _sut.ExecuteAsync(parentId, includeInactive);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.GetByParentIdAsync(parentId, includeInactive, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnMappedCategoryDtos()
    {
        // Arrange
        var parentCategory = new Category("Alimentação", "Categoria pai");
        var category = new Category("Restaurantes", "Gastos em restaurantes", parentCategory.Id);

        var categories = new List<Category> { category };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.ExecuteAsync(null, false);

        // Assert
        result.Categories.Should().AllBeOfType<CategoryDto>();
        var dto = result.Categories.First();
        dto.Id.Should().Be(category.Id);
        dto.Name.Should().Be(category.Name);
        dto.Description.Should().Be(category.Description);
        dto.IsActive.Should().Be(category.IsActive);
        dto.ParentCategoryId.Should().Be(category.ParentCategoryId);
    }
}
