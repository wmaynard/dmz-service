using Dmz.Extensions;
using Dmz.Models.Permissions;
using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/chat"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class ChatController : DmzController
{
    [HttpPost, Route("messages/broadcast")]
    public ActionResult Broadcast()
    {
        Require(Permissions.Chat.SendAnnouncements);

        return Forward("chat/admin/broadcast");
    }

    [HttpPut, Route("messages/update")]
    public ActionResult EditMessage()
    {
        Require(Permissions.Chat.EditMessages);

        return Forward("chat/admin/messages/update");
    }

    [HttpDelete, Route("messages/delete")]
    public ActionResult DeleteMessage()
    {
        Require(Permissions.Chat.DeleteMessages);

        return Forward("chat/admin/messages/delete");
    }

    [HttpGet, Route("messages/search")]
    public ActionResult SearchMessages()
    {
        Require(Permissions.Chat.ViewMessages);

        return Forward("chat/admin/messages/search");
    }

    [HttpGet, Route("reports")]
    public ActionResult GetReports()
    {
        Require(Permissions.Chat.ViewReports);

        return Forward("chat/admin/reports");
    }

    [HttpPatch, Route("reports/update")]
    public ActionResult UpdateReport()
    {
        Require(Permissions.Chat.EditReports);

        return Forward("chat/reports/update");
    }

    [HttpGet, Route("rooms")]
    public ActionResult ListRooms()
    {
        Require(Permissions.Chat.ViewRooms);

        return Forward("chat/rooms");
    }

    [HttpPatch, Route("rooms/update")]
    public ActionResult UpdateRoom()
    {
        Require(Permissions.Chat.EditRooms);

        return Forward("chat/rooms/update");
    }
}