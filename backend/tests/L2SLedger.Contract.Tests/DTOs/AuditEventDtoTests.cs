using FluentAssertions;
using L2SLedger.Application.DTOs.Audit;
using System.Text.Json;

namespace L2SLedger.Contract.Tests.DTOs;

/// <summary>
/// Testes de contrato para AuditEventDto.
/// Garante que a estrutura do DTO não mude sem intenção.
/// </summary>
public class AuditEventDtoTests
{
    [Fact]
    public void AuditEventDto_ShouldHaveAllRequiredProperties()
    {
        // Arrange
        var dto = new AuditEventDto();

        // Act & Assert - Verify all properties exist
        dto.Should().NotBeNull();
        dto.Id.Should().Be(Guid.Empty);
        dto.EventType.Should().Be(0);
        dto.EventTypeName.Should().BeEmpty();
        dto.EntityType.Should().BeEmpty();
        dto.EntityId.Should().BeNull();
        dto.Before.Should().BeNull();
        dto.After.Should().BeNull();
        dto.UserId.Should().BeNull();
        dto.UserEmail.Should().BeNull();
        dto.Timestamp.Should().Be(default);
        dto.Source.Should().Be(0);
        dto.SourceName.Should().BeEmpty();
        dto.IpAddress.Should().BeNull();
        dto.UserAgent.Should().BeNull();
        dto.Result.Should().BeEmpty();
        dto.Details.Should().BeNull();
        dto.TraceId.Should().BeNull();
    }

    [Fact]
    public void AuditEventDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var dto = new AuditEventDto
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            EventType = 1,
            EventTypeName = "Create",
            EntityType = "Transaction",
            EntityId = Guid.Parse("87654321-4321-4321-4321-210987654321"),
            Before = null,
            After = "{\"amount\":100}",
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserEmail = "user@test.com",
            Timestamp = new DateTime(2026, 1, 22, 10, 30, 0, DateTimeKind.Utc),
            Source = 2,
            SourceName = "API",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            Result = "Success",
            Details = null,
            TraceId = "trace-123"
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<AuditEventDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(dto.Id);
        deserialized.EventType.Should().Be(dto.EventType);
        deserialized.EventTypeName.Should().Be(dto.EventTypeName);
        deserialized.EntityType.Should().Be(dto.EntityType);
        deserialized.EntityId.Should().Be(dto.EntityId);
        deserialized.After.Should().Be(dto.After);
        deserialized.UserEmail.Should().Be(dto.UserEmail);
        deserialized.IpAddress.Should().Be(dto.IpAddress);
        deserialized.Result.Should().Be(dto.Result);
    }

    [Fact]
    public void GetAuditEventsRequest_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var request = new GetAuditEventsRequest();

        // Assert
        request.Page.Should().Be(1);
        request.PageSize.Should().Be(20);
        request.EventType.Should().BeNull();
        request.EntityType.Should().BeNull();
        request.EntityId.Should().BeNull();
        request.UserId.Should().BeNull();
        request.StartDate.Should().BeNull();
        request.EndDate.Should().BeNull();
        request.Result.Should().BeNull();
    }

    [Fact]
    public void GetAuditEventsResponse_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange & Act
        var response = new GetAuditEventsResponse
        {
            TotalCount = 55,
            PageSize = 10
        };

        // Assert
        response.TotalPages.Should().Be(6); // 55/10 = 5.5 -> ceil = 6
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(100, 20, 5)]
    [InlineData(101, 20, 6)]
    public void GetAuditEventsResponse_TotalPages_ShouldCalculateCorrectly(int totalCount, int pageSize, int expectedPages)
    {
        // Arrange & Act
        var response = new GetAuditEventsResponse
        {
            TotalCount = totalCount,
            PageSize = pageSize
        };

        // Assert
        response.TotalPages.Should().Be(expectedPages);
    }
}
