using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;

namespace Dmz.Controllers;

[Route("dmz/calendar"), RequireAuth()]
public class CalendarController : DmzController
{
	[HttpGet, Route("events")]
	public ActionResult Events()
	{
		Require(Permissions.Calendar.View_Page);

		return Forward("/calendar/events");
	}
}