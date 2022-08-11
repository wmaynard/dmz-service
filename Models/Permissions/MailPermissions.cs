namespace TowerPortal.Models.Permissions;

public class MailPermissions : PermissionGroup
{
    public override string Name => "Mail Service";

    public bool SendDirectMessages { get; set; } // TODO: Implement
    public bool SendGlobalMessages { get; set; } // TODO: Implement
}