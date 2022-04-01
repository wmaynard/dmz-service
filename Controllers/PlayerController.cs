using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
[Route("player")]
public class PlayerController : PlatformController
{
#pragma warning disable CS0649
    private readonly ApiService _apiService;
    private readonly DynamicConfigService _dynamicConfigService;
#pragma warning restore CS0649
    
    public readonly HttpClient client = new HttpClient(); // should be used for all

    [Route("search")]
    // [Route("search/{query=query}")]
    public async Task<IActionResult> Search(string query)
    {
        ViewData["Message"] = "Player search";

        if (query != null)
        {
            List<string> searchId = new List<string>();
            List<string> searchUser = new List<string>();

            string token = _dynamicConfigService.GameConfig.Require<string>("playerServiceToken");

            // Use the API Service to simplify web requests
            _apiService
                .Request($"https://dev.nonprod.tower.cdrentertainment.com/player/v2/admin/search?term={query}")
                .AddAuthorization(token)
                .OnSuccess(((sender, apiResponse) =>
                {
                    Log.Local(Owner.Nathan, "Request to player-service-v2 succeeded.");
                }))
                .OnFailure(((sender, apiResponse) =>
                {
                    Log.Error(Owner.Nathan, "Request to player-service-v2 failed.", data: new
                    {
                        Response = apiResponse
                    });
                }))
                .Get(out GenericData response, out int code);
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string responseString = response.JSON;

            SearchResponse searchResponse = JsonConvert.DeserializeObject<SearchResponse>(responseString);

            foreach (SearchResult result in searchResponse.Results)
            {
                searchId.Add(result.Player.Id);
                searchUser.Add(result.Player.Username);
            }
            
            List<List<string>> searchList = new List<List<string>>();
            for (int i = 0; i < searchId.Count; i++)
            {
                List<string> searchEntry = new List<string>();
                searchEntry.Add(searchId[i]);
                searchEntry.Add(searchUser[i]);
                
                searchList.Add(searchEntry);
            }

            ViewData["Query"] = query;
            ViewData["Data"] = searchList;
            
            return View();
        }
        ViewData["Data"] = new List<List<string>>();
        return View();
    }
    
    [Route("details")]
    public async Task<IActionResult> Details(string id)
    {
        string requestUrl = "https://dev.nonprod.tower.cdrentertainment.com/player/v2/admin/details?accountId=" + id;

        string token = _dynamicConfigService.GameConfig.Require<string>("playerServiceToken");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        _apiService
            .Request(requestUrl)
            .AddAuthorization(token)
            .OnSuccess(((sender, apiResponse) =>
            {
                Log.Local(Owner.Nathan, "Request to player-service-v2 details succeeded.");
            }))
            .OnFailure(((sender, apiResponse) =>
            {
                Log.Error(Owner.Nathan, "Request to player-service-v2 details failed.", data: new
                {
                    Response = apiResponse
                });
            }))
            .Get(out GenericData response, out int code);

        ViewData["accountId"] = id;
        ViewData["Response"] = response;

        return View();
    }

    [Route("health")]
    public override ActionResult HealthCheck() => Ok(_apiService.HealthCheckResponseObject, _dynamicConfigService.HealthCheckResponseObject);
}