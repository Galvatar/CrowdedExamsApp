using Microsoft.EntityFrameworkCore;

public class CrowdedExamsDb : DbContext
{
    public CrowdedExamsDb(DbContextOptions<CrowdedExamsDb> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Institution> Institutions { get; set; }
    public DbSet<Exam> Exams { get; set; }
}