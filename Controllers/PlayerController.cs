using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RCL.Logging;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using TowerPortal.Extensions;
using TowerPortal.Models.Player;

namespace TowerPortal.Controllers;

[Route("portal/player"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class PlayerController : PortalController
{
#pragma warning disable CS0649
    private readonly DynamicConfigService _dynamicConfigService;
#pragma warning restore CS0649

    
    #region Player lookup
    // Search for a player
    [HttpGet, Route("search")]
    public ActionResult Search()
    {
        Require(Permissions.Player.Search); // necessary? maybe change to view_page

        return Forward("/player/v2/admin/search");
    }
    
    // Get player details
    [HttpGet, Route("details")]
    public ActionResult Details()
    {
        Require(Permissions.Player.View_Page);

        return Forward("/player/v2/admin/details");
    }
    #endregion

    #region Modifying data
    // Update a player's screenname
    [HttpPatch, Route("screenname")]
    public ActionResult Screenname()
    {
        Require(Permissions.Player.Screenname);

        return Forward("/player/v2/admin/screenname");
    }
    
    // Unlink account
    [HttpDelete, Route("unlink")]
    public ActionResult Unlink()
    {
        Require(Permissions.Player.Unlink_Accounts);

        return Forward("/player/v2/admin/profiles/unlink");
    }
    
    // Requires the whole player component with field modified, as well as version += 1
    [HttpPatch, Route("update")]
    public ActionResult Update()
    {
        Require(Permissions.Player.Update);

        return Forward("/player/v2/admin/component");
    }
    
    // Temporary wallet update before player service is updated
    [HttpPatch, Route("update/wallet")]
    public ActionResult UpdateWallet()
    {
        Require(Permissions.Player.Update);

        string aid = Require<string>(key: "aid");
        List<WalletCurrency> currencies = Require<List<WalletCurrency>>(key: "currencies");
        
        _apiService
            .Request(PlatformEnvironment.Url("/player/v2/admin/details"))
            .AddParameter("accountId", aid)
            .AddAuthorization(_dynamicConfigService.GameConfig.Require<string>("playerServiceToken"))
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
            .Get(out GenericData response, out int tempCode);
        
        PlayerComponents component;

        try
        {
            component = response.Require<PlayerComponents>(key: "components");
        }
        catch (Exception e)
        {
            Log.Error(owner: Owner.Nathan, message: "Failed to parse response from player-service.", data: e);
            throw new PlatformException(message: "Failed to parse response from player-service.");
        }

        if (component == null)
        {
            Log.Error(owner: Owner.Nathan, message: "Error occurred when attempting to update player components.", data:"Components was null when passed from player details to player wallets.");
            throw new PlatformException(message: "Error occurred when attempting to update player components.");
        }

        List<WalletCurrency> newWalletCurrencies = new List<WalletCurrency>();

        foreach (WalletCurrency oldCurrency in component.Wallet.Data.Currencies)
        {
            newWalletCurrencies.Add(currencies.FirstOrDefault(currency => currency.CurrencyId == oldCurrency.CurrencyId) ?? oldCurrency);
        }

        component.Wallet.Data.Currencies = newWalletCurrencies;
        component.Wallet.Version += 1;

        return Ok(data: _apiService.Forward(url: "/player/v2/admin/component", payload: new GenericData
                                                                                        {
                                                                                            {"component", component.Wallet}
                                                                                        }
                                           )
                 );
    }
    #endregion
}