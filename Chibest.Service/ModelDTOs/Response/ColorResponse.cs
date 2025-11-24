using System;

namespace Chibest.Service.ModelDTOs.Response;

public class ColorResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}

