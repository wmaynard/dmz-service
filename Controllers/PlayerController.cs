using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace TowerPortal.Controllers;

[Route("portal/player"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PlayerController : PortalController
{
    [HttpGet, Route("search")]
    public ActionResult Search()
    {
        Require(Permissions.Player.Search);

        return Forward("/player/v2/admin/search");
    }
    
    [HttpGet, Route("details")]
    public ActionResult Details()
    {
        Require(Permissions.Player.View_Page);

        return Forward("/player/v2/admin/details");
    }
}