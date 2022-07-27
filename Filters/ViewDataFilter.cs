using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RCL.Logging;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Controllers;
using TowerPortal.Extensions;
using TowerPortal.Models;
using TowerPortal.Services;

namespace TowerPortal.Filters;

public class ViewDataFilter : PlatformFilter, IActionFilter
{
	/// <summary>
	/// This executes before every single controller endpoint.  We need to check the Google permissions
	/// for every endpoint, so rather than repeat the code in every single method, this instead allows us to remove
	/// a ton of boilerplate.
	/// </summary>
	/// <param name="context"></param>
	public void OnActionExecuting(ActionExecutingContext context)
	{
		PortalController controller = (PortalController)context.Controller;
		if (!GetService(out AccountService accountService))
		{
			Log.Local(Owner.Will, "Account Service was null, returning.");
			return;
		}

		try
		{
			Account googleAccount = Account.FromGoogleClaims(controller.User.Claims);
			Account mongoAccount = accountService.FindOne(filter: account => account.Email == googleAccount.Email);

			Log.Local(Owner.Will, "Setting the ViewData permissions object");
			Permissions permissions = accountService.CheckPermissions(mongoAccount);
			controller.ViewData.SetPermissions(permissions);
		
			// TODO: Update places 
		}
		catch (Exception e)
		{
			Log.Local(Owner.Will, "There was a problem in the filter; unable to authenticate.  May happen on health checks.", exception: e);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context) { }
}