using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Web;
using TowerPortal.Enums;
using TowerPortal.Exceptions;
using TowerPortal.Extensions;
using TowerPortal.Interfaces;
using TowerPortal.Models;
using TowerPortal.Models.Permissions;
using TowerPortal.Utilities;
using TowerPortal.Views.Shared;

namespace TowerPortal.Controllers;

public abstract class PortalController : PlatformController, IStatusMessageProvider
{
#region IStatusMessageProvider Properties
    public bool WasSuccessful => TempData.WasSuccessful();
    public string StatusMessage => TempData.GetStatusMessage();
    public RequestStatus Status => TempData.GetStatus();
    public void SetStatus(string message, RequestStatus status) => TempData.SetStatusMessage(message, status);
    public void ClearStatus() => TempData.SetStatusMessage(null, RequestStatus.None);
#endregion

    protected Passport Permissions => (Passport)ViewData[PortalView.KEY_PERMISSIONS] ?? new Passport();

    protected void Require(params bool[] permissions)
    {
        if (!permissions.Any(_bool => !_bool))
            return;
        
        throw new PermissionInvalidException();
    }
}