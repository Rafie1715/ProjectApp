namespace CutiApp.Models;

public class LeaveBalance
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RemainingDays { get; set; }

    public User User { get; set; } = null!;
}

