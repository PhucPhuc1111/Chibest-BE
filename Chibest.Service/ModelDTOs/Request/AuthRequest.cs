namespace Chibest.Service.ModelDTOs.Request;

public class AuthRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}