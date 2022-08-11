using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TowerPortal.Controllers;
using TowerPortal.Models;
using TowerPortal.Models.Permissions;
using TowerPortal.Views.Shared;

namespace TowerPortal.Extensions;

public static class ViewDataExtension
{
    // This gives you the ability to use more readable code:
    // ViewData.GetPermissions().{NAME};
    // ViewData.SetPermissions(value);
    // instead of
    // ((Permissions)ViewData["Permissions"]).{NAME};
    // ViewData["Permissions"] = value;
    // This also has the benefit of letting you set more complicated keys without having to re-type them / remember them all the time.

    public static void SetPermissions(this PortalController controller, Passport permissions) => controller.ViewData[PortalView.KEY_PERMISSIONS] = permissions;

    // This is just for another example of an extension method in case you want to write more.
    // This lets you typecast in a more readable way.
    // e.g. ViewData.Get<bool>("someBoolValue")
    // e.g. ViewData.Get<Permissions>("Permissions")
    public static T Get<T>(this ViewDataDictionary viewData, string key) => (T)viewData[key];
}