using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using TowerPortal.Extensions;
using TowerPortal.Models.Permissions;

namespace TowerPortal.Views.Shared;

public class PortalView : RazorPage<object>
{
    public const string KEY_PERMISSIONS = "Permissions";
    protected Passport Permissions => (Passport)ViewData[KEY_PERMISSIONS] ?? new Passport();
    public override Task ExecuteAsync() => Task.CompletedTask;
}