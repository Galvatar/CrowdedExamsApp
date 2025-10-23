using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/exams")]
[Authorize]
public class ExamController : ControllerBase
{
    private readonly CrowdedExamsDb _database;
    private readonly IConfiguration _configuration;

    public ExamController(CrowdedExamsDb context, IConfiguration configuration)
    {
        _database = context;
        _configuration = configuration;
    }

    [HttpGet("userrole")]
    public IActionResult getUserRole()
    {
        string userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == null)
        {
            return BadRequest("No role");
        }
        return Ok(userRole);
    }
}