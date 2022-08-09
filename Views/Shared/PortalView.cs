using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using TowerPortal.Enums;
using TowerPortal.Exceptions;
using TowerPortal.Extensions;
using TowerPortal.Interfaces;
using TowerPortal.Models.Permissions;
using TowerPortal.Utilities;

namespace TowerPortal.Views.Shared;

public class PortalView : RazorPage<object>, IStatusMessageProvider
{
#region IStatusMessageProvider Properties
    public bool WasSuccessful => TempData.WasSuccessful();
    public string StatusMessage => TempData.GetStatusMessage();
    public RequestStatus Status => TempData.GetStatus();
    public void SetStatus(string message, RequestStatus status) => TempData.SetStatusMessage(message, status);
    public void ClearStatus() => TempData.SetStatusMessage(null, RequestStatus.None);
#endregion
    
    public const string KEY_PERMISSIONS = "Permissions";
    protected Passport Permissions => (Passport)ViewData[KEY_PERMISSIONS] ?? new Passport();
    public override Task ExecuteAsync() => Task.CompletedTask;

    public void Require(params bool[] permissions)
    {
        if (permissions.Any(boolean => !boolean))
            throw new PermissionInvalidException();
    }
}