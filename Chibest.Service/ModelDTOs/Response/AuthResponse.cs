namespace Chibest.Service.ModelDTOs.Response;
public class AuthResponse
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public int ExpiresIn { get; set; } = 0;
    public Guid AccountId { get; set; } = Guid.Empty;
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}