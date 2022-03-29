using System;
using System.Collections.Generic;
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

namespace tower_admin_portal.Controllers
{
    [Authorize]
    public class PlayerController : Controller
    {
// #pragma warning disable CS0169
        private ApiService _apiService;
        private DynamicConfigService _dynamicConfigService;
// #pragma warning restore CS0169
        
        public readonly HttpClient client = new HttpClient(); // should be used for all

        public PlayerController(ApiService api, DynamicConfigService dynamicConfig)
        {
            _apiService = api;
            _dynamicConfigService = dynamicConfig; // Dynamic config service may be changing soon, or rewritten as part of portal, TBD.
        }
        
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

                ViewData["Response"] = response;
                
                return View();
            }
            ViewData["Data"] = new List<List<string>>();
            return View();
        }
        
        public async Task<IActionResult> Details(string id)
        {
            string requestUrl = "https://dev.nonprod.tower.cdrentertainment.com/player/v2/admin/details?accountId=" + id;

            object response = null;
                
            string token = PlatformEnvironment.Require("PLAYER_TOKEN");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
            HttpResponseMessage httpResponse = await client.GetAsync(requestUrl);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                string responseBody = await httpResponse.Content.ReadAsStringAsync();
                response = JsonConvert.DeserializeObject<object>(responseBody);
            }
            else
            {
                // Log.Error(owner: Owner.Nathan, message: "Request to player service v2 failed.", data: $"Response code {httpResponse.StatusCode}");
                response = httpResponse.StatusCode;
            }

            ViewData["accountId"] = id;
            ViewData["Response"] = response;

            return View();
        }

        public ActionResult HealthCheck()
        {
            return Ok();
        }
    }
}