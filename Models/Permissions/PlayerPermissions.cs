namespace TowerPortal.Models.Permissions;

public class PlayerPermissions : PermissionGroup
{
    public override string Name => "Player Service";
    
    public bool Search          { get; set; }
    public bool Screenname      { get; set; }
    public bool Add_Currency    { get; set; }
    public bool Remove_Currency { get; set; }
}