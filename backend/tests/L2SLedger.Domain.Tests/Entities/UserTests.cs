using FluentAssertions;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateUserWithDefaultRole()
    {
        // Arrange & Act
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Assert
        user.FirebaseUid.Should().Be("firebase-uid");
        user.Email.Should().Be("test@example.com");
        user.DisplayName.Should().Be("Test User");
        user.EmailVerified.Should().BeTrue();
        user.Roles.Should().Contain("Leitura");
        user.Roles.Should().HaveCount(1);
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddRole_ShouldAddNewRole()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        user.AddRole("Financeiro");

        // Assert
        user.Roles.Should().Contain("Financeiro");
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().Contain(new[] { "Leitura", "Financeiro" });
    }

    [Fact]
    public void AddRole_WithDuplicateRole_ShouldNotAddDuplicate()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.AddRole("Financeiro");

        // Act
        user.AddRole("Financeiro");

        // Assert
        user.Roles.Should().HaveCount(2); // Leitura + Financeiro (sem duplicata)
        user.Roles.Count(r => r == "Financeiro").Should().Be(1);
    }

    [Fact]
    public void RemoveRole_ShouldRemoveExistingRole()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.AddRole("Financeiro");
        user.AddRole("Admin");

        // Act
        user.RemoveRole("Financeiro");

        // Assert
        user.Roles.Should().NotContain("Financeiro");
        user.Roles.Should().Contain(new[] { "Leitura", "Admin" });
        user.Roles.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveRole_WithNonExistentRole_ShouldDoNothing()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        user.RemoveRole("NonExistent");

        // Assert
        user.Roles.Should().HaveCount(1);
        user.Roles.Should().Contain("Leitura");
    }

    [Fact]
    public void UpdateDisplayName_ShouldUpdateName()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Old Name", true);

        // Act
        user.UpdateDisplayName("New Name");

        // Assert
        user.DisplayName.Should().Be("New Name");
    }

    [Fact]
    public void VerifyEmail_ShouldSetEmailVerifiedToTrue()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", false);

        // Act
        user.VerifyEmail();

        // Assert
        user.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithExistingRole_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.AddRole("Admin");

        // Act & Assert
        user.HasRole("Admin").Should().BeTrue();
        user.HasRole("Leitura").Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithNonExistentRole_ShouldReturnFalse()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act & Assert
        user.HasRole("Admin").Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_WithAdminRole_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);
        user.AddRole("Admin");

        // Act & Assert
        user.IsAdmin().Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_WithoutAdminRole_ShouldReturnFalse()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act & Assert
        user.IsAdmin().Should().BeFalse();
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var user = new User("firebase-uid", "test@example.com", "Test User", true);

        // Act
        user.MarkAsDeleted();

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
