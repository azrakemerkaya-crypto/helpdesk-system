using System.ComponentModel.DataAnnotations;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.DTOs;

public record RegisterRequest([Required] string FullName, [Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public record CreateTicketRequest([Required, MinLength(3)] string Title, [Required, MinLength(5)] string Description, int CategoryId, TicketPriority Priority, string? DeviceSerialNumber);
public record UpdateStatusRequest(TicketStatus Status);
public record AssignTicketRequest(int TechnicianId);
public record AddCommentRequest([Required, MinLength(1)] string Text);
