using System.Net;
using Concertable.Application.Requests;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.Auth;

[Collection("Integration")]
public class AuthApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public AuthApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region Register

    [Fact]
    public async Task Register_ShouldReturn204_WhenValidRequest()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Auth/register", new RegisterRequest
        {
            Email = "newuser@test.com",
            Password = "Password123!",
            Role = Role.Customer
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenEmailAlreadyExists()
    {
        // Arrange
        var client = fixture.CreateClient();

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "Password123!",
            Role = Role.Customer
        });

        // Act
        var response = await client.PostAsync("/api/Auth/register", new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "Password123!",
            Role = Role.Customer
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login

    [Fact]
    public async Task Login_ShouldReturn400_WhenPasswordIsWrong()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Auth/login", new LoginRequest
        {
            Email = "venuemanager1@test.com",
            Password = "wrongpassword"
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenEmailNotVerified()
    {
        // Arrange
        var client = fixture.CreateClient();

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = "unverified@test.com",
            Password = "Password123!",
            Role = Role.Customer
        });

        // Act
        var response = await client.PostAsync("/api/Auth/login", new LoginRequest
        {
            Email = "unverified@test.com",
            Password = "Password123!"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturn200_WhenEmailVerified()
    {
        // Arrange
        var client = fixture.CreateClient();
        const string email = "verified@test.com";
        const string password = "Password123!";

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            Role = Role.Customer
        });

        var token = fixture.EmailService.ExtractToken(email);
        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = token! });

        // Act
        var response = await client.PostAsync("/api/Auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.NotNull(loginResponse.AccessToken);
        Assert.NotNull(loginResponse.RefreshToken);
    }

    #endregion

    #region SendVerification

    [Fact]
    public async Task SendVerification_ShouldReturn204_WhenUserExists()
    {
        // Arrange
        var client = fixture.CreateClient();

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = "resend@test.com",
            Password = "Password123!",
            Role = Role.Customer
        });

        fixture.EmailService.Reset();

        // Act
        var response = await client.PostAsync("/api/Auth/send-verification", new ForgotPasswordRequest
        {
            Email = "resend@test.com"
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(fixture.EmailService.ExtractToken("resend@test.com"));
    }

    [Fact]
    public async Task SendVerification_ShouldReturn204_WhenUserDoesNotExist()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act — should not reveal whether user exists
        var response = await client.PostAsync("/api/Auth/send-verification", new ForgotPasswordRequest
        {
            Email = "doesnotexist@test.com"
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region VerifyEmail

    [Fact]
    public async Task VerifyEmail_ShouldReturn204_WhenTokenIsValid()
    {
        // Arrange
        var client = fixture.CreateClient();

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = "toverify@test.com",
            Password = "Password123!",
            Role = Role.Customer
        });

        var token = fixture.EmailService.ExtractToken("toverify@test.com");

        // Act
        var response = await client.PostAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = token! });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturn400_WhenTokenIsInvalid()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = "invalid-token" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturn400_WhenTokenAlreadyUsed()
    {
        // Arrange
        var client = fixture.CreateClient();

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = "alreadyverified@test.com",
            Password = "Password123!",
            Role = Role.Customer
        });

        var token = fixture.EmailService.ExtractToken("alreadyverified@test.com");
        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = token! });

        // Act
        var response = await client.PostAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = token! });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region ForgotPassword

    [Fact]
    public async Task ForgotPassword_ShouldReturn204_WhenUserExists()
    {
        // Arrange
        var client = fixture.CreateClient();

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = "forgotpw@test.com",
            Password = "Password123!",
            Role = Role.Customer
        });

        fixture.EmailService.Reset();

        // Act
        var response = await client.PostAsync("/api/Auth/forgot-password", new ForgotPasswordRequest
        {
            Email = "forgotpw@test.com"
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(fixture.EmailService.ExtractToken("forgotpw@test.com"));
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturn204_WhenUserDoesNotExist()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act — should not reveal whether user exists
        var response = await client.PostAsync("/api/Auth/forgot-password", new ForgotPasswordRequest
        {
            Email = "doesnotexist@test.com"
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region ResetPassword

    [Fact]
    public async Task ResetPassword_ShouldReturn204_WhenTokenIsValid()
    {
        // Arrange
        var client = fixture.CreateClient();
        const string email = "resetpw@test.com";

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = email,
            Password = "OldPassword123!",
            Role = Role.Customer
        });

        fixture.EmailService.Reset();
        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/forgot-password", new ForgotPasswordRequest { Email = email });
        var token = fixture.EmailService.ExtractToken(email);

        // Act
        var response = await client.PostAsync("/api/Auth/reset-password", new ResetPasswordRequest
        {
            Token = token!,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturn400_WhenTokenIsInvalid()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Auth/reset-password", new ResetPasswordRequest
        {
            Token = "invalid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Logout

    [Fact]
    public async Task Logout_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Auth/logout", new RefreshTokenRequest { RefreshToken = "any-token" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldReturn204_WhenAuthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();
        const string email = "logout@test.com";
        const string password = "Password123!";

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            Role = Role.Customer
        });

        var token = fixture.EmailService.ExtractToken(email);
        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = token! });

        var loginResponse = await (await client.PostAsync("/api/Auth/login", new LoginRequest { Email = email, Password = password }))
            .Content.ReadAsync<LoginResponse>();

        var authenticatedClient = fixture.CreateClient(loginResponse!.User.Id, Role.Customer);

        // Act
        var response = await authenticatedClient.PostAsync("/api/Auth/logout", new RefreshTokenRequest
        {
            RefreshToken = loginResponse.RefreshToken
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldInvalidateRefreshToken()
    {
        // Arrange
        var client = fixture.CreateClient();
        const string email = "logoutinvalidate@test.com";
        const string password = "Password123!";

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            Role = Role.Customer
        });

        var token = fixture.EmailService.ExtractToken(email);
        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = token! });

        var loginResponse = await (await client.PostAsync("/api/Auth/login", new LoginRequest { Email = email, Password = password }))
            .Content.ReadAsync<LoginResponse>();

        var authenticatedClient = fixture.CreateClient(loginResponse!.User.Id, Role.Customer);
        await authenticatedClient.PostAsJsonEnsureSuccessAsync("/api/Auth/logout", new RefreshTokenRequest
        {
            RefreshToken = loginResponse.RefreshToken
        });

        // Act — attempt to use the invalidated refresh token
        var response = await client.PostAsync("/api/Auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = loginResponse.RefreshToken
        });

        // Assert — invalidated token is treated as unauthorized, not bad request
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Refresh

    [Fact]
    public async Task Refresh_ShouldReturn401_WhenTokenIsInvalid()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Auth/refresh", new RefreshTokenRequest { RefreshToken = "invalid-token" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ShouldReturn200_WhenTokenIsValid()
    {
        // Arrange
        var client = fixture.CreateClient();
        const string email = "refresh@test.com";
        const string password = "Password123!";

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            Role = Role.Customer
        });

        var token = fixture.EmailService.ExtractToken(email);
        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = token! });

        var loginResponse = await (await client.PostAsync("/api/Auth/login", new LoginRequest { Email = email, Password = password }))
            .Content.ReadAsync<LoginResponse>();

        // Act
        var response = await client.PostAsync("/api/Auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = loginResponse!.RefreshToken
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshResponse = await response.Content.ReadAsync<LoginResponse>();
        Assert.NotNull(refreshResponse);
        Assert.NotNull(refreshResponse.AccessToken);
        Assert.NotNull(refreshResponse.RefreshToken);
    }

    #endregion

    #region Me

    [Fact]
    public async Task Me_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturn200_WhenAuthenticated()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);

        // Act
        var response = await client.GetAsync("/api/Auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadAsync<CustomerDto>();
        Assert.NotNull(user);
        Assert.Equal(fixture.SeedData.Customer.Id, user.Id);
        Assert.Equal("customer@test.com", user.Email);
        Assert.Equal(Role.Customer, user.Role);
    }

    #endregion

    #region ChangePassword

    [Fact]
    public async Task ChangePassword_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPassword123!"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturn400_WhenCurrentPasswordIsWrong()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.Customer);

        // Act
        var response = await client.PostAsync("/api/Auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "NewPassword123!"
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturn204_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        var client = fixture.CreateClient();
        const string email = "changepw@test.com";
        const string password = "Password123!";

        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            Role = Role.Customer
        });

        var token = fixture.EmailService.ExtractToken(email);
        await client.PostAsJsonEnsureSuccessAsync("/api/Auth/verify-email", new VerifyEmailRequest { Token = token! });

        var loginResponse = await (await client.PostAsync("/api/Auth/login", new LoginRequest { Email = email, Password = password }))
            .Content.ReadAsync<LoginResponse>();

        var authenticatedClient = fixture.CreateClient(loginResponse!.User.Id, Role.Customer);

        // Act
        var response = await authenticatedClient.PostAsync("/api/Auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = password,
            NewPassword = "NewPassword123!"
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion
}
