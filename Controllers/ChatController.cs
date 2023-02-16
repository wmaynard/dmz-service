using Dmz.Extensions;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/chat"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class ChatController : DmzController
{
    #region Announcements
    // Gets all announcements
    [HttpGet, Route("announcements")]
    public ActionResult Announcements()
    {
        Require(Permissions.Chat.View);

        return Forward("/chat/admin/messages/sticky");
    }

    // Send a chat announcement
    [HttpPost, Route("announcements/send")]
    public ActionResult AnnouncementsSend()
    {
        Require(Permissions.Chat.Send_Announcements);

        return Forward("/chat/admin/messages/sticky");
    }
    
    // Edit a Chat announcement
    [HttpPatch, Route("announcements/edit")]
    public ActionResult AnnouncementsEdit()
    {
        Require(Permissions.Chat.Send_Announcements);

        return Forward(""); // TODO fill out when endpoint is implemented
    }

    // Deletes a chat announcement
    // TODO should be changed to DELETE
    [HttpPost, Route("announcements/delete")]
    public ActionResult AnnouncementsDelete()
    {
        Require(Permissions.Chat.Delete_Announcements);

        return Forward("/chat/admin/messages/unsticky");
    }
    #endregion
  
    #region Player lookup
    // Gets player specific chat reports and bans
    [HttpGet, Route("player")]
    public ActionResult Player()
    {
        Require(Permissions.Chat.View);

        return Forward("/chat/admin/playerDetails");
    }
    #endregion
  
    #region Reports
    // Gets all reports
    [HttpGet, Route("reports")]
    public ActionResult Reports()
    {
        Require(Permissions.Chat.View);

        return Forward("/chat/admin/reports/list");
    }
  
    // Ignores a report for a player
    [HttpPost, Route("reports/ignore")]
    public ActionResult ReportsIgnore()
    {
        Require(Permissions.Chat.Ignore_Reports);

        return Forward("/chat/admin/reports/ignore");
    }
  
    // Deletes a report for a player
    // TODO should be changed to DELETE
    [HttpPost, Route("reports/delete")]
    public ActionResult ReportsDelete()
    {
        Require(Permissions.Chat.Delete_Reports);

        return Forward("/chat/admin/reports/delete");
    }
    #endregion
  
    #region Chat bans
    // Chat bans a player
    [HttpPost, Route("ban")]
    public ActionResult Ban()
    {
        Require(Permissions.Chat.Ban);
        
        string aid = Require<string>(key: "aid");
        _apiService.ForceRefresh(aid);

        return Forward("/chat/admin/ban/player");
    }
  
    // Chat unbans a player
    [HttpPost, Route("unban")]
    public ActionResult Unban()
    {
        Require(Permissions.Chat.Unban);

        return Forward("/chat/admin/ban/lift");
    }
    
    // List chat bans
    [HttpGet, Route("bans")]
    public ActionResult Bans()
    {
        Require(Permissions.Chat.View);

        return Forward("/chat/admin/ban/list");
    }
    #endregion
}