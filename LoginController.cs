using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using SendGrid.Helpers.Errors.Model;


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
        public bool Moderator { get; set; } = false;

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

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            Expires = doesExpire ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(2)
        };
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
        var user = await _database.Users.FirstOrDefaultAsync(t => t.Email == partialUser.Email.ToString());
        if (user == null)
        {
            return Unauthorized("Wrong password or email");
        }
        if (user.Password == null)
        {
            return Unauthorized("Use google signin");
        }
        if (!BCrypt.Net.BCrypt.Verify(partialUser.Password, user.Password))
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
        else
        {
            cookieOptions.Expires = DateTime.UtcNow.AddHours(2);
        }
        Response.Cookies.Append("accessToken", token, cookieOptions);

        LoginResponse responsePayload = new LoginResponse();
        responsePayload.FirstName = user.FirstName;
        responsePayload.LastName = user.LastName;
        responsePayload.Institution = user.Institution;
        return Ok(responsePayload);
    }

    [HttpGet("user-info")]
    [Authorize]
    public async Task<IActionResult> getUserInformation()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var userId))
        {
            return Unauthorized("Invalid user identifier.");
        }

        var user = await _database.Users.FindAsync(userId);
        if (user == null)
        {
            return Unauthorized("No valid user");
        }

        var payload = new
        {
            user.FirstName,
            user.LastName,
            user.Institution,
            Moderator = user.Role == "Moderator"
        };

        return Ok(payload);
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

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        var verificationUrl = $"https://crowded-exams.onrender.com/?token={user.EmailVerificationToken}";
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
        var emailText = $@"Welcome to Crowded Exams!

        Please verify your email by opening this link: {verificationUrl}

        This link expires in 1 hour.";

        await _email.SendAsync(
            to: user.Email,
            subject: "Verify your email - Crowded Exams",
            htmlBody: emailHtml,
            plainTextBody: emailText
        );

        return NoContent();
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var callbackUrl = Url.Action(nameof(GoogleCallback), "Login", values: null, protocol: Request.Scheme);
        var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        Console.WriteLine("Entered");
        var frontendLoginUrl = "https://crowded-exams.onrender.com/";
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (result?.Succeeded != true)
        {
            return Redirect($"{frontendLoginUrl}?error=google-auth-failed");
        }

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var firstName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        var lastName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        var hostedDomain = claims?.FirstOrDefault(c => c.Type == "hd")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Redirect($"{frontendLoginUrl}?error=email-not-found");
        }
        Console.WriteLine(email);

        var user = await _database.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            Console.WriteLine(hostedDomain);
            string institutionName = "Unknown";
            if (string.IsNullOrEmpty(hostedDomain))
            {
                return Redirect($"{frontendLoginUrl}?error=personal-email-not-allowed");
            }
            var institution = await _database.Institutions.FirstOrDefaultAsync(i => i.Email == hostedDomain);
            if (institution == null)
            {
                return Redirect($"{frontendLoginUrl}?error=institution-not-supported");
            }
            user = new User
            {
                Email = email,
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                isEmailVerified = true,
                Role = "Student",
                Institution = institution.Name
            };
            _database.Users.Add(user);
            await _database.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user, true);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(30)
        };
        Response.Cookies.Append("accessToken", token, cookieOptions);

        return Redirect($"{frontendLoginUrl}");
    }
}
