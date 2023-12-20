using System;
using System.Linq;
using Dmz.Exceptions;
using Dmz.Models.Permissions;
using Dmz.Utilities;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Web;
using Dmz.Extensions;
using RCL.Logging;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

// ReSharper disable InconsistentNaming

namespace Dmz.Controllers;

public abstract class DmzController : PlatformController
{
    protected static Passport Permissions => ContextHelper.Passport
        ?? throw new PermissionInvalidException();

    protected static bool Require(params bool[] permissions) => permissions.Any(_bool => !_bool)
        ? throw new PermissionInvalidException()
        : true;

    protected static bool RequireOneOf(params bool[] permissions) => permissions.Any(boolean => boolean)
        ? true
        : throw new PermissionInvalidException();

    protected ActionResult Forward(string url, bool asAdmin = false) => Ok(data: _apiService.Forward(url, asAdmin: asAdmin));
    
    protected ActionResult Forward(string url, out RumbleJson response)
    {
        try
        {
            response = _apiService.Forward(url);
        }
        catch (UriFormatException e)
        {
            Log.Error(Owner.Default, "Bad URL prevented DMZ from forwarding a request.", data: new
            {
                url = url
            }, exception: e);
            throw;
        }
        
        return Ok(data: response);
    }

    public new OkObjectResult Ok() => Ok(new RumbleJson());
    public override OkObjectResult Ok(object value) => base.Ok(value ?? new RumbleJson());
}