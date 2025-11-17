using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Chibest.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chibest.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class PermissionAttribute : TypeFilterAttribute
{
    public PermissionAttribute(params string[] permissionCodes)
        : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = new object[] { permissionCodes };
    }
}

public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IPermissionService _permissionService;
    private readonly string[] _requiredPermissions;

    public PermissionAuthorizationFilter(string[] permissionCodes, IPermissionService permissionService)
    {
        _requiredPermissions = permissionCodes ?? Array.Empty<string>();
        _permissionService = permissionService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Check if AllowAnonymous is present on the action or controller
        var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
            .Any(m => m is IAllowAnonymous);
        
        if (hasAllowAnonymous)
        {
            return;
        }

        if (_requiredPermissions.Length == 0)
        {
            context.Result = new ForbidResult();
            return;
        }

        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var accountIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(accountIdValue, out var accountId) || accountId == Guid.Empty)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasPermission = await _permissionService.HasPermissionAsync(accountId, _requiredPermissions);
        if (hasPermission == false)
        {
            context.Result = new ForbidResult();
        }
    }
}

