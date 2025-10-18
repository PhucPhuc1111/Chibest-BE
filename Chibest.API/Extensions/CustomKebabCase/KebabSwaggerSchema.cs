using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Chibest.API.Extensions.CustomKebabCase;

public class KebabSwaggerSchema : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null)
        {
            return;
        }

        var kebabCaseProperties = schema.Properties.ToList();
        schema.Properties.Clear();

        foreach (var entry in kebabCaseProperties)
        {
            var kebabCaseName = ToKebabCase(entry.Key);
            schema.Properties.Add(kebabCaseName, entry.Value);
        }
    }

    private static string ToKebabCase(string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
    }
}