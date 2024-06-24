using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthStream.API.Services;

public class TokenService
{
    private readonly ConfigurationService _configuration;
    public TokenService(ConfigurationService configuration)
    {
        _configuration = configuration;

        signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetSection("Security:Key")));
    }

    private SymmetricSecurityKey signingKey;

    public string CreateJWT(List<Claim> claims, DateTime expirationDateTime)
    {
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expirationDateTime,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public bool IsValidJWT(string token, out ClaimsPrincipal? claimsPrincipal)
    {
        claimsPrincipal = null;

        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            // Throws an exception if the given token is not valid
            claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}