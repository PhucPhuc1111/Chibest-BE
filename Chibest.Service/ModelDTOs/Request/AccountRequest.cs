namespace Chibest.Service.ModelDTOs.Request;

public class AccountRequest
{
    public Guid? Id { get; set; }
    public string? AvartarUrl { get; set; }
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Cccd { get; set; }
    public string? TaxCode { get; set; }
    public string? FaxNumber { get; set; }
    public string? Status { get; set; }

    //Optional when create account
    public Guid? RoleId { get; set; }
    public Guid? BranchId { get; set; }
}