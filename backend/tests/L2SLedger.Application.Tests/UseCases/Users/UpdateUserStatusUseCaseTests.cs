using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Users;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Users;

public class UpdateUserStatusUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<UpdateUserStatusUseCase>> _loggerMock;
    private readonly UpdateUserStatusUseCase _sut;

    public UpdateUserStatusUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<UpdateUserStatusUseCase>>();

        // Configurar AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _currentUserServiceMock
            .Setup(x => x.GetUserId())
            .Returns(Guid.NewGuid());

        _sut = new UpdateUserStatusUseCase(
            _userRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _auditServiceMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ApprovePendingUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        var request = new UpdateUserStatusRequest
        {
            Status = "Active",
            Reason = "Cadastro aprovado após verificação"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditServiceMock
            .Setup(x => x.LogUpdateAsync(It.IsAny<User>(), It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Active");
        user.Status.Should().Be(UserStatus.Active);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(x => x.LogUpdateAsync(It.IsAny<User>(), It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SuspendActiveUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve(); // Status = Active

        var request = new UpdateUserStatusRequest
        {
            Status = "Suspended",
            Reason = "Atividade suspeita detectada"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditServiceMock
            .Setup(x => x.LogUpdateAsync(It.IsAny<User>(), It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Suspended");
        user.Status.Should().Be(UserStatus.Suspended);
    }

    [Fact]
    public async Task ExecuteAsync_ReactivateSuspendedUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve();
        user.Suspend(); // Status = Suspended

        var request = new UpdateUserStatusRequest
        {
            Status = "Active",
            Reason = "Situação regularizada"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditServiceMock
            .Setup(x => x.LogUpdateAsync(It.IsAny<User>(), It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Active");
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task ExecuteAsync_RejectPendingUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        // Status = Pending by default

        var request = new UpdateUserStatusRequest
        {
            Status = "Rejected",
            Reason = "Documentação insuficiente"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditServiceMock
            .Setup(x => x.LogUpdateAsync(It.IsAny<User>(), It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Rejected");
        user.Status.Should().Be(UserStatus.Rejected);
    }

    [Fact]
    public async Task ExecuteAsync_WithUserNotFound_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserStatusRequest
        {
            Status = "Active",
            Reason = "Test reason"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(ex => ex.Code == "USER_NOT_FOUND");
    }

    [Fact]
    public async Task ExecuteAsync_WithoutReason_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        var request = new UpdateUserStatusRequest
        {
            Status = "Active",
            Reason = "" // Empty reason
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _sut.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(ex => ex.Code == "USER_STATUS_REASON_REQUIRED");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidStatus_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        var request = new UpdateUserStatusRequest
        {
            Status = "InvalidStatus",
            Reason = "Test reason"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _sut.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(ex => ex.Code == "USER_INVALID_STATUS");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidTransition_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.Approve(); // Status = Active

        var request = new UpdateUserStatusRequest
        {
            Status = "Rejected", // Cannot reject Active user
            Reason = "Test reason"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _sut.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(ex => ex.Code == "USER_INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task ExecuteAsync_SuspendPendingUser_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        // Status = Pending by default

        var request = new UpdateUserStatusRequest
        {
            Status = "Suspended", // Cannot suspend Pending user
            Reason = "Test reason"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _sut.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(ex => ex.Code == "USER_INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task ExecuteAsync_ReasonTooLong_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        var request = new UpdateUserStatusRequest
        {
            Status = "Active",
            Reason = new string('x', 2001) // Exceeds 2000 characters
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _sut.ExecuteAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(ex => ex.Code == "USER_STATUS_REASON_TOO_LONG");
    }
}
