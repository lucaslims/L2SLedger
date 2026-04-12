using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Audit;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Audit;

/// <summary>
/// Testes para GetAuditEventByIdUseCase.
/// Conforme ADR-014 (Auditoria Financeira).
/// </summary>
public class GetAuditEventByIdUseCaseTests
{
    private readonly Mock<IAuditEventRepository> _auditEventRepositoryMock;
    private readonly IMapper _mapper;
    private readonly GetAuditEventByIdUseCase _sut;

    public GetAuditEventByIdUseCaseTests()
    {
        _auditEventRepositoryMock = new Mock<IAuditEventRepository>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuditProfile>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _sut = new GetAuditEventByIdUseCase(
            _auditEventRepositoryMock.Object,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ShouldReturnAuditEvent()
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
            AuditSource.API,
            before: null,
            after: "{\"amount\":100}");

        _auditEventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auditEvent);

        // Act
        var result = await _sut.ExecuteAsync(eventId);

        // Assert
        result.Should().NotBeNull();
        result.EntityType.Should().Be("Transaction");
        result.EventTypeName.Should().Be("Create");
        result.UserEmail.Should().Be("user@test.com");
        result.After.Should().Be("{\"amount\":100}");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _auditEventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditEvent?)null);

        // Act
        var act = async () => await _sut.ExecuteAsync(eventId);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessRuleException>();
        exception.Which.Code.Should().Be("AUDIT_EVENT_NOT_FOUND");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.Login,
            userId,
            "admin@test.com",
            "192.168.1.1",
            "Mozilla/5.0",
            "Success");

        _auditEventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auditEvent);

        // Act
        var result = await _sut.ExecuteAsync(eventId);

        // Assert
        result.Should().NotBeNull();
        result.EventType.Should().Be((int)AuditEventType.Login);
        result.EventTypeName.Should().Be("Login");
        result.EntityType.Should().Be("Access");
        result.UserId.Should().Be(userId);
        result.UserEmail.Should().Be("admin@test.com");
        result.IpAddress.Should().Be("192.168.1.1");
        result.UserAgent.Should().Be("Mozilla/5.0");
        result.Result.Should().Be("Success");
        result.Source.Should().Be((int)AuditSource.API);
        result.SourceName.Should().Be("API");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Delete,
            "Category",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "user@test.com",
            AuditSource.UI);

        _auditEventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auditEvent);

        // Act
        await _sut.ExecuteAsync(eventId);

        // Assert
        _auditEventRepositoryMock.Verify(
            x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
