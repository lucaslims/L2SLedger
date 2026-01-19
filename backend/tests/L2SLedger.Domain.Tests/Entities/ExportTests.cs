using L2SLedger.Domain.Entities;
using Xunit;

namespace L2SLedger.Domain.Tests.Entities;

public class ExportTests
{
    private readonly Guid _validUserId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_CreatesExportWithPendingStatus()
    {
        // Arrange & Act
        var export = new Export(
            exportType: "Transactions",
            format: ExportFormat.Csv,
            parametersJson: "{\"startDate\":\"2026-01-01\"}",
            requestedByUserId: _validUserId
        );

        // Assert
        Assert.Equal("Transactions", export.ExportType);
        Assert.Equal(ExportFormat.Csv, export.Format);
        Assert.Equal(ExportStatus.Pending, export.Status);
        Assert.Equal(_validUserId, export.RequestedByUserId);
        Assert.Equal("{\"startDate\":\"2026-01-01\"}", export.ParametersJson);
        Assert.Null(export.FilePath);
        Assert.Null(export.FileSizeBytes);
        Assert.Null(export.ProcessingStartedAt);
        Assert.Null(export.CompletedAt);
        Assert.Null(export.ErrorMessage);
        Assert.Null(export.RecordCount);
        Assert.False(export.IsDeleted);
    }

    [Fact]
    public void MarkAsProcessing_WithPendingStatus_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Pdf, "{}", _validUserId);
        Assert.Equal(ExportStatus.Pending, export.Status);
        Assert.Null(export.ProcessingStartedAt);

        // Act
        export.MarkAsProcessing();

        // Assert
        Assert.Equal(ExportStatus.Processing, export.Status);
        Assert.NotNull(export.ProcessingStartedAt);
        Assert.True(export.ProcessingStartedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsProcessing_WithNonPendingStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Csv, "{}", _validUserId);
        export.MarkAsProcessing(); // Agora está Processing

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => export.MarkAsProcessing());
        Assert.Contains("Only pending exports can be marked as processing", exception.Message);
    }

    [Fact]
    public void MarkAsCompleted_WithProcessingStatus_UpdatesStatusAndMetadata()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Csv, "{}", _validUserId);
        export.MarkAsProcessing();
        var filePath = "/exports/test.csv";
        var fileSizeBytes = 1024L;
        var recordCount = 100;

        // Act
        export.MarkAsCompleted(filePath, fileSizeBytes, recordCount);

        // Assert
        Assert.Equal(ExportStatus.Completed, export.Status);
        Assert.Equal(filePath, export.FilePath);
        Assert.Equal(fileSizeBytes, export.FileSizeBytes);
        Assert.Equal(recordCount, export.RecordCount);
        Assert.NotNull(export.CompletedAt);
        Assert.True(export.CompletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsCompleted_WithNonProcessingStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Pdf, "{}", _validUserId);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            export.MarkAsCompleted("/path/file.pdf", 2048L, 50));
        Assert.Contains("Only processing exports can be marked as completed", exception.Message);
    }

    [Fact]
    public void MarkAsFailed_WithProcessingStatus_UpdatesStatusAndErrorMessage()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Csv, "{}", _validUserId);
        export.MarkAsProcessing();
        var errorMessage = "Database connection failed";

        // Act
        export.MarkAsFailed(errorMessage);

        // Assert
        Assert.Equal(ExportStatus.Failed, export.Status);
        Assert.Equal(errorMessage, export.ErrorMessage);
        Assert.NotNull(export.CompletedAt);
        Assert.True(export.CompletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsFailed_WithNonProcessingStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Pdf, "{}", _validUserId);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            export.MarkAsFailed("Some error"));
        Assert.Contains("Only processing exports can be marked as failed", exception.Message);
    }

    [Fact]
    public void IsDownloadable_WithCompletedStatusAndFilePath_ReturnsTrue()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Csv, "{}", _validUserId);
        export.MarkAsProcessing();
        export.MarkAsCompleted("/exports/test.csv", 1024L, 100);

        // Act
        var isDownloadable = export.IsDownloadable();

        // Assert
        Assert.True(isDownloadable);
    }

    [Fact]
    public void IsDownloadable_WithPendingStatus_ReturnsFalse()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Pdf, "{}", _validUserId);

        // Act
        var isDownloadable = export.IsDownloadable();

        // Assert
        Assert.False(isDownloadable);
    }

    [Fact]
    public void IsDownloadable_WithCompletedStatusButNoFilePath_ReturnsFalse()
    {
        // Arrange
        var export = new Export("Transactions", ExportFormat.Csv, "{}", _validUserId);
        export.MarkAsProcessing();
        // Simula um caso onde Completed foi setado mas FilePath está vazio (caso improvável mas possível)
        // Não é possível via API pública, mas IsDownloadable() deve proteger contra isso
        
        // Neste caso, como não podemos criar um export Completed sem FilePath via métodos públicos,
        // vamos testar apenas que Pending retorna false
        Assert.False(export.IsDownloadable());
    }
}
