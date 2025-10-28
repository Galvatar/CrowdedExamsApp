using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public TokenRefreshMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, CrowdedExamsDb db)
    {
        // First, let the rest of the pipeline run
        await _next(context);

        // After the request is handled, check if we need to refresh the token.
        // Only proceed if the user is authenticated and the response was successful.
        if (context.User.Identity?.IsAuthenticated == true && context.Response.StatusCode < 400)
        {
            var expClaim = context.User.FindFirst(c => c.Type == JwtRegisteredClaimNames.Exp);
            if (expClaim == null) return;

            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).UtcDateTime;
            var refreshThreshold = DateTime.UtcNow.AddMinutes(30); // Refresh if token expires in the next 30 mins

            if (expirationTime < refreshThreshold)
            {
                var userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString)) return;

                var user = await db.Users.FindAsync(int.Parse(userIdString));
                if (user == null) return;

                // Generate a new token (assuming a non-persistent 2-hour session)
                var newToken = GenerateJwtToken(user, false, _configuration);

                // Set the new token in the response cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(2) // Match the new token's expiry
                };
                context.Response.Cookies.Append("accessToken", newToken, cookieOptions);
            }
        }
    }

    // This is a static version of your token generation logic
    private static string GenerateJwtToken(User user, bool rememberMe, IConfiguration configuration)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(2);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = creds,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            Expires = expires
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}