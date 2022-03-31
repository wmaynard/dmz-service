using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Controllers
{
    [Authorize]
    public class PlayerController : PlatformController
    {
#pragma warning disable CS0649
        private ApiService _apiService;
        private DynamicConfigService _dynamicConfigService;
#pragma warning restore CS0649
        
        public readonly HttpClient client = new HttpClient(); // should be used for all

        public async Task<IActionResult> Search(string query)
        {
            ViewData["Message"] = "Player search";

            if (query != null)
            {
                List<string> searchId = new List<string>();
                List<string> searchUser = new List<string>();

                string requestUrl = "https://dev.nonprod.tower.cdrentertainment.com/player/v2/admin/search?term=" + query;
                
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
                
                // string token = PlatformEnvironment.Require("PLAYER_TOKEN");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                searchId.Add("id");
                searchId.Add("id2");
                searchUser.Add("user");
                searchUser.Add("user2");

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

                ViewData["Response"] = response.JSON;
                
                return View();
            }
            ViewData["Data"] = new List<List<string>>();
            return View();
        }
        
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

        public override ActionResult HealthCheck() => Ok();
    }
}