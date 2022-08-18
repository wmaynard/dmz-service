namespace TowerPortal.Models.Permissions;

public class PlayerPermissions : PermissionGroup
{
    public override string Name => "Player Service";
    
    public bool Search { get; set; }
}