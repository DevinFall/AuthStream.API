using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using AuthStream.API.Data;
using AuthStream.API.DTOs;
using AuthStream.API.Helpers;
using AuthStream.API.Models;
using AuthStream.API.Services;
using Microsoft.EntityFrameworkCore;

namespace TrekReserve.Auth.Services;

public class AuthenticationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ConfigurationService _configuration;
    private readonly TokenService _tokenService;
    private readonly EmailSenderService _emailService;
    public AuthenticationService(
        ApplicationDbContext authDbContext,
        ConfigurationService configuration,
        TokenService tokenService,
        EmailSenderService emailService)
    {
        _dbContext = authDbContext;
        _configuration = configuration;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<ServiceResponse<string>> LoginAsync(LoginForm loginForm)
    {
        var serviceResponse = new ServiceResponse<string>();

        try
        {
            // Get user object from database
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginForm.Email.ToLower());

            // Check the client's credentials agains server's
            if (dbUser is null || !PasswordHelper.VerifyPasswordHash(loginForm.Password, dbUser.PasswordHash, dbUser.PasswordSalt))
            {
                throw new Exception("Invalid credentials!");
            }

            var claims = new List<Claim>
            {
                new Claim("token_type", "account_login"),
                new Claim(ClaimTypes.NameIdentifier, dbUser.Id),
                new Claim(ClaimTypes.Name, dbUser.Name),
                new Claim(ClaimTypes.Email, dbUser.Email),
                new Claim("admin", dbUser.IsAdmin.ToString().ToLower()),
                new Claim("account_confirmed", dbUser.IsAccountConfirmed.ToString().ToLower())
            };
            var expirationDateTime = DateTime.UtcNow.AddDays(1);

            var token = _tokenService.CreateJWT(claims, expirationDateTime);

            serviceResponse.Data = token;
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = ex.Message;
        }

        // Return JWT of type 'Login' or null
        return serviceResponse;
    }

    public async Task<ServiceResponse<string>> RegisterAsync(RegisterForm registerForm)
    {
        var serviceResponse = new ServiceResponse<string>();

        try
        {
            // Check if email is taken
            if (await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == registerForm.Email.ToLower()))
            {
                throw new Exception($"There is already an account using that email. Please try another.");
            }

            // Create user's password hash and salt
            PasswordHelper.CreatePasswordHash(registerForm.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Create a user object
            var newUser = new User
            {
                Name = registerForm.Name,
                Email = registerForm.Email.ToLower(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            // Save new user to database
            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            // TODO: Schedule user deletion if email
            //  is not confirmed within 15 minutes

            var hostBaseUrl = _configuration.GetSection("Client:BaseAddress");

            var confirmationTokenClaims = new List<Claim>
            {
                new Claim(type: "token_type", value: "account_confirmation"),
                new Claim(ClaimTypes.Email, newUser.Email)
            };
            var confirmationTokenExpirationTime = DateTime.UtcNow.AddMinutes(30);

            var confirmationToken = _tokenService.CreateJWT(confirmationTokenClaims, confirmationTokenExpirationTime);
            
            var confirmationEndpoint = _configuration.GetSection("Client:Endpoints:ConfirmAccountEmail");
            var confirmationUrl = $"{hostBaseUrl}{confirmationEndpoint}/{confirmationToken}";

            var fromAddress = new MailAddress(_emailService.SMTPUser, "TrekReserve");
            var toAddress = new MailAddress(newUser.Email, newUser.Name);
            var mailMessage = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Verify your TrekReserve Account",
                Body =
                    "<b>Please verify your TrekReserve account.</b><br>" +
                    "<e>You won't be able to use our service until your email is confirmed.</e><br>" +
                    $"<p>Click <a href='{confirmationUrl}'>here</a> to activate your account.</p>",
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
            };

            await _emailService.SendEmailAsync(mailMessage);

            var loginForm = new LoginForm
            {
                Email = newUser.Email,
                Password = registerForm.Password
            };

            serviceResponse = await LoginAsync(loginForm);
            serviceResponse.Message = "Successfully created account. The client can now login with the attatched token.";
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = ex.Message;
        }

        // Return user's email
        return serviceResponse;
    }

    public async Task<ServiceResponse<string>> VerifyEmailAsync(string confirmationToken)
    {
        var serviceResponse = new ServiceResponse<string>();

        try
        {
            var isTokenValid = _tokenService.IsValidJWT(confirmationToken, out ClaimsPrincipal? claimsPrincipal);

            if (isTokenValid is false || claimsPrincipal is null)
            {
                throw new Exception("Confirmation link is not valid. Please log in to receive another.");
            }

            var accountConfirmedClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "token_type");
            
            if (accountConfirmedClaim is null)
            {
                throw new Exception("Claim of type 'token_type' not found in confirmation token.");
            }

            if (accountConfirmedClaim.Value is not "account_confirmation")
            {
                throw new Exception("Not a valid confirmation token.");
            }

            var emailClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim is null)
            {
                throw new Exception("Claim of type 'email' not found in confirmation token.");
            }

            var email = emailClaim.Value;

            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (dbUser is null)
            {
                throw new Exception("User with provided email not found.");
            }

            dbUser.IsAccountConfirmed = true;

            await _dbContext.SaveChangesAsync();

            serviceResponse.Message = "Successfully verified account email.";
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = ex.Message;
        }

        // Return null
        return serviceResponse;
    }

    public async Task<ServiceResponse<string>> ResendConfirmationEmail(IEnumerable<Claim> userClaims)
    {
        var serviceResponse = new ServiceResponse<string>();

        try
        {
            var emailClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim is null)
            {
                throw new Exception("Email not found.");
            }

            var email = emailClaim.Value;

            var hostBaseUrl = _configuration.GetSection("Client:BaseAddress");

            var confirmationTokenClaims = new List<Claim>
            {
                new Claim(type: "token_type", value: "account_confirmation"),
                new Claim(ClaimTypes.Email, email)
            };
            var confirmationTokenExpirationTime = DateTime.UtcNow.AddMinutes(30);

            var confirmationToken = _tokenService.CreateJWT(confirmationTokenClaims, confirmationTokenExpirationTime);
            
            var confirmationEndpoint = _configuration.GetSection("Client:Endpoints:ConfirmAccountEmail");
            var confirmationUrl = $"{hostBaseUrl}{confirmationEndpoint}/{confirmationToken}";

            var fromAddress = new MailAddress(_emailService.SMTPUser, "TrekReserve");
            var toAddress = new MailAddress(email, email);
            var mailMessage = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Verify your TrekReserve Account",
                Body =
                    "<b>Please verify your TrekReserve account.</b><br>" +
                    "<e>You won't be able to use our service until your email is confirmed.</e><br>" +
                    $"<p>Click <a href='{confirmationUrl}'>here</a> to activate your account.</p>",
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
            };

            await _emailService.SendEmailAsync(mailMessage);

            serviceResponse.Message = $"Successfully sent account confirmation email to '{email}'.";
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = ex.Message;
        }

        return serviceResponse;
    }
}