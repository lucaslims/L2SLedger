using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Exports;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Exports;

public class GetExportByIdUseCaseTests
{
    private readonly Mock<IExportRepository> _mockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetExportByIdUseCase>> _mockLogger;
    private readonly GetExportByIdUseCase _useCase;

    public GetExportByIdUseCaseTests()
    {
        _mockRepository = new Mock<IExportRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<GetExportByIdUseCase>>();

        _useCase = new GetExportByIdUseCase(
            _mockRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidId_ReturnsExportDto()
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
        result.ExportType.Should().Be("Transactions");
        result.Format.Should().Be("Csv");
        result.Status.Should().Be("Pending");
        result.RequestedByUserId.Should().Be(userId);
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
}
