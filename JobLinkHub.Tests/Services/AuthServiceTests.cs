using FluentAssertions;
using JobLinkHub.API.Services;
using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace JobLinkHub.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _emailServiceMock = new Mock<IEmailService>();

        var configData = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "JobLinkHub-Super-Secret-Key-AtLeast-32-Chars!!",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:ExpirationMinutes"] = "60",
            ["Email:FrontendBaseUrl"] = "http://localhost:3000"
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _context = TestDbContextFactory.Create();

        _service = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _config,
            _context,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task RegisterCandidateAsync_NewUser_ReturnsAuthResponse()
    {
        var dto = new RegisterCandidateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Password = "Password1!"
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<User, string>((u, _) => u.Id = 1);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "CANDIDATE"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "CANDIDATE" });
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("confirm-token");

        var result = await _service.RegisterCandidateAsync(dto);

        result.Should().NotBeNull();
        result.Email.Should().Be("john@test.com");
        result.Role.Should().Be("CANDIDATE");
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterCandidateAsync_DuplicateEmail_ThrowsException()
    {
        var dto = new RegisterCandidateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@test.com",
            Password = "Password1!"
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new User { Email = dto.Email });

        var act = () => _service.RegisterCandidateAsync(dto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Email already registered");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        var user = new User
        {
            Id = 1,
            Email = "john@test.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "CANDIDATE",
            IsActive = true,
            EmailConfirmed = true
        };

        _context.JobSeekerProfiles.Add(new JobSeekerProfile
        {
            Id = 1,
            UserId = 1
        });
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com"))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, "Password1!", true))
            .ReturnsAsync(SignInResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "CANDIDATE" });

        var dto = new LoginDto { Email = "john@test.com", Password = "Password1!" };
        var result = await _service.LoginAsync(dto, "127.0.0.1");

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsException()
    {
        var user = new User
        {
            Id = 1,
            Email = "john@test.com",
            IsActive = true,
            EmailConfirmed = true
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com"))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, "wrong", true))
            .ReturnsAsync(SignInResult.Failed);

        var dto = new LoginDto { Email = "john@test.com", Password = "wrong" };
        var act = () => _service.LoginAsync(dto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_DeactivatedUser_ThrowsException()
    {
        var user = new User
        {
            Id = 1,
            Email = "john@test.com",
            IsActive = false
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com"))
            .ReturnsAsync(user);

        var dto = new LoginDto { Email = "john@test.com", Password = "any" };
        var act = () => _service.LoginAsync(dto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Account is deactivated");
    }

    [Fact]
    public async Task LoginAsync_UnverifiedEmail_ThrowsException()
    {
        var user = new User
        {
            Id = 1,
            Email = "john@test.com",
            IsActive = true,
            EmailConfirmed = false
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com"))
            .ReturnsAsync(user);

        var dto = new LoginDto { Email = "john@test.com", Password = "any" };
        var act = () => _service.LoginAsync(dto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Email not verified*");
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        var user = new User
        {
            Id = 1,
            Email = "john@test.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "CANDIDATE",
            IsActive = true
        };

        var refreshToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            User = user
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "CANDIDATE" });

        var result = await _service.RefreshTokenAsync("valid-refresh-token", "127.0.0.1");

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe("valid-refresh-token");
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_ThrowsException()
    {
        var user = new User
        {
            Id = 2,
            Email = "expired@test.com",
            FirstName = "Test",
            LastName = "User",
            Role = "CANDIDATE"
        };

        var refreshToken = new RefreshToken
        {
            Id = 2,
            UserId = 2,
            Token = "expired-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            User = user
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var act = () => _service.RefreshTokenAsync("expired-refresh-token", "127.0.0.1");

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Refresh token has expired");
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedToken_ThrowsException()
    {
        var user = new User
        {
            Id = 3,
            Email = "revoked@test.com",
            FirstName = "Test",
            LastName = "User",
            Role = "CANDIDATE"
        };

        var refreshToken = new RefreshToken
        {
            Id = 3,
            UserId = 3,
            Token = "revoked-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = DateTime.UtcNow,
            User = user
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var act = () => _service.RefreshTokenAsync("revoked-refresh-token", "127.0.0.1");

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Refresh token has been revoked");
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidPassword_ReturnsTrue()
    {
        var user = new User { Id = 1, Email = "john@test.com" };
        _userManagerMock.Setup(m => m.FindByIdAsync("1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ChangePasswordAsync(user, "OldPass1!", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);

        var dto = new ChangePasswordDto { CurrentPassword = "OldPass1!", NewPassword = "NewPass1!" };
        var result = await _service.ChangePasswordAsync(1, dto);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPasswordAsync_ExistingEmail_ReturnsTrue()
    {
        var user = new User
        {
            Id = 1,
            Email = "john@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        var result = await _service.ForgotPasswordAsync("john@test.com");

        result.Should().BeTrue();
        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(
            "john@test.com",
            "John Doe",
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ReturnsTrue()
    {
        var user = new User { Id = 1, Email = "john@test.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, It.IsAny<string>(), "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);

        var dto = new ResetPasswordDto
        {
            Email = "john@test.com",
            Token = "reset-token",
            NewPassword = "NewPass1!"
        };

        var result = await _service.ResetPasswordAsync(dto);

        result.Should().BeTrue();
    }
}
