using FluentAssertions;
using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Exports;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Exports;

public class RequestExportUseCaseTests
{
    private readonly Mock<IExportRepository> _mockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<RequestExportUseCase>> _mockLogger;
    private readonly RequestExportUseCase _useCase;

    public RequestExportUseCaseTests()
    {
        _mockRepository = new Mock<IExportRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<RequestExportUseCase>>();

        _useCase = new RequestExportUseCase(
            _mockRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_CreatesExportWithPendingStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RequestExportRequest
        {
            Format = 1, // CSV
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            CategoryId = null,
            TransactionType = null
        };

        var createdExport = new Export(
            exportType: "Transactions",
            format: ExportFormat.Csv,
            parametersJson: "{\"StartDate\":\"2026-01-01T00:00:00\",\"EndDate\":\"2026-01-31T00:00:00\",\"CategoryId\":null,\"TransactionType\":null}",
            requestedByUserId: userId
        );

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Export>()))
            .ReturnsAsync(createdExport);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdExport.Id);
        result.ExportType.Should().Be("Transactions");
        result.Format.Should().Be("Csv");
        result.Status.Should().Be("Pending");
        result.RequestedByUserId.Should().Be(userId);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Export>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryFilter_CreatesExportWithParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new RequestExportRequest
        {
            Format = 2, // PDF
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            CategoryId = categoryId,
            TransactionType = 1
        };

        var createdExport = new Export(
            exportType: "Transactions",
            format: ExportFormat.Pdf,
            parametersJson: $"{{\"StartDate\":\"2026-01-01T00:00:00\",\"EndDate\":\"2026-01-31T00:00:00\",\"CategoryId\":\"{categoryId}\",\"TransactionType\":\"Expense\"}}",
            requestedByUserId: userId
        );

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Export>()))
            .ReturnsAsync(createdExport);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Format.Should().Be("Pdf");
        result.ParametersJson.Should().Contain(categoryId.ToString());
        result.ParametersJson.Should().Contain("Expense");

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Export>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsExportCreation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RequestExportRequest
        {
            Format = 1,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            CategoryId = null,
            TransactionType = null
        };

        var createdExport = new Export(
            exportType: "Transactions",
            format: ExportFormat.Csv,
            parametersJson: "{}",
            requestedByUserId: userId
        );

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Export>()))
            .ReturnsAsync(createdExport);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Exportação {createdExport.Id} solicitada pelo usuário {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAllRequiredDtoFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RequestExportRequest
        {
            Format = 1,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 31),
            CategoryId = null,
            TransactionType = null
        };

        var createdExport = new Export(
            exportType: "Transactions",
            format: ExportFormat.Csv,
            parametersJson: "{}",
            requestedByUserId: userId
        );

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Export>()))
            .ReturnsAsync(createdExport);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.ExportType.Should().NotBeNullOrEmpty();
        result.Format.Should().NotBeNullOrEmpty();
        result.Status.Should().NotBeNullOrEmpty();
        result.RequestedByUserId.Should().NotBeEmpty();
        result.RequestedAt.Should().BeAfter(DateTime.MinValue);
        result.CreatedAt.Should().BeAfter(DateTime.MinValue);
    }
}
