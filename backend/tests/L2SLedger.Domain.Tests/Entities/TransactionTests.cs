using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Xunit;

namespace L2SLedger.Domain.Tests.Entities;

public class TransactionTests
{
    private readonly Guid _validCategoryId = Guid.NewGuid();
    private readonly Guid _validUserId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_CreatesTransaction()
    {
        // Arrange & Act
        var transaction = new Transaction(
            "Salário Janeiro",
            5000.00m,
            TransactionType.Income,
            new DateTime(2026, 1, 15),
            _validCategoryId,
            _validUserId,
            "Salário do mês",
            false,
            null
        );

        // Assert
        Assert.Equal("Salário Janeiro", transaction.Description);
        Assert.Equal(5000.00m, transaction.Amount);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(new DateTime(2026, 1, 15), transaction.TransactionDate);
        Assert.Equal(_validCategoryId, transaction.CategoryId);
        Assert.Equal(_validUserId, transaction.UserId);
        Assert.Equal("Salário do mês", transaction.Notes);
        Assert.False(transaction.IsRecurring);
        Assert.Null(transaction.RecurringDay);
        Assert.False(transaction.IsDeleted);
    }

    [Fact]
    public void Constructor_WithEmptyDescription_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Transaction("", 100m, TransactionType.Expense, DateTime.Today, _validCategoryId, _validUserId));

        Assert.Contains("Descrição é obrigatória", exception.Message);
    }

    [Fact]
    public void Constructor_WithDescriptionTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longDescription = new string('A', 201);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Transaction(longDescription, 100m, TransactionType.Expense, DateTime.Today, _validCategoryId, _validUserId));

        Assert.Contains("não pode exceder 200 caracteres", exception.Message);
    }

    [Fact]
    public void Constructor_WithZeroAmount_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Transaction("Teste", 0m, TransactionType.Expense, DateTime.Today, _validCategoryId, _validUserId));

        Assert.Contains("deve ser maior que zero", exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Transaction("Teste", -100m, TransactionType.Expense, DateTime.Today, _validCategoryId, _validUserId));

        Assert.Contains("deve ser maior que zero", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyCategoryId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Transaction("Teste", 100m, TransactionType.Expense, DateTime.Today, Guid.Empty, _validUserId));

        Assert.Contains("CategoryId é obrigatório", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Transaction("Teste", 100m, TransactionType.Expense, DateTime.Today, _validCategoryId, Guid.Empty));

        Assert.Contains("UserId é obrigatório", exception.Message);
    }

    [Fact]
    public void Constructor_WithRecurringButInvalidDay_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Transaction("Teste", 100m, TransactionType.Expense, DateTime.Today, _validCategoryId, _validUserId, null, true, 0));

        Assert.Contains("Dia de recorrência deve estar entre 1 e 31", exception.Message);
    }

    [Fact]
    public void Constructor_WithRecurringAndValidDay_CreatesRecurringTransaction()
    {
        // Arrange & Act
        var transaction = new Transaction(
            "Aluguel",
            1500m,
            TransactionType.Expense,
            DateTime.Today,
            _validCategoryId,
            _validUserId,
            null,
            true,
            10
        );

        // Assert
        Assert.True(transaction.IsRecurring);
        Assert.Equal(10, transaction.RecurringDay);
    }

    [Fact]
    public void Constructor_NormalizesTransactionDateToDateOnly()
    {
        // Arrange
        var dateWithTime = new DateTime(2026, 1, 15, 14, 30, 45);

        // Act
        var transaction = new Transaction(
            "Teste",
            100m,
            TransactionType.Income,
            dateWithTime,
            _validCategoryId,
            _validUserId
        );

        // Assert
        Assert.Equal(new DateTime(2026, 1, 15), transaction.TransactionDate);
        Assert.Equal(TimeSpan.Zero, transaction.TransactionDate.TimeOfDay);
    }

    [Fact]
    public void Update_WithValidData_UpdatesTransaction()
    {
        // Arrange
        var transaction = new Transaction(
            "Descrição Original",
            100m,
            TransactionType.Income,
            DateTime.Today,
            _validCategoryId,
            _validUserId
        );
        var newCategoryId = Guid.NewGuid();

        // Act
        transaction.Update(
            "Nova Descrição",
            200m,
            TransactionType.Expense,
            DateTime.Today.AddDays(1),
            newCategoryId,
            "Novas notas"
        );

        // Assert
        Assert.Equal("Nova Descrição", transaction.Description);
        Assert.Equal(200m, transaction.Amount);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(DateTime.Today.AddDays(1), transaction.TransactionDate);
        Assert.Equal(newCategoryId, transaction.CategoryId);
        Assert.Equal("Novas notas", transaction.Notes);
    }

    [Fact]
    public void Update_WhenDeleted_ThrowsBusinessRuleException()
    {
        // Arrange
        var transaction = new Transaction(
            "Teste",
            100m,
            TransactionType.Income,
            DateTime.Today,
            _validCategoryId,
            _validUserId
        );
        transaction.MarkAsDeleted();

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() =>
            transaction.Update("Nova", 200m, TransactionType.Expense, DateTime.Today, _validCategoryId));

        Assert.Equal("FIN_TRANSACTION_DELETED", exception.Code);
    }

    [Fact]
    public void UpdateRecurringSettings_WithValidData_UpdatesSettings()
    {
        // Arrange
        var transaction = new Transaction(
            "Teste",
            100m,
            TransactionType.Income,
            DateTime.Today,
            _validCategoryId,
            _validUserId
        );

        // Act
        transaction.UpdateRecurringSettings(true, 15);

        // Assert
        Assert.True(transaction.IsRecurring);
        Assert.Equal(15, transaction.RecurringDay);
    }

    [Fact]
    public void MarkAsDeleted_WhenNotDeleted_MarksAsDeleted()
    {
        // Arrange
        var transaction = new Transaction(
            "Teste",
            100m,
            TransactionType.Income,
            DateTime.Today,
            _validCategoryId,
            _validUserId
        );

        // Act
        transaction.MarkAsDeleted();

        // Assert
        Assert.True(transaction.IsDeleted);
    }

    [Fact]
    public void MarkAsDeleted_WhenAlreadyDeleted_ThrowsBusinessRuleException()
    {
        // Arrange
        var transaction = new Transaction(
            "Teste",
            100m,
            TransactionType.Income,
            DateTime.Today,
            _validCategoryId,
            _validUserId
        );
        transaction.MarkAsDeleted();

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleException>(() =>
            transaction.MarkAsDeleted());

        Assert.Equal("FIN_TRANSACTION_ALREADY_DELETED", exception.Code);
    }
}
