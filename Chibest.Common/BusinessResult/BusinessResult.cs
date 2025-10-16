namespace Chibest.Common.BusinessResult
{
    public class BusinessResult : IBusinessResult
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }

        public BusinessResult(int eRROR_EXCEPTION)
        {
            StatusCode = -1;
            Message = "Action fail";
        }

        public BusinessResult(int status, string message)
        {
            StatusCode = status;
            Message = message;
        }

        public BusinessResult(int status, string message, object data)
        {
            StatusCode = status;
            Message = message;
            Data = data;
        }
    }
}
