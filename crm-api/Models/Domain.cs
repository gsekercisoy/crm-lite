using System.ComponentModel.DataAnnotations;

namespace CrmApi.Models;

public class User
{
    public int Id { get; set; }
    [Required] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [Required] public string PasswordSalt { get; set; } = "";
    [Required] public string Role { get; set; } = "Sales"; // Admin | Sales
}

public class Customer
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Company { get; set; } = "";
    public List<Deal> Deals { get; set; } = new();
}

public class Deal
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int OwnerId { get; set; }
    [Required] public string Title { get; set; } = "";
    public decimal Value { get; set; }
    [Required] public string Stage { get; set; } = "Lead"; // Lead|Qualified|Proposal|Won|Lost
    public DateTime? ExpectedClose { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Activity> Activities { get; set; } = new();

    public bool IsClosed => Stage == DealStages.Won || Stage == DealStages.Lost;
}

public class Activity
{
    public int Id { get; set; }
    public int DealId { get; set; }
    public Deal? Deal { get; set; }
    public int UserId { get; set; }
    [Required] public string Kind { get; set; } = "Note"; // Call|Meeting|Email|Note
    [Required] public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class DealStages
{
    public static readonly string[] All = ["Lead", "Qualified", "Proposal", "Won", "Lost"];
    public const string Lead = "Lead";
    public const string Qualified = "Qualified";
    public const string Proposal = "Proposal";
    public const string Won = "Won";
    public const string Lost = "Lost";
    public static bool IsValid(string? s) => All.Contains(s ?? "");
    public static bool IsClosed(string? s) => s == Won || s == Lost;
}

public static class ActivityKinds
{
    public static readonly string[] All = ["Call", "Meeting", "Email", "Note"];
    public static bool IsValid(string? s) => All.Contains(s ?? "");
}
