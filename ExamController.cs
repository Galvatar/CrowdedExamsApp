using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

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

    [HttpGet("create")]
    [Authorize(Roles = "Moderator")]
    public IActionResult getAuthorization()
    {
        var userRole = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userRole == null)
        {
            return Unauthorized("No permission");
        }
        return Ok(userRole);
    }

    [HttpPut("create")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> createExam([FromBody] Exam exam)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _database.Users.FirstOrDefaultAsync(t => t.Id == int.Parse(userId));
        if (user == null)
        {
            return Unauthorized("No valid user");
        }
        exam.Institution = user.Institution;
        await _database.Exams.AddAsync(exam);
        await _database.SaveChangesAsync();
        return Created();
    }

    [HttpGet]
    public async Task<IActionResult> getAllExams()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _database.Users.FirstOrDefaultAsync(t => t.Id == int.Parse(userId));
        if (user == null)
        {
            return Unauthorized("No valid user");
        }
        var items = _database.Exams
            .Where(e => e.Institution == user.Institution)
            .ToListAsync();
        return Ok(items);
    }
    
    [HttpGet("{id}/questions")]
    public async Task<IActionResult> getExamQuestions(int id) 
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _database.Users.FirstOrDefaultAsync(t => t.Id == int.Parse(userId));
        if (user == null)
        {
            return Unauthorized("No valid user");
        }
        var items = _database.Questions
            .Where(e => e.ExamId == id)
            .ToListAsync();
        return Ok(items);
    }
}