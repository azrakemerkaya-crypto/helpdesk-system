namespace HelpdeskSystem.Models;

public enum UserRole { User, Technician, Admin }
public enum TicketStatus { Open, Assigned, InProgress, Resolved, Closed }
public enum TicketPriority { Low, Normal, High, Critical }

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    public string? DeviceSerialNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public List<Comment> Comments { get; set; } = [];
}

public class Comment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
