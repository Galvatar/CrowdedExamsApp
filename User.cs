public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Boolean isEmailVerified { get; set; } = false;
    public string EmailVerificationToken { get; set; } = string.Empty;
    public DateTime VerificationTokenExpires { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<UserVote> Votes { get; set; } = new();
}