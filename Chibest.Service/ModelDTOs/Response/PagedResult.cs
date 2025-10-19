namespace Chibest.Service.ModelDTOs.Response;

public class PagedResult<T> where T : class
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPage => (int)Math.Ceiling(decimal.Divide(TotalCount, PageSize));
    public List<T> DataList { get; set; } = [];
}
