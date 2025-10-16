namespace Chibest.Common.BusinessResult
{
    public interface IBusinessResult
    {
        int StatusCode { get; set; }
        string? Message { get; set; }
        object? Data { get; set; }
    }
}
