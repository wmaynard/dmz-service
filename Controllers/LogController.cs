using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Models;
using TowerPortal.Models.Portal;

namespace TowerPortal.Controllers;

[Route("/portal/log")]
public class LogController : PortalController
{
    [HttpPost, Route("critical")]
    public ActionResult Critical()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Critical(Owner.David, log.Message, log.Data);
        return Ok();
    }

    [HttpPost, Route("error")]
    public ActionResult Error()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Error(Owner.David, message: log.Message, data: log.Data);
        return Ok();
    }

    [HttpPost, Route("info")]
    public ActionResult Info()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Info(Owner.David, log.Message, log.Data);
        return Ok();
    }

    [HttpPost, Route("warn")]
    public ActionResult Warning()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Warn(Owner.David, log.Message, log.Data);
        return Ok();
    }
    
    [HttpPost, Route("verbose")]
    public ActionResult Verbose()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Verbose(Owner.David, log.Message, log.Data);
        return Ok();
    }
}