using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Xunit;

namespace L2SLedger.Domain.Tests.Entities;

/// <summary>
/// Testes unitários para a entidade Adjustment.
/// Conforme ADR-015 (Imutabilidade e Ajustes Pós-Fechamento).
/// </summary>
public class AdjustmentTests
{
    private readonly Guid _validTransactionId = Guid.NewGuid();
    private readonly Guid _validUserId = Guid.NewGuid();
    private readonly DateTime _validDate = new DateTime(2026, 1, 17);

    [Fact]
    public void Constructor_WithValidData_CreatesAdjustment()
    {
        // Arrange & Act
        var adjustment = new Adjustment(
            _validTransactionId,
            100.50m,
            AdjustmentType.Correction,
            "Correção de valor digitado incorretamente",
            _validDate,
            _validUserId
        );

        // Assert
        Assert.Equal(_validTransactionId, adjustment.OriginalTransactionId);
        Assert.Equal(100.50m, adjustment.Amount);
        Assert.Equal(AdjustmentType.Correction, adjustment.Type);
        Assert.Equal("Correção de valor digitado incorretamente", adjustment.Reason);
        Assert.Equal(_validDate, adjustment.AdjustmentDate);
        Assert.Equal(_validUserId, adjustment.CreatedByUserId);
        Assert.False(adjustment.IsDeleted);
        Assert.NotEqual(Guid.Empty, adjustment.Id);
    }

    [Fact]
    public void Constructor_WithEmptyTransactionId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Adjustment(Guid.Empty, 100m, AdjustmentType.Correction, "Justificativa válida", _validDate, _validUserId));

        Assert.Contains("OriginalTransactionId é obrigatório", exception.Message);
    }

    [Fact]
    public void Constructor_WithZeroAmount_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Adjustment(_validTransactionId, 0m, AdjustmentType.Correction, "Justificativa válida", _validDate, _validUserId));

        Assert.Contains("Amount não pode ser zero", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyReason_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Adjustment(_validTransactionId, 100m, AdjustmentType.Correction, "", _validDate, _validUserId));

        Assert.Contains("Justificativa é obrigatória", exception.Message);
    }

    [Fact]
    public void Constructor_WithShortReason_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Adjustment(_validTransactionId, 100m, AdjustmentType.Correction, "Curto", _validDate, _validUserId));

        Assert.Contains("no mínimo 10 caracteres", exception.Message);
    }

    [Fact]
    public void Constructor_WithLongReason_ThrowsArgumentException()
    {
        // Arrange
        var longReason = new string('A', 501);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Adjustment(_validTransactionId, 100m, AdjustmentType.Correction, longReason, _validDate, _validUserId));

        Assert.Contains("não pode exceder 500 caracteres", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Adjustment(_validTransactionId, 100m, AdjustmentType.Correction, "Justificativa válida", _validDate, Guid.Empty));

        Assert.Contains("CreatedByUserId é obrigatório", exception.Message);
    }

    [Fact]
    public void Constructor_NormalizesDateToDateOnly()
    {
        // Arrange
        var dateTime = new DateTime(2026, 1, 17, 14, 30, 45);

        // Act
        var adjustment = new Adjustment(
            _validTransactionId,
            100m,
            AdjustmentType.Correction,
            "Justificativa válida",
            dateTime,
            _validUserId
        );

        // Assert
        Assert.Equal(new DateTime(2026, 1, 17), adjustment.AdjustmentDate);
        Assert.Equal(TimeSpan.Zero, adjustment.AdjustmentDate.TimeOfDay);
    }

    [Fact]
    public void ValidateAgainstOriginal_WithNullTransaction_ThrowsBusinessRuleException()
    {
        // Arrange
        var adjustment = new Adjustment(
            _validTransactionId,
            100m,
            AdjustmentType.Correction,
            "Justificativa válida",
            _validDate,
            _validUserId
        );

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() =>
            adjustment.ValidateAgainstOriginal(null!));

        Assert.Equal("FIN_ADJUSTMENT_INVALID_ORIGINAL", exception.Code);
    }

    [Fact]
    public void ValidateAgainstOriginal_WithDeletedTransaction_ThrowsBusinessRuleException()
    {
        // Arrange
        var adjustment = new Adjustment(
            _validTransactionId,
            100m,
            AdjustmentType.Correction,
            "Justificativa válida",
            _validDate,
            _validUserId
        );

        var transaction = new Transaction(
            "Original",
            500m,
            TransactionType.Expense,
            _validDate,
            Guid.NewGuid(),
            _validUserId
        );
        transaction.MarkAsDeleted();

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() =>
            adjustment.ValidateAgainstOriginal(transaction));

        Assert.Equal("FIN_ADJUSTMENT_ORIGINAL_DELETED", exception.Code);
    }

    [Fact]
    public void ValidateAgainstOriginal_WithReversalExceedingOriginalAmount_ThrowsBusinessRuleException()
    {
        // Arrange
        var adjustment = new Adjustment(
            _validTransactionId,
            600m, // Maior que os 500 da transação original
            AdjustmentType.Reversal,
            "Estorno total com valor excedente",
            _validDate,
            _validUserId
        );

        var transaction = new Transaction(
            "Original",
            500m,
            TransactionType.Expense,
            _validDate,
            Guid.NewGuid(),
            _validUserId
        );

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() =>
            adjustment.ValidateAgainstOriginal(transaction));

        Assert.Equal("FIN_ADJUSTMENT_REVERSAL_EXCEEDS", exception.Code);
    }

    [Fact]
    public void ValidateAgainstOriginal_WithValidReversal_DoesNotThrow()
    {
        // Arrange
        var adjustment = new Adjustment(
            _validTransactionId,
            400m,
            AdjustmentType.Reversal,
            "Estorno parcial válido",
            _validDate,
            _validUserId
        );

        var transaction = new Transaction(
            "Original",
            500m,
            TransactionType.Expense,
            _validDate,
            Guid.NewGuid(),
            _validUserId
        );

        // Act & Assert
        var exception = Record.Exception(() => adjustment.ValidateAgainstOriginal(transaction));
        Assert.Null(exception);
    }

    [Fact]
    public void CalculateAdjustedAmount_WithCorrection_ReturnsAdjustmentAmount()
    {
        // Arrange
        var adjustment = new Adjustment(
            _validTransactionId,
            300m,
            AdjustmentType.Correction,
            "Correção de valor",
            _validDate,
            _validUserId
        );

        var transaction = new Transaction(
            "Original",
            500m,
            TransactionType.Expense,
            _validDate,
            Guid.NewGuid(),
            _validUserId
        );

        // Act
        var result = adjustment.CalculateAdjustedAmount(transaction);

        // Assert
        Assert.Equal(300m, result);
    }

    [Fact]
    public void CalculateAdjustedAmount_WithReversal_SubtractsFromOriginal()
    {
        // Arrange
        var adjustment = new Adjustment(
            _validTransactionId,
            200m,
            AdjustmentType.Reversal,
            "Estorno parcial",
            _validDate,
            _validUserId
        );

        var transaction = new Transaction(
            "Original",
            500m,
            TransactionType.Expense,
            _validDate,
            Guid.NewGuid(),
            _validUserId
        );

        // Act
        var result = adjustment.CalculateAdjustedAmount(transaction);

        // Assert
        Assert.Equal(300m, result); // 500 - 200
    }

    [Fact]
    public void CalculateAdjustedAmount_WithCompensation_AddsToOriginal()
    {
        // Arrange
        var adjustment = new Adjustment(
            _validTransactionId,
            100m,
            AdjustmentType.Compensation,
            "Compensação de juros",
            _validDate,
            _validUserId
        );

        var transaction = new Transaction(
            "Original",
            500m,
            TransactionType.Expense,
            _validDate,
            Guid.NewGuid(),
            _validUserId
        );

        // Act
        var result = adjustment.CalculateAdjustedAmount(transaction);

        // Assert
        Assert.Equal(600m, result); // 500 + 100
    }

    [Fact]
    public void MarkAsDeleted_SetsIsDeletedToTrue()
    {
        // Arrange
        var adjustment = new Adjustment(
            _validTransactionId,
            100m,
            AdjustmentType.Correction,
            "Justificativa válida",
            _validDate,
            _validUserId
        );

        // Act
        adjustment.MarkAsDeleted();

        // Assert
        Assert.True(adjustment.IsDeleted);
        Assert.NotNull(adjustment.UpdatedAt);
    }
}
