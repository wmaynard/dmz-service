using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Controllers;

[ApiController, Route(template: "portal")]
public class TopController  :  PlatformController
{
  // health handled by platform controller?
}