using FluentAssertions;
using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Exports;
using L2SLedger.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Exports;

public class GetExportsUseCaseTests
{
    private readonly Mock<IExportRepository> _mockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<GetExportsUseCase>> _mockLogger;
    private readonly GetExportsUseCase _useCase;

    public GetExportsUseCaseTests()
    {
        _mockRepository = new Mock<IExportRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockLogger = new Mock<ILogger<GetExportsUseCase>>();

        _useCase = new GetExportsUseCase(
            _mockRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithFilters_ReturnsFilteredExports()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetExportsRequest
        {
            Status = 1, // Pending
            Format = 1, // CSV
            Page = 1,
            PageSize = 10
        };

        var export1 = new Export("Transactions", ExportFormat.Csv, "{}", userId);
        var export2 = new Export("Transactions", ExportFormat.Csv, "{}", userId);
        var exports = new List<Export> { export1, export2 };

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByFiltersAsync(userId, 1, 1, 1, 10))
            .ReturnsAsync(exports);

        _mockRepository
            .Setup(r => r.CountByFiltersAsync(userId, 1, 1))
            .ReturnsAsync(2);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Exports.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutFilters_ReturnsAllUserExports()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetExportsRequest
        {
            Status = null,
            Format = null,
            Page = 1,
            PageSize = 10
        };

        var export1 = new Export("Transactions", ExportFormat.Csv, "{}", userId);
        var export2 = new Export("Transactions", ExportFormat.Pdf, "{}", userId);
        export2.MarkAsProcessing();

        var exports = new List<Export> { export1, export2 };

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByFiltersAsync(userId, null, null, 1, 10))
            .ReturnsAsync(exports);

        _mockRepository
            .Setup(r => r.CountByFiltersAsync(userId, null, null))
            .ReturnsAsync(2);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Exports.Should().HaveCount(2);
        result.Exports[0].Format.Should().Be("Csv");
        result.Exports[1].Format.Should().Be("Pdf");
        result.Exports[0].Status.Should().Be("Pending");
        result.Exports[1].Status.Should().Be("Processing");
    }

    [Fact]
    public async Task ExecuteAsync_AdminUser_CanSeeAllExports()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var request = new GetExportsRequest
        {
            Status = null,
            Format = null,
            Page = 1,
            PageSize = 10
        };

        var export1 = new Export("Transactions", ExportFormat.Csv, "{}", user1Id);
        var export2 = new Export("Transactions", ExportFormat.Pdf, "{}", user2Id);
        var exports = new List<Export> { export1, export2 };

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(adminUserId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(true);

        _mockRepository
            .Setup(r => r.GetByFiltersAsync(Guid.Empty, null, null, 1, 10))
            .ReturnsAsync(exports);

        _mockRepository
            .Setup(r => r.CountByFiltersAsync(Guid.Empty, null, null))
            .ReturnsAsync(2);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Exports.Should().HaveCount(2);
        result.Exports[0].RequestedByUserId.Should().Be(user1Id);
        result.Exports[1].RequestedByUserId.Should().Be(user2Id);
    }

    [Fact]
    public async Task ExecuteAsync_LogsQueryInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new GetExportsRequest
        {
            Status = null,
            Format = null,
            Page = 1,
            PageSize = 10
        };

        var exports = new List<Export>();

        _mockCurrentUserService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _mockCurrentUserService
            .Setup(s => s.IsInRole("Admin"))
            .Returns(false);

        _mockRepository
            .Setup(r => r.GetByFiltersAsync(userId, null, null, 1, 10))
            .ReturnsAsync(exports);

        _mockRepository
            .Setup(r => r.CountByFiltersAsync(userId, null, null))
            .ReturnsAsync(0);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"User {userId} retrieved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
