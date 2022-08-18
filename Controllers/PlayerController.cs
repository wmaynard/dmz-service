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
    public ActionResult Search(string query)
    {
        Require(Permissions.Player.Search);

        return Forward("/player/v2/admin/search");
    }
    
    [Route("details")]
    public async Task<IActionResult> Details(string id)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("editScreenname")]
    public async Task<IActionResult> EditScreenname(string accountId, string editScreenname)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    [Route("EditWallet")]
    public async Task<IActionResult> EditWallet(IFormCollection collection)
    {
        throw new NotImplementedException();
    }
}