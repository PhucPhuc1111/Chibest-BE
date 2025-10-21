namespace Chibest.Service.ModelDTOs.Request;
public class SystemLogRequest
{
    public Guid Id { get; set; }

    public string Action { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public Guid? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Description { get; set; }

    public Guid? AccountId { get; set; }

    public string? AccountName { get; set; }

    public string? Ipaddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public string LogLevel { get; set; } = null!;

    public string? Module { get; set; }
}