using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using TowerPortal.Models;

namespace TowerPortal.Controllers;

[Authorize]
[Route("portal/mailbox")]
public class MailboxController : PlatformController
{
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;

    [Route("global")]
    public async Task<IActionResult> Global()
    {
        string token = _dynamicConfigService.GameConfig.Require<string>("mailServiceToken");
        
        _apiService
            .Request($"https://dev.nonprod.tower.cdrentertainment.com/mail/admin/global/messages")
            .AddAuthorization(token)
            .OnSuccess(((sender, apiResponse) =>
            {
                Log.Local(Owner.Nathan, "Request to mailbox-service succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                Log.Error(Owner.Nathan, "Request to mailbox-service failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Get(out GenericData response, out int code);

        string responseString = response.JSON;

        GlobalMessageResponse globalMessageResponse =
            JsonConvert.DeserializeObject<GlobalMessageResponse>(responseString);

        List<List<string>> globalMessages = new List<List<string>>();
        
        

        ViewData["GlobalMessages"] = globalMessages;
        
        return View();
    }
    
    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}