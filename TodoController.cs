// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;


// [ApiController]
// [Route("api/todo")]
// public class TodoController : ControllerBase
// {
//     private readonly TodoDb _database;
//     public TodoController(TodoDb context)
//     {
//         _database = context;
//     }

//     private string? GetCurrentUserId()
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
//                      ?? User.FindFirst("sub")?.Value
//                      ?? User.FindFirst("oid")?.Value;

//         if (!string.IsNullOrWhiteSpace(userId))
//         {
//             return userId;
//         }

//         if (Request.Headers.TryGetValue("X-User-Id", out var header) && !string.IsNullOrWhiteSpace(header))
//         {
//             return header.ToString();
//         }

//         return null;
//     }

//     [HttpPost]
//     public async Task<IActionResult> CreateTodoItem([FromBody] TodoItem newItem)
//     {
//         var userId = GetCurrentUserId();
//         if (string.IsNullOrWhiteSpace(userId))
//         {
//             return Unauthorized("Missing user id. Provide a JWT with a subject/NameIdentifier claim or set X-User-Id header in dev.");
//         }

//         newItem.UserId = userId;

//         _database.TodoItems.Add(newItem);
//         await _database.SaveChangesAsync();

//         return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id }, newItem);
//     }

//     [HttpGet("{id}")]
//     public async Task<IActionResult> GetItemById(int id)
//     {
//         var userId = GetCurrentUserId();
//         if (string.IsNullOrWhiteSpace(userId))
//         {
//             return Unauthorized("Missing user id.");
//         }

//         var item = await _database.TodoItems.FindAsync(id);
//         if (item == null || item.UserId != userId)
//         {
//             return NotFound();
//         }
//         return Ok(item);
//     }

//     [HttpPut("{id}")]
//     public async Task<IActionResult> UpdateItem(int id, [FromBody] TodoItem item)
//     {
//         var userId = GetCurrentUserId();
//         if (string.IsNullOrWhiteSpace(userId))
//         {
//             return Unauthorized("Missing user id.");
//         }

//         if (item.Id != id)
//         {
//             return BadRequest();
//         }

//         var existing = await _database.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
//         if (existing == null)
//         {
//             return NotFound();
//         }

//         existing.Description = item.Description;
//         existing.IsCompleted = item.IsCompleted;

//         await _database.SaveChangesAsync();

//         return NoContent();
//     }

//     [HttpGet]
//     public async Task<IActionResult> GetAllItems()
//     {
//         var userId = GetCurrentUserId();
//         if (string.IsNullOrWhiteSpace(userId))
//         {
//             return Unauthorized("Missing user id.");
//         }

//         var items = await _database.TodoItems
//             .Where(t => t.UserId == userId)
//             .ToListAsync();
//         return Ok(items);
//     }

//     [HttpDelete("{id}")]
//     public async Task<IActionResult> DeleteItem(int id)
//     {
//         var userId = GetCurrentUserId();
//         if (string.IsNullOrWhiteSpace(userId))
//         {
//             return Unauthorized("Missing user id.");
//         }

//         var item = await _database.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

//         if (item == null)
//         {
//             return NotFound();
//         }

//         _database.TodoItems.Remove(item);
//         await _database.SaveChangesAsync();

//         return NoContent();
//     }

//     [HttpGet("ping")]
//     public IActionResult Ping()
//     {
//         return Ok("ACK");
//     }
// }