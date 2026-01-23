using AutoMapper;
using FluentAssertions;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Auth;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Auth;

public class AuthenticationServiceTests
{
    private readonly Mock<IFirebaseAuthService> _firebaseAuthServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _firebaseAuthServiceMock = new Mock<IFirebaseAuthService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();

        // Configurar AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuthProfile>();
        });
        _mapper = config.CreateMapper();

        _sut = new AuthenticationService(
            _firebaseAuthServiceMock.Object,
            _userRepositoryMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidTokenAndVerifiedEmail_ShouldCreateNewUser()
    {
        // Arrange
        var request = new LoginRequest { FirebaseIdToken = "valid-token" };
        var firebaseUser = new FirebaseUserData
        {
            Uid = "firebase-uid-123",
            Email = "test@example.com",
            DisplayName = "Test User",
            EmailVerified = true
        };

        _firebaseAuthServiceMock
            .Setup(x => x.ValidateTokenAsync(request.FirebaseIdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firebaseUser);

        _userRepositoryMock
            .Setup(x => x.GetByFirebaseUidAsync(firebaseUser.Uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var newUser = new User(firebaseUser.Uid, firebaseUser.Email, firebaseUser.DisplayName, firebaseUser.EmailVerified);
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUser);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(firebaseUser.Email);
        result.User.DisplayName.Should().Be(firebaseUser.DisplayName);
        result.User.Roles.Should().Contain("Leitura");

        _firebaseAuthServiceMock.Verify(x => x.ValidateTokenAsync(request.FirebaseIdToken, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.GetByFirebaseUidAsync(firebaseUser.Uid, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithExistingUser_ShouldReturnExistingUser()
    {
        // Arrange
        var request = new LoginRequest { FirebaseIdToken = "valid-token" };
        var firebaseUser = new FirebaseUserData
        {
            Uid = "firebase-uid-123",
            Email = "test@example.com",
            DisplayName = "Test User",
            EmailVerified = true
        };

        var existingUser = new User(firebaseUser.Uid, firebaseUser.Email, firebaseUser.DisplayName, true);

        _firebaseAuthServiceMock
            .Setup(x => x.ValidateTokenAsync(request.FirebaseIdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firebaseUser);

        _userRepositoryMock
            .Setup(x => x.GetByFirebaseUidAsync(firebaseUser.Uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().Be(existingUser.Email);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithUnverifiedEmail_ShouldThrowAuthenticationException()
    {
        // Arrange
        var request = new LoginRequest { FirebaseIdToken = "valid-token" };
        var firebaseUser = new FirebaseUserData
        {
            Uid = "firebase-uid-123",
            Email = "test@example.com",
            DisplayName = "Test User",
            EmailVerified = false // Email NÃO verificado
        };

        _firebaseAuthServiceMock
            .Setup(x => x.ValidateTokenAsync(request.FirebaseIdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firebaseUser);

        // Act
        var act = async () => await _sut.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*Email não verificado*");

        _userRepositoryMock.Verify(x => x.GetByFirebaseUidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenFirebaseValidationFails_ShouldPropagateException()
    {
        // Arrange
        var request = new LoginRequest { FirebaseIdToken = "invalid-token" };

        _firebaseAuthServiceMock
            .Setup(x => x.ValidateTokenAsync(request.FirebaseIdToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthenticationException("AUTH_INVALID_TOKEN", "Token inválido"));

        // Act
        var act = async () => await _sut.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .Where(ex => ex.Code == "AUTH_INVALID_TOKEN");
    }

    [Fact]
    public async Task LoginAsync_WithUnverifiedExistingUser_ShouldUpdateEmailVerification()
    {
        // Arrange
        var request = new LoginRequest { FirebaseIdToken = "valid-token" };
        var firebaseUser = new FirebaseUserData
        {
            Uid = "firebase-uid-123",
            Email = "test@example.com",
            DisplayName = "Test User",
            EmailVerified = true
        };

        var existingUser = new User(firebaseUser.Uid, firebaseUser.Email, firebaseUser.DisplayName, false);

        _firebaseAuthServiceMock
            .Setup(x => x.ValidateTokenAsync(request.FirebaseIdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firebaseUser);

        _userRepositoryMock
            .Setup(x => x.GetByFirebaseUidAsync(firebaseUser.Uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.EmailVerified), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidUserId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetCurrentUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().Be(user.Email);
        result.User.DisplayName.Should().Be(user.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidUserId_ShouldThrowAuthenticationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.GetCurrentUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .Where(ex => ex.Code == "AUTH_USER_NOT_FOUND");
    }
}
