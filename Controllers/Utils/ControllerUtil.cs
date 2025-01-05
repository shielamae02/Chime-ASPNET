using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Chime_ASPNET.Models.Response;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static Chime_ASPNET.Models.Utils.Error;

namespace Chime_ASPNET.Controllers.Utils;

public static class ControllerUtil
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdString, out var userId) ? userId : -1;
    }

}
