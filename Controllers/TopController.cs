using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Web;

namespace Dmz.Controllers;

[ApiController, Route(template: "dmz")]
public class TopController : PlatformController
{   
    // health handled by platform controller
}