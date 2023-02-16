using Microsoft.AspNetCore.Mvc;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Utilities;
// ReSharper disable ArrangeAttributes

namespace Dmz.Controllers;

[Route("dmz/mailbox"), RequireAuth(AuthType.ADMIN_TOKEN)]
public class MailboxController : DmzController
{
    #region Global Messages
    // Gets all global messages
    [HttpGet, Route("global")]
    public ActionResult Global()
    {
        Require(Permissions.Mail.View);

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
    [HttpPatch, Route("global/expire")]
    public ActionResult GlobalExpire()
    {
          Require(Permissions.Mail.Expire_Global_Messages);

          return Forward("/mail/admin/global/messages/expire");
    }
    #endregion

    #region Direct messages
    // Sends direct messages
    [HttpPost, Route("direct/send")]
    public ActionResult DirectSend()
    {
        Require(Permissions.Mail.Send_Direct_Messages);

        return Forward("/mail/admin/messages/send");
    }
    #endregion

    #region Player inbox
    // Gets inbox for a player
    [HttpGet, Route("inbox")]
    public ActionResult Inbox()
    {
        Require(Permissions.Mail.View);

        return Forward("/mail/admin/inbox");
    }
    
    // Edits message in player inbox
    [HttpPatch, Route("inbox/edit")]
    public ActionResult InboxEdit()
    {
        Require(Permissions.Mail.Modify_Inbox);

        return Forward("/mail/admin/messages/edit");
    }
    
    // Expires message in player inbox
    [HttpPatch, Route("inbox/expire")]
    public ActionResult InboxExpire()
    {
        Require(Permissions.Mail.Modify_Inbox);

        return Forward("/mail/admin/messages/expire");
    }
    #endregion
}
