﻿namespace Chibest.Service.ModelDTOs.Request.Query;

public class ProductQuery: BaseQuery
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsMaster { get; set; }
    public string? Brand { get; set; }
}