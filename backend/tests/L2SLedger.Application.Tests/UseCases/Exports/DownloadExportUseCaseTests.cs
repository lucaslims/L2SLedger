using FluentAssertions;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Exports;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Exports;

public class DownloadExportUseCaseTests
{
    private readonly Mock<IExportRepository> _mockRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<DownloadExportUseCase>> _mockLogger;
    private readonly DownloadExportUseCase _useCase;

    public DownloadExportUseCaseTests()
    {
        _mockRepository = new Mock<IExportRepository>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<DownloadExportUseCase>>();

        _useCase = new DownloadExportUseCase(
            _mockRepository.Object,
            _mockFileStorageService.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CompletedExport_ReturnsFileBytes()
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
        export.MarkAsProcessing();
        export.MarkAsCompleted("exports/test.csv", 1024, 100);

        var fileBytes = new byte[] { 1, 2, 3, 4, 5 };

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync(export);

        _mockFileStorageService
            .Setup(s => s.ReadExportFileAsync("exports/test.csv"))
            .ReturnsAsync(fileBytes);

        // Act
        var result = await _useCase.ExecuteAsync(exportId);

        // Assert
        result.FileBytes.Should().BeEquivalentTo(fileBytes);
        result.FileName.Should().Contain("transactions_");
        result.FileName.Should().EndWith(".csv");
        result.ContentType.Should().Be("text/csv");

        _mockFileStorageService.Verify(s => s.ReadExportFileAsync("exports/test.csv"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PendingExport_ThrowsBusinessRuleException()
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
        Func<Task> act = async () => await _useCase.ExecuteAsync(exportId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Export is not ready for download. Current status: Pending");
    }

    [Fact]
    public async Task ExecuteAsync_MissingFile_ThrowsException()
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
        export.MarkAsProcessing();
        export.MarkAsCompleted("exports/test.csv", 1024, 100);

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync(export);

        _mockFileStorageService
            .Setup(s => s.ReadExportFileAsync("exports/test.csv"))
            .ThrowsAsync(new FileNotFoundException("File not found"));

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(exportId);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
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
        export.MarkAsProcessing();
        export.MarkAsCompleted("exports/test.csv", 1024, 100);

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
            .WithMessage("You are not authorized to download this export.");
    }

    [Fact]
    public async Task ExecuteAsync_PdfExport_ReturnsCorrectContentType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exportId = Guid.NewGuid();
        var export = new Export(
            exportType: "Transactions",
            format: ExportFormat.Pdf,
            parametersJson: "{}",
            requestedByUserId: userId
        );
        export.MarkAsProcessing();
        export.MarkAsCompleted("exports/test.pdf", 2048, 100);

        var fileBytes = new byte[] { 1, 2, 3, 4, 5 };

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync(export);

        _mockFileStorageService
            .Setup(s => s.ReadExportFileAsync("exports/test.pdf"))
            .ReturnsAsync(fileBytes);

        // Act
        var result = await _useCase.ExecuteAsync(exportId);

        // Assert
        result.FileName.Should().EndWith(".pdf");
        result.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task ExecuteAsync_LogsDownloadOperation()
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
        export.MarkAsProcessing();
        export.MarkAsCompleted("exports/test.csv", 1024, 100);

        var fileBytes = new byte[] { 1, 2, 3, 4, 5 };

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByIdAsync(exportId))
            .ReturnsAsync(export);

        _mockFileStorageService
            .Setup(s => s.ReadExportFileAsync("exports/test.csv"))
            .ReturnsAsync(fileBytes);

        // Act
        await _useCase.ExecuteAsync(exportId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Exportação {exportId} baixada pelo usuário {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
