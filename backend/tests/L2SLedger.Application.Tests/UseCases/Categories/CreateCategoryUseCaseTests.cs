using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Enums;
using L2SLedger.Domain.Exceptions;
using Moq;
using FluentValidationException = FluentValidation.ValidationException;

namespace L2SLedger.Application.Tests.UseCases.Categories;

public class CreateCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IValidator<CreateCategoryRequest>> _validatorMock;
    private readonly CreateCategoryUseCase _sut;

    public CreateCategoryUseCaseTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _validatorMock = new Mock<IValidator<CreateCategoryRequest>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CategoryMappingProfile>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _sut = new CreateCategoryUseCase(
            _categoryRepositoryMock.Object,
            _mapper,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldCreateCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Alimentação",
            Type = "Expense",
            Description = "Gastos com alimentação"
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var category = new Category(request.Name, CategoryType.Expense, request.Description);
        _categoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.IsActive.Should().BeTrue();

        _validatorMock.Verify(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidRequest_ShouldThrowFluentValidationException()
    {
        // Arrange
        var request = new CreateCategoryRequest { Name = "", Type = "Expense" };
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Nome é obrigatório")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<FluentValidationException>();
        _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateName_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Alimentação",
            Type = "Expense",
            Description = "Gastos com alimentação"
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Já existe uma categoria com este nome*")
            .Where(ex => ex.Code == "VAL_DUPLICATE_NAME");

        _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidParentId_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = new CreateCategoryRequest
        {
            Name = "Restaurantes",
            Type = "Expense",
            ParentCategoryId = parentId
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Categoria pai não encontrada*")
            .Where(ex => ex.Code == "VAL_INVALID_REFERENCE");
    }

    [Fact]
    public async Task ExecuteAsync_WithParentAsSubCategory_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var grandParentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var parentCategory = new Category("Despesas", CategoryType.Expense, "Categoria pai", grandParentId); // Já é subcategoria

        var request = new CreateCategoryRequest
        {
            Name = "Restaurantes",
            Type = "Expense",
            ParentCategoryId = parentId
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentCategory);

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*categorias raiz podem ter subcategorias*")
            .Where(ex => ex.Code == "VAL_BUSINESS_RULE_VIOLATION");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParent_ShouldCreateSubCategory()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parentCategory = new Category("Despesas", CategoryType.Expense, "Categoria raiz"); // Categoria raiz

        var request = new CreateCategoryRequest
        {
            Name = "Restaurantes",
            Type = "Expense",
            Description = "Gastos em restaurantes",
            ParentCategoryId = parentId
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentCategory);

        var subCategory = new Category(request.Name, CategoryType.Expense, request.Description, parentId);
        _categoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subCategory);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.ParentCategoryId.Should().Be(parentId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var request = new CreateCategoryRequest { Name = "Categoria", Type = "Expense" };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var category = new Category(request.Name, CategoryType.Expense);
        _categoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        await _sut.ExecuteAsync(request);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Category>(c => c.Name == request.Name), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnMappedCategoryDto()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Alimentação",
            Type = "Expense",
            Description = "Descrição"
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var category = new Category(request.Name, CategoryType.Expense, request.Description);
        _categoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Should().BeOfType<CategoryDto>();
        result.Id.Should().NotBeEmpty(); // AutoMapper cria novo GUID ao mapear
        result.Name.Should().Be(category.Name);
        result.Description.Should().Be(category.Description);
        result.IsActive.Should().Be(category.IsActive);
    }
}
