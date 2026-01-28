using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using L2SLedger.API.Controllers;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Audit;
using L2SLedger.Application.Validators.Audit;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;

namespace L2SLedger.API.Tests.Controllers;

/// <summary>
/// Testes para AuditController.
/// Conforme ADR-014 (Auditoria Financeira) e ADR-016 (RBAC).
/// </summary>
public class AuditControllerTests
{
    private readonly Mock<IAuditEventRepository> _auditEventRepositoryMock;
    private readonly Mock<ILogger<AuditController>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAuditEventsRequest> _validator;
    private readonly AuditController _sut;

    public AuditControllerTests()
    {
        _auditEventRepositoryMock = new Mock<IAuditEventRepository>();
        _loggerMock = new Mock<ILogger<AuditController>>();
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuditProfile>();
        });
        _mapper = config.CreateMapper();
        _validator = new GetAuditEventsRequestValidator();

        var getAuditEventsUseCase = new GetAuditEventsUseCase(
            _auditEventRepositoryMock.Object,
            _validator,
            _mapper);

        var getAuditEventByIdUseCase = new GetAuditEventByIdUseCase(
            _auditEventRepositoryMock.Object,
            _mapper);

        _sut = new AuditController(
            getAuditEventsUseCase,
            getAuditEventByIdUseCase,
            _loggerMock.Object);

        // Configurar HttpContext para evitar NullReferenceException ao acessar TraceIdentifier
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task GetEvents_WithValidRequest_ShouldReturnOk()
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
        var result = await _sut.GetEvents(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as GetAuditEventsResponse;
        response.Should().NotBeNull();
        response!.Events.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEvents_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new GetAuditEventsRequest { Page = 0, PageSize = 10 }; // Invalid page

        // Act
        var result = await _sut.GetEvents(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetEventById_WithExistingId_ShouldReturnOk()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Create,
            "Transaction",
            entityId,
            userId,
            "user@test.com",
            AuditSource.API);

        _auditEventRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(auditEvent);

        // Act
        var result = await _sut.GetEventById(eventId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var dto = okResult!.Value as AuditEventDto;
        dto.Should().NotBeNull();
        dto!.EntityType.Should().Be("Transaction");
    }

    [Fact]
    public async Task GetEventById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _auditEventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditEvent?)null);

        // Act
        var result = await _sut.GetEventById(eventId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAccessLogs_ShouldFilterByAccessEntityType()
    {
        // Arrange
        var events = new List<AuditEvent>
        {
            AuditEvent.CreateAccessEvent(
                AuditEventType.Login,
                Guid.NewGuid(),
                "user@test.com",
                "192.168.1.1",
                "Mozilla/5.0",
                "Success")
        };

        _auditEventRepositoryMock
            .Setup(x => x.GetByFiltersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AuditEventType?>(),
                "Access",
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((events, 1));

        // Act
        var result = await _sut.GetAccessLogs();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        _auditEventRepositoryMock.Verify(x => x.GetByFiltersAsync(
            1, 20,
            null,
            "Access",
            null, null, null, null, null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccessLogs_WithDateRange_ShouldPassDatesToRepository()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

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
        var result = await _sut.GetAccessLogs(startDate, endDate);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        _auditEventRepositoryMock.Verify(x => x.GetByFiltersAsync(
            1, 20,
            null,
            "Access",
            null, null,
            startDate, endDate,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
