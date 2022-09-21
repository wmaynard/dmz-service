using Dmz.Models;
using Dmz.Services;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;

namespace Dmz.Controllers;

[Route("dmz/otp"), RequireAuth]
public class OtpController : PlatformController
{
#pragma warning disable
    private readonly OtpService _otpService;
#pragma warning restore
    
    [HttpPost, Route("token")]
    public ActionResult StoreValue() => Ok(new GenericData
    {
        { "otp", _otpService.Store(new StoredValue
        {
            Value = Token.Authorization,
            AccountId = Token.AccountId
        })}
    });

    [HttpDelete, NoAuth]
    public ActionResult Retrieve()
    {
        string otp = Require<string>("id");

        return Ok(new GenericData
        {
            { "value", _otpService.Retrieve(otp).Value }
        });
    }
}