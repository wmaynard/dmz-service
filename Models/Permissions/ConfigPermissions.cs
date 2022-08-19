namespace TowerPortal.Models.Permissions;

public class ConfigPermissions : PermissionGroup
{
    public override string Name => "Config";
    
    public bool Manage { get; set; }
}