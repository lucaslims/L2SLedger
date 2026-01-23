using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Moq;
using FluentValidationException = FluentValidation.ValidationException;

namespace L2SLedger.Application.Tests.UseCases.Categories;

public class UpdateCategoryUseCaseTestsFixed
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IValidator<UpdateCategoryRequest>> _validatorMock;
    private readonly UpdateCategoryUseCase _sut;

    public UpdateCategoryUseCaseTestsFixed()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _validatorMock = new Mock<IValidator<UpdateCategoryRequest>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CategoryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _sut = new UpdateCategoryUseCase(
            _categoryRepositoryMock.Object,
            _mapper,
            _validatorMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldUpdateCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category("Nome Antigo", "Descrição antiga");
        var request = new UpdateCategoryRequest
        {
            Name = "Nome Novo",
            Description = "Descrição nova"
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ExecuteAsync(categoryId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);

        _categoryRepositoryMock.Verify(x => x.UpdateAsync(existingCategory, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidRequest_ShouldThrowFluentValidationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest { Name = "" };
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Nome é obrigatório")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var act = async () => await _sut.ExecuteAsync(categoryId, request);

        // Assert
        await act.Should().ThrowAsync<FluentValidationException>();
        _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCategory_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest { Name = "Nome" };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var act = async () => await _sut.ExecuteAsync(categoryId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Categoria não encontrada*")
            .Where(ex => ex.Code == "FIN_CATEGORY_NOT_FOUND");
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateName_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category("Nome Original");
        var otherCategory = new Category("Nome Duplicado");

        var request = new UpdateCategoryRequest
        {
            Name = "Nome Duplicado"
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { existingCategory, otherCategory });

        // Act
        var act = async () => await _sut.ExecuteAsync(categoryId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Já existe outra categoria com este nome*")
            .Where(ex => ex.Code == "VAL_DUPLICATE_NAME");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category("Original", "Descrição original");
        var request = new UpdateCategoryRequest
        {
            Name = "Atualizado",
            Description = "Descrição atualizada"
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.ExecuteAsync(categoryId, request);

        // Assert
        existingCategory.Name.Should().Be("Atualizado");
        existingCategory.Description.Should().Be("Descrição atualizada");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryUpdateAsync()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category("Original");
        var request = new UpdateCategoryRequest { Name = "Atualizado" };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.ExecuteAsync(categoryId, request);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Category>(c => c.Name == "Atualizado"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnMappedCategoryDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category("Original", "Descrição");
        var request = new UpdateCategoryRequest
        {
            Name = "Atualizado",
            Description = "Nova descrição"
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.ExecuteAsync(categoryId, request);

        // Assert
        result.Should().BeOfType<CategoryDto>();
        result.Name.Should().Be("Atualizado");
        result.Description.Should().Be("Nova descrição");
    }
}
