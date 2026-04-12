using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using L2SLedger.Application.DTOs.Adjustments;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Adjustments;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Moq;
using FluentValidationException = FluentValidation.ValidationException;
using DomainTransaction = L2SLedger.Domain.Entities.Transaction;

namespace L2SLedger.Application.Tests.UseCases.Adjustments;

/// <summary>
/// Testes para CreateAdjustmentUseCase.
/// Conforme ADR-015 (Imutabilidade e Ajustes Pós-Fechamento).
/// </summary>
public class CreateAdjustmentUseCaseTests
{
    private readonly Mock<IAdjustmentRepository> _adjustmentRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IValidator<CreateAdjustmentRequest>> _validatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;
    private readonly CreateAdjustmentUseCase _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _transactionId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public CreateAdjustmentUseCaseTests()
    {
        _adjustmentRepositoryMock = new Mock<IAdjustmentRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _validatorMock = new Mock<IValidator<CreateAdjustmentRequest>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AdjustmentProfile>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(_userId);
        _currentUserServiceMock.Setup(x => x.GetUserName()).Returns("Test User");

        _sut = new CreateAdjustmentUseCase(
            _adjustmentRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _validatorMock.Object,
            _currentUserServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldCreateAdjustment()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = _transactionId,
            Amount = 100m,
            Type = 1, // Correction
            Reason = "Correção de valor digitado incorretamente"
        };

        var transaction = new DomainTransaction(
            "Original Transaction",
            500m,
            TransactionType.Expense,
            DateTime.UtcNow.Date,
            _categoryId,
            _userId
        );

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(_transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        _adjustmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OriginalTransactionId.Should().Be(_transactionId);
        result.Amount.Should().Be(100m);
        result.Type.Should().Be(1);
        result.Reason.Should().Be(request.Reason);

        _validatorMock.Verify(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _transactionRepositoryMock.Verify(x => x.GetByIdAsync(_transactionId, It.IsAny<CancellationToken>()), Times.Once);
        _adjustmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = Guid.Empty,
            Amount = 0m,
            Type = 1,
            Reason = ""
        };

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("OriginalTransactionId", "OriginalTransactionId é obrigatório"),
            new ValidationFailure("Amount", "Amount não pode ser zero"),
            new ValidationFailure("Reason", "Justificativa é obrigatória")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<FluentValidationException>();
        _transactionRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentTransaction_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = _transactionId,
            Amount = 100m,
            Type = 1,
            Reason = "Correção válida"
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(_transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainTransaction?)null);

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Transação original não encontrada*")
            .Where(ex => ex.Code == "FIN_ADJUSTMENT_INVALID_ORIGINAL");

        _adjustmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithTransactionFromDifferentUser_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = _transactionId,
            Amount = 100m,
            Type = 1,
            Reason = "Correção válida"
        };

        var transaction = new DomainTransaction(
            "Original Transaction",
            500m,
            TransactionType.Expense,
            DateTime.UtcNow.Date,
            _categoryId,
            otherUserId // Diferente do usuário atual
        );

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(_transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*não tem permissão*")
            .Where(ex => ex.Code == "FIN_ADJUSTMENT_UNAUTHORIZED");

        _adjustmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithReversalExceedingOriginalAmount_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = _transactionId,
            Amount = 600m, // Maior que o valor original
            Type = 2, // Reversal
            Reason = "Estorno total com excesso"
        };

        var transaction = new DomainTransaction(
            "Original Transaction",
            500m,
            TransactionType.Expense,
            DateTime.UtcNow.Date,
            _categoryId,
            _userId
        );

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(_transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*não pode exceder o valor da transação original*")
            .Where(ex => ex.Code == "FIN_ADJUSTMENT_REVERSAL_EXCEEDS");

        _adjustmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithDeletedTransaction_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = _transactionId,
            Amount = 100m,
            Type = 1,
            Reason = "Correção válida"
        };

        var transaction = new DomainTransaction(
            "Original Transaction",
            500m,
            TransactionType.Expense,
            DateTime.UtcNow.Date,
            _categoryId,
            _userId
        );
        transaction.MarkAsDeleted();

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(_transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*transação excluída*")
            .Where(ex => ex.Code == "FIN_ADJUSTMENT_ORIGINAL_DELETED");

        _adjustmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullAdjustmentDate_ShouldUseToday()
    {
        // Arrange
        var request = new CreateAdjustmentRequest
        {
            OriginalTransactionId = _transactionId,
            Amount = 100m,
            Type = 1,
            Reason = "Correção válida",
            AdjustmentDate = null // Não especificado
        };

        var transaction = new DomainTransaction(
            "Original Transaction",
            500m,
            TransactionType.Expense,
            DateTime.UtcNow.Date,
            _categoryId,
            _userId
        );

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(_transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        _adjustmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.AdjustmentDate.Should().Be(DateTime.UtcNow.Date);
    }
}
