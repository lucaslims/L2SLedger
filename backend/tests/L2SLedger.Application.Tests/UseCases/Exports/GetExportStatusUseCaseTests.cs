using FluentAssertions;
using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Exports;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Exports;

public class GetExportStatusUseCaseTests
{
    private readonly Mock<IExportRepository> _mockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<GetExportStatusUseCase>> _mockLogger;
    private readonly GetExportStatusUseCase _useCase;

    public GetExportStatusUseCaseTests()
    {
        _mockRepository = new Mock<IExportRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<GetExportStatusUseCase>>();

        _useCase = new GetExportStatusUseCase(
            _mockRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidId_ReturnsExportStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exportId = Guid.NewGuid();
        var export = new Export(
            exportType: "Transactions",
            format: ExportFormat.Csv,
            parametersJson: "{}",
            requestedByUserId: userId
        );

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync(export);

        // Act
        var result = await _useCase.ExecuteAsync(exportId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(export.Id);
        result.Status.Should().Be("Pending");
        result.ProgressPercentage.Should().Be(0);
        result.IsDownloadable.Should().BeFalse();
        result.RequestedAt.Should().Be(export.RequestedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exportId = Guid.NewGuid();

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync((Export?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(exportId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Export with ID {exportId} not found.");
    }

    [Fact]
    public async Task ExecuteAsync_UnauthorizedUser_ThrowsAuthorizationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var exportId = Guid.NewGuid();
        var export = new Export(
            exportType: "Transactions",
            format: ExportFormat.Csv,
            parametersJson: "{}",
            requestedByUserId: otherUserId
        );

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync(export);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(exportId);

        // Assert
        await act.Should().ThrowAsync<AuthorizationException>()
            .WithMessage("You are not authorized to view this export.");
    }

    [Theory]
    [InlineData(ExportStatus.Pending, 0)]
    [InlineData(ExportStatus.Processing, 50)]
    [InlineData(ExportStatus.Completed, 100)]
    [InlineData(ExportStatus.Failed, 100)]
    public async Task ExecuteAsync_DifferentStatuses_ReturnsCorrectProgressPercentage(
        ExportStatus status, int expectedProgress)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exportId = Guid.NewGuid();
        var export = new Export(
            exportType: "Transactions",
            format: ExportFormat.Csv,
            parametersJson: "{}",
            requestedByUserId: userId
        );

        // Simulate status progression
        if (status == ExportStatus.Processing || status == ExportStatus.Completed || status == ExportStatus.Failed)
        {
            export.MarkAsProcessing();
        }

        if (status == ExportStatus.Completed)
        {
            export.MarkAsCompleted("test-file.csv", 1024, 100);
        }

        if (status == ExportStatus.Failed)
        {
            export.MarkAsFailed("Test error");
        }

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync(export);

        // Act
        var result = await _useCase.ExecuteAsync(exportId);

        // Assert
        result.ProgressPercentage.Should().Be(expectedProgress);
        result.Status.Should().Be(status.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_AdminUser_CanAccessOtherUsersExports()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var exportId = Guid.NewGuid();
        var export = new Export(
            exportType: "Transactions",
            format: ExportFormat.Csv,
            parametersJson: "{}",
            requestedByUserId: otherUserId
        );

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(adminUserId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(true);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync(export);

        // Act
        var result = await _useCase.ExecuteAsync(exportId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(export.Id);
        // Admin can access other users' exports without exception
    }
}
