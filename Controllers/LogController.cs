using Dmz.Models.Portal;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Enums;
using Rumble.Platform.Common.Utilities;
// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("/dmz/log")]
public class LogController : DmzController
{
    [HttpPost, Route("critical")]
    public ActionResult Critical()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Critical(Owner.Will, log.Message, log.Data);
        return Ok();
    }

    [HttpPost, Route("error")]
    public ActionResult Error()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Error(Owner.Will, message: log.Message, data: log.Data);
        return Ok();
    }

    [HttpPost, Route("info")]
    public ActionResult Info()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Info(Owner.Will, log.Message, log.Data);
        return Ok();
    }

    [HttpPost, Route("warn")]
    public ActionResult Warning()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Warn(Owner.Will, log.Message, log.Data);
        return Ok();
    }
    
    [HttpPost, Route("verbose")]
    public ActionResult Verbose()
    {
        FrontendLog log = Require<FrontendLog>("log");
        
        Log.Verbose(Owner.Will, log.Message, log.Data);
        return Ok();
    }
}