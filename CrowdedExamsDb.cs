using Microsoft.EntityFrameworkCore;

public class CrowdedExamsDb : DbContext
{
    public CrowdedExamsDb(DbContextOptions<CrowdedExamsDb> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Institution> Institutions { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Solution> Solutions { get; set; }
    public DbSet<Reply> Replies { get; set; }
    public DbSet<UserVote> UserVotes { get; set; }
}