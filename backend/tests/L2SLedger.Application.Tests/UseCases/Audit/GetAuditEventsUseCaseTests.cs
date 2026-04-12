using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Audit;
using L2SLedger.Domain.Entities;
using Moq;
using FluentValidationException = FluentValidation.ValidationException;

namespace L2SLedger.Application.Tests.UseCases.Audit;

/// <summary>
/// Testes para GetAuditEventsUseCase.
/// Conforme ADR-014 (Auditoria Financeira).
/// </summary>
public class GetAuditEventsUseCaseTests
{
    private readonly Mock<IAuditEventRepository> _auditEventRepositoryMock;
    private readonly Mock<IValidator<GetAuditEventsRequest>> _validatorMock;
    private readonly IMapper _mapper;
    private readonly GetAuditEventsUseCase _sut;

    public GetAuditEventsUseCaseTests()
    {
        _auditEventRepositoryMock = new Mock<IAuditEventRepository>();
        _validatorMock = new Mock<IValidator<GetAuditEventsRequest>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuditProfile>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _sut = new GetAuditEventsUseCase(
            _auditEventRepositoryMock.Object,
            _validatorMock.Object,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReturnPaginatedEvents()
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 1,
            PageSize = 10
        };

        var userId = Guid.NewGuid();
        var events = new List<AuditEvent>
        {
            AuditEvent.CreateEntityEvent(
                AuditEventType.Create,
                "Transaction",
                Guid.NewGuid(),
                userId,
                "user@test.com",
                AuditSource.API),
            AuditEvent.CreateEntityEvent(
                AuditEventType.Update,
                "Transaction",
                Guid.NewGuid(),
                userId,
                "user@test.com",
                AuditSource.API)
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditEventRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AuditEventType?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((events, 2));

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ExecuteAsync_WithEventTypeFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            EventType = (int)AuditEventType.Create,
            Page = 1,
            PageSize = 10
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditEventRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                AuditEventType.Create,
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AuditEvent>(), 0));

        // Act
        await _sut.ExecuteAsync(request);

        // Assert
        _auditEventRepositoryMock.Verify(x => x.GetByFiltersAsync(
            1, 10,
            AuditEventType.Create,
            null, null, null, null, null, null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEntityTypeFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            EntityType = "Transaction",
            Page = 1,
            PageSize = 10
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditEventRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AuditEventType?>(),
                "Transaction",
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AuditEvent>(), 0));

        // Act
        await _sut.ExecuteAsync(request);

        // Assert
        _auditEventRepositoryMock.Verify(x => x.GetByFiltersAsync(
            1, 10,
            null,
            "Transaction",
            null, null, null, null, null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDateRangeFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

        var request = new GetAuditEventsRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            Page = 1,
            PageSize = 10
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditEventRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AuditEventType?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                startDate,
                endDate,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AuditEvent>(), 0));

        // Act
        await _sut.ExecuteAsync(request);

        // Assert
        _auditEventRepositoryMock.Verify(x => x.GetByFiltersAsync(
            1, 10,
            null, null, null, null,
            startDate, endDate,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var request = new GetAuditEventsRequest
        {
            Page = 0, // Invalid
            PageSize = 10
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Page", "Page deve ser maior que zero")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var act = async () => await _sut.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<FluentValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapEventTypeToName()
    {
        // Arrange
        var request = new GetAuditEventsRequest { Page = 1, PageSize = 10 };

        var events = new List<AuditEvent>
        {
            AuditEvent.CreateEntityEvent(
                AuditEventType.Create,
                "Transaction",
                Guid.NewGuid(),
                Guid.NewGuid(),
                "user@test.com",
                AuditSource.API)
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditEventRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AuditEventType?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((events, 1));

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Events.First().EventTypeName.Should().Be("Create");
        result.Events.First().SourceName.Should().Be("API");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new GetAuditEventsRequest { Page = 1, PageSize = 10 };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditEventRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AuditEventType?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AuditEvent>(), 0));

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.Events.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var request = new GetAuditEventsRequest { Page = 1, PageSize = 10 };

        _validatorMock
            .Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _auditEventRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AuditEventType?>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AuditEvent>(), 25));

        // Act
        var result = await _sut.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3); // 25/10 = 2.5 -> ceil = 3
    }
}
