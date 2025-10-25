using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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

    public class ExamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Points { get; set; }
        public List<SolutionDto> Solutions { get; set; } = new();
    }

    public class SolutionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string User { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public List<Reply> Replies { get; set; } = new List<Reply>();
        public string? UserVote { get; set; } // The new attribute
    }

    public class VoteUpdateDto
    {
        public string Vote { get; set; } = string.Empty;
    }

    public class CommentDto {
        public string Text { get; set; } = string.Empty;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> getExamById(int id)
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

        var examDto = await _database.Exams
            .Where(e => e.Id == id && e.Institution == user.Institution)
            .Select(exam => new ExamDto
            {
                Id = exam.Id,
                Name = exam.Name,
                Course = exam.Course,
                Difficulty = exam.Difficulty,
                Institution = exam.Institution,
                Questions = exam.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Number = q.Number,
                    Description = q.Description,
                    Points = q.Points,
                    Solutions = q.Solutions.Select(s => new SolutionDto
                    {
                        Id = s.Id,
                        UserId = s.UserId,
                        User = s.User,
                        Description = s.Description,
                        UpVotes = s.UpVotes,
                        DownVotes = s.DownVotes,
                        Replies = s.Replies,
                        UserVote = _database.UserVotes
                            .Where(v => v.SolutionId == s.Id && v.UserId == userId)
                            .Select(v => v.Vote)
                            .FirstOrDefault()
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (examDto == null)
        {
            return NotFound();
        }

        return Ok(examDto);
    }

    [HttpPut("questions/{questionId}/solutions")]
    public async Task<IActionResult> newExamSolutions(int questionId, [FromBody] Solution solution)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _database.Users.FirstOrDefaultAsync(t => t.Id == int.Parse(userId));
        if (user == null)
        {
            return Unauthorized("No valid user");
        }
        var question = await _database.Questions.FirstOrDefaultAsync(q => q.Id == questionId);
        if (question == null)
        {
            return NotFound("No such question");
        }
        var newSolution = new Solution
        {
            QuestionId = questionId,
            Question = question,
            UserId = int.Parse(userId),
            User = user.FirstName + " " + user.LastName,
            Description = solution.Description,
            UpVotes = 0,
            DownVotes = 0,
            Replies = new List<Reply>()
        };
        _database.Solutions.Add(newSolution);
        await _database.SaveChangesAsync();
        return Created("", newSolution);
    }

    [HttpPut("questions/{solutionId}/comments")]
    public async Task<IActionResult> addComment(int solutionId, [FromBody] CommentDto text)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var userId))
        {
            return Unauthorized("Invalid user identifier.");
        }

        var user = await _database.Users.FindAsync(userId);
        if (user == null) {
            return Unauthorized("Invalid user identifier.");
        }


        var solution = await _database.Solutions.FirstOrDefaultAsync(s => s.Id == solutionId);
        if (solution == null)
        {
            return NotFound("No solution with that id");
        }

        Reply newReply = new Reply
        {
            SolutionId = solution.Id,
            User = user.FirstName + " " + user.LastName,
            UserId = user.Id,
            Description = text.Text
        };
        solution.Replies.Add(newReply);
        await _database.SaveChangesAsync();
        return Created();
    }
    
    [HttpPatch("questions/{solutionId}/solutions")]
    public async Task<IActionResult> newVotes(int solutionId, [FromBody] VoteUpdateDto voteUpdate) {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _database.Users.FirstOrDefaultAsync(t => t.Id == int.Parse(userId));
        if (user == null)
        {
            return Unauthorized("No valid user");
        }
        var curSolution = await _database.Solutions.FirstOrDefaultAsync(q => q.Id == solutionId);
        if (curSolution == null)
        {
            return NotFound("No such solution");
        }
        var curVote = await _database.UserVotes.FirstOrDefaultAsync(v => v.SolutionId == solutionId && v.UserId == user.Id);
        if (curVote == null)
        {
            var newVote = new UserVote
            {
                UserId = user.Id,
                SolutionId = solutionId,
                Vote = voteUpdate.Vote
            };
            if (voteUpdate.Vote == "up") {
                curSolution.UpVotes++;
            } else {
                curSolution.DownVotes++;
            }
            user.Votes.Add(newVote);
        }
        else
        {
            if (curVote.Vote == "down" && voteUpdate.Vote == "up") {
                curSolution.UpVotes++;
                curSolution.DownVotes--;
            } else if (curVote.Vote == "up" && voteUpdate.Vote == "down") {
                curSolution.UpVotes--;
                curSolution.DownVotes++;
            }
            curVote.Vote = voteUpdate.Vote;
        }
        await _database.SaveChangesAsync();
        return Ok(new { curSolution.UpVotes, curSolution.DownVotes });
    }
}