using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


[ApiController]
[Route("api/login")]
public class LoginController : ControllerBase
{
    private readonly IEmailSender _email;
    private readonly CrowdedExamsDb _database;
    private readonly IConfiguration _configuration;

    public LoginController(CrowdedExamsDb context, IConfiguration configuration, IEmailSender email)
    {
        _database = context;
        _configuration = configuration;
        _email = email;
    }

    private class LoginResponse
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;

    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    private string GenerateJwtToken(User user, Boolean doesExpire)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        if (key == null)
        {
            Console.WriteLine("Error getting key");
        }

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddDays(30);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };
        if (doesExpire)
        {
            tokenDescriptor.Expires = expires;
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    [HttpGet("{emailToken}")]
    public async Task<IActionResult> verifyEmail(string emailToken)
    {
        var user = await _database.Users.FirstOrDefaultAsync(t => t.EmailVerificationToken == emailToken);
        if (user == null)
        {
            return Unauthorized("Invalid token");
        }
        if (DateTime.UtcNow > user.VerificationTokenExpires)
        {
            return Unauthorized("Time limit exceeded");
        }
        user.isEmailVerified = true;
        await _database.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> getAllInstitutions()
    {
        var items = await _database.Institutions.ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> getLoggedIn([FromBody] LoginDto partialUser)
    {

        if (string.IsNullOrWhiteSpace(partialUser.Email) || string.IsNullOrWhiteSpace(partialUser.Password))
        {
            return BadRequest("Missing email or password");
        }
        var user = await _database.Users.FirstOrDefaultAsync(t => t.Email == partialUser.Email.ToString() && t.Password == partialUser.Password.ToString());
        if (user == null)
        {
            return Unauthorized("Wrong password or email");
        }
        if (!user.isEmailVerified)
        {
            return Unauthorized("Unverified email");
        }
        var token = GenerateJwtToken(user, partialUser.RememberMe);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        };
        if (partialUser.RememberMe)
        {
            cookieOptions.Expires = DateTime.UtcNow.AddDays(30);
        }
        Response.Cookies.Append("accessToken", token, cookieOptions);

        LoginResponse responsePayload = new LoginResponse();
        responsePayload.FirstName = user.FirstName;
        responsePayload.LastName = user.LastName;
        responsePayload.Institution = user.Institution;
        return Ok(responsePayload);
    }

    [HttpPut]
    public async Task<IActionResult> createAccount([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
        {
            return BadRequest("Email or password");
        }

        var exists = await _database.Users.AnyAsync(u => u.Email == user.Email);
        if (exists)
        {
            return Conflict("Email already exists");
        }
        user.Role = "Student";
        user.EmailVerificationToken = Guid.NewGuid().ToString();
        user.VerificationTokenExpires = DateTime.UtcNow.AddHours(1);

        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        var verificationUrl = $"https://localhost:3000/?token={user.EmailVerificationToken}";
        var emailHtml = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Welcome to Crowded Exams!</h2>
                <p>Thanks for signing up. Please verify your email by clicking the button below:</p>
                <a href='{verificationUrl}' 
                   style='display: inline-block; padding: 12px 24px; background-color: #007bff; 
                          color: white; text-decoration: none; border-radius: 4px; margin: 16px 0;'>
                    Verify Email
                </a>
                <p style='color: #666; font-size: 12px;'>
                    This link expires in 1 hour. If you didn't create an account, please ignore this email.
                </p>
            </body>
            </html>";

        await _email.SendAsync(
            to: user.Email,
            subject: "Verify your email - Crowded Exams",
            htmlBody: emailHtml
        );

        return NoContent();
    }
}
