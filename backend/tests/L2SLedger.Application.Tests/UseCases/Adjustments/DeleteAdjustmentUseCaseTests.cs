using FluentAssertions;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Adjustments;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Adjustments;

public class DeleteAdjustmentUseCaseTests
{
    private readonly Mock<IAdjustmentRepository> _adjustmentRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteAdjustmentUseCase _sut;

    private readonly Guid _adjustmentId = Guid.NewGuid();

    public DeleteAdjustmentUseCaseTests()
    {
        _adjustmentRepositoryMock = new Mock<IAdjustmentRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _sut = new DeleteAdjustmentUseCase(
            _adjustmentRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithAdminUser_ShouldDeleteAdjustment()
    {
        // Arrange
        var adjustment = CreateAdjustment();

        _currentUserServiceMock
            .Setup(x => x.IsInRole("Admin"))
            .Returns(true);

        _adjustmentRepositoryMock
            .Setup(x => x.GetByIdAsync(_adjustmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adjustment);

        _adjustmentRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(_adjustmentId);

        // Assert
        _adjustmentRepositoryMock.Verify(x => x.GetByIdAsync(_adjustmentId, It.IsAny<CancellationToken>()), Times.Once);
        _adjustmentRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonAdminUser_ShouldThrowBusinessRuleException()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.IsInRole("Admin"))
            .Returns(false);

        // Act
        var act = async () => await _sut.ExecuteAsync(_adjustmentId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*administradores podem deletar ajustes*")
            .Where(ex => ex.Code == ErrorCodes.PERM_INSUFFICIENT_PRIVILEGES);

        _adjustmentRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentAdjustment_ShouldThrowBusinessRuleException()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.IsInRole("Admin"))
            .Returns(true);

        _adjustmentRepositoryMock
            .Setup(x => x.GetByIdAsync(_adjustmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Adjustment?)null);

        // Act
        var act = async () => await _sut.ExecuteAsync(_adjustmentId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Ajuste não encontrado*")
            .Where(ex => ex.Code == "FIN_ADJUSTMENT_NOT_FOUND");

        _adjustmentRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyDeletedAdjustment_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var adjustment = CreateAdjustment();
        adjustment.MarkAsDeleted();

        _currentUserServiceMock
            .Setup(x => x.IsInRole("Admin"))
            .Returns(true);

        _adjustmentRepositoryMock
            .Setup(x => x.GetByIdAsync(_adjustmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adjustment);

        // Act
        var act = async () => await _sut.ExecuteAsync(_adjustmentId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*já foi excluído*")
            .Where(ex => ex.Code == "FIN_ADJUSTMENT_ALREADY_DELETED");

        _adjustmentRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkAsDeleted_NotPhysicallyDelete()
    {
        // Arrange
        var adjustment = CreateAdjustment();
        Adjustment? capturedAdjustment = null;

        _currentUserServiceMock
            .Setup(x => x.IsInRole("Admin"))
            .Returns(true);

        _adjustmentRepositoryMock
            .Setup(x => x.GetByIdAsync(_adjustmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adjustment);

        _adjustmentRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<Adjustment>(), It.IsAny<CancellationToken>()))
            .Callback<Adjustment, CancellationToken>((adj, ct) => capturedAdjustment = adj)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(_adjustmentId);

        // Assert
        capturedAdjustment.Should().NotBeNull();
        capturedAdjustment!.IsDeleted.Should().BeTrue();
        capturedAdjustment.UpdatedAt.Should().NotBeNull();
    }

    private Adjustment CreateAdjustment()
    {
        return new Adjustment(
            Guid.NewGuid(),
            100m,
            AdjustmentType.Correction,
            "Justificativa válida para teste",
            DateTime.UtcNow.Date,
            Guid.NewGuid()
        );
    }
}
