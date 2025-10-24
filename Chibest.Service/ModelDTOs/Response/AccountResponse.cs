namespace Chibest.Service.ModelDTOs.Response;

public class AccountResponse
{
    public Guid Id { get; set; }
    public string? FcmToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? AvartarUrl { get; set; }
    public string Code { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Cccd { get; set; }
    public string? FaxNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; } = null!;
}
