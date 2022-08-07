using System;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;

namespace TowerPortal.Models.Permissions;


// [BsonIgnoreExtraElements]
// public class Permissions : PlatformDataModel
// {
//     
// #region SPECIAL
//     public bool Admin { get; set; }
//     public bool ManagePermissions { get; set; }
// #endregion
//     
// #region VIEW
//     public bool Config_View_Service { get; set; }
//     public bool Mail_View_Service { get; set; }
//     public bool Player_View_Service { get; set; }
//     public bool Token_View_Service { get; set; }
// #endregion
//     
// #region EDIT
//     // TODO: Permissions should be granular where it makes sense to do so.
//     // e.g. Mail_SendGlobalMessage, Mail_SendDirectMessage, Config
//     // This will be more important for other services, such as editing player records.
//     public bool Config_EditData { get; set; }
//     public bool Mail_SendMessages { get; set; }
//     public bool Player_EditData { get; set; }
//     public bool Token_EditData { get; set; }
// #endregion
//
//     public void SetAdmin()
//     {
//         Admin = true;
//         ManagePermissions = true;
//     }
//
//     public void SetUser()
//     {
//         Player_View_Service = true;
//         Mail_View_Service = true;
//         Token_View_Service = true;
//         Config_View_Service = true;
//     }
//
//     // Until we have group-level permissions, these can be used as a band-aid.
//     public static readonly Permissions SUPERUSER = InitializeSuperuser();
//     public static readonly Permissions READONLY = InitializeReadonly();
//     public static readonly Permissions EXTERNAL_QA_USER = READONLY;
//     public static readonly Permissions RUMBLE_STANDARD_USER = READONLY;
//     public static readonly Permissions RUMBLE_PLATFORM_USER = PlatformEnvironment.IsProd
//         ? READONLY
//         : SUPERUSER;
//
//         
//     private static Permissions InitializeReadonly()
//     {
//         Permissions output = new Permissions();
//         foreach (PropertyInfo info in typeof(Permissions).GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
//             if (info.PropertyType.IsAssignableTo(typeof(bool)))
//                 info.SetValue(obj: output, value: info.Name.Contains("_View"));
//
//         return output;
//     }
//     
//     // This will automatically grant all permissions, even newly added ones, so long as permissions are boolean properties.
//     private static Permissions InitializeSuperuser()
//     {
//         Permissions output = new Permissions();
//         foreach (PropertyInfo info in typeof(Permissions).GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
//             if (info.PropertyType.IsAssignableTo(typeof(bool)))
//                 info.SetValue(obj: output, value: true);
//         
//         return output;
//     }
// }