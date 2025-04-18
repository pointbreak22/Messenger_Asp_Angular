using System.IdentityModel.Tokens.Jwt;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChatApi.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string userId, string userName)
    {
        var keyString = _config["JWTSetting:SecurityKey"];
        if (string.IsNullOrEmpty(keyString))
            throw new Exception("JWT SecurityKey is not configured");


        var key = Encoding.ASCII.GetBytes(keyString);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),

        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey
                (key), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);

        // {
        //     MaximumTokenSizeInBytes = 0,
        //     SetDefaultTimesOnTokenCreation = false,
        //     TokenLifetimeInMinutes = 0,
        //     MapInboundClaims = false,
        //     InboundClaimTypeMap = null,
        //     OutboundClaimTypeMap = null,
        //     InboundClaimFilter = null
        // };

    }
}