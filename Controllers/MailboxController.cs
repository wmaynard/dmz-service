using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;

namespace TowerPortal.Controllers;

[Route("portal/mailbox"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class MailboxController : PortalController
{
  // Gets all global messages
  [HttpGet, Route("global")]
  public ActionResult Global()
  {
    Require(Permissions.Mail.View_Page);

    return Forward("/mail/admin/global/messages");
  }

  // Sends global message
  [HttpPost, Route("global/send")]
  public ActionResult GlobalSend()
  {
    Require(Permissions.Mail.Send_Global_Messages);

    return Forward("/mail/admin/global/messages/send");
  }

  // Edits global message
  [HttpPatch, Route("global/edit")]
  public ActionResult GlobalEdit()
  {
    Require(Permissions.Mail.Send_Global_Messages);

    return Forward("/mail/admin/global/messages/edit");
  }

  // Expires global message - service does not actually delete
  [HttpPatch, Route("global/delete")]
  public ActionResult GlobalDelete()
  {
    Require(Permissions.Mail.Delete_Global_Messages);

    return Forward("/mail/admin/global/messages/expire");
  }

  // Sends direct messages
  [HttpPost, Route("direct/send")]
  public ActionResult DirectSend()
  {
    Require(Permissions.Mail.Send_Direct_Messages);

    return Forward("/mail/admin/messages/send");
  }

  // Gets inbox for a player
  // TODO should be changed to GET
  [HttpPost, Route("inbox")]
  public ActionResult Inbox()
  {
    Require(Permissions.Mail.View_Page);

    return Forward("/mail/admin/inbox");
  }
}
