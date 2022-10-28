using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace Dmz.Controllers;

[Route("dmz/calendar"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class CalendarController : DmzController
{
	[HttpGet, Route("events")]
	public ActionResult Events()
	{
		Require(Permissions.Calendar.View_Page);

		return Forward("/calendar/events");
	}
}