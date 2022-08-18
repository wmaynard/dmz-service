using MongoDB.Bson.Serialization.Attributes;

namespace TowerPortal.Models.Permissions;

[BsonIgnoreExtraElements]
public class NftPermissions : PermissionGroup
{
    public override string Name => "NFT";
}