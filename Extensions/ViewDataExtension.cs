using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TowerPortal.Models;

namespace TowerPortal.Extensions;

public static class ViewDataExtension
{
	private const string PERMISSIONS = "Permissions";
	
	// This gives you the ability to use more readable code:
	// ViewData.GetPermissions().{NAME};
	// ViewData.SetPermissions(value);
	// instead of
	// ((Permissions)ViewData["Permissions"]).{NAME};
	// ViewData["Permissions"] = value;
	// This also has the benefit of letting you set more complicated keys without having to re-type them / remember them all the time.
	public static Permissions GetPermissions(this ViewDataDictionary viewData) => (Permissions)viewData[PERMISSIONS];

	public static void SetPermissions(this ViewDataDictionary viewData, Permissions permissions) => viewData[PERMISSIONS] = permissions;

	// This is just for another example of an extension method in case you want to write more.
	// This lets you typecast in a more readable way.
	// e.g. ViewData.Get<bool>("someBoolValue")
	// e.g. ViewData.Get<Permissions>("Permissions")
	public static T Get<T>(this ViewDataDictionary viewData, string key) => (T)viewData[key];

}