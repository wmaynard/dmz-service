# Managing Permissions

## Introduction

Permissions are an incredibly important feature of the Portal.  Permissions allow granular control over how users interact with our system, from viewing navigation to updating player records directly.  Because this is an interface for all of Platform services, there are some incredibly sensitive areas of the website.  Consequently, we need to lock down as much as we can.

This document will walk you through the basics of how the permissions work in Portal as well as proper maintenance for them.

## Glossary

|          Term | Definition                                                                                                                                                                                                                 |
|--------------:|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|        Filter | For the purposes of this document, this term refers to, specifically, the piece of code that runs before any endpoint is hit, in the `ViewDataFilter`.  The Filter prepares the Passport for use by controllers and views. |
|         Group | A full permission set for a Platform project.  Every Platform project has its own group, including Portal.                                                                                                                 |
|      Passport | A complete collection of all of a user's permissions.                                                                                                                                                                      |
|    Permission | A boolean value indicating whether or not a user has access to a particular resource.  Permissions should be named in an English-friendly manner such that the code almost reads as if spoken.                             |
| Readonly User | Someone who can only view data, but not make any direct changes to any aspect of Platform projects.                                                                                                                        |

## The `PermissionGroup` Class

Every Group is an implementation of the abstract class `PermissionGroup`.  This base class does three things:

1. Guarantee that developers give each group a name.  This name is used when managing permissions to create sections.
2. Provide a basic permission for viewing the navigation buttons.
3. Provide a basic permission for viewing the landing page of a service.
4. Provide a basic permission for edit access for the entirety of the landing page*

The edit permission is from the first iteration of permissions and will be removed at a future date.  Permissions should be more granular than that.  Say a player changes their name to something offensive and we have a full staff of community managers who are responsible for keeping Chat clean.  We want to allow them to forcibly rename the player, but it may not make sense to allow them to edit other parts of the player record, such as removing currency.

The real power of the Group is that we can add as many permissions as we need without cluttering a monolithic Permission class (as was the case with V1).  For the above scenario, there could be permissions such as:

* `Passport.Player.CanRename`
* `Passport.Player.CanSubtractCurrency`
* `Passport.Player.CanAddCurrency`

Note that these are just theoretical values, but should be illustrative of how organized we can keep permissions moving forward.

### Naming

* Write Permissions should be named in such a way that they read easily, almost as if someone was speaking them.
* Read Permissions should always **start** with `View`.  This is a limitation of the way Portal automatically assigns readonly users.

## Controller Usage

As a prerequisite, every controller needs to inherit from `PortalController`.  So long as that's been done, you can access the current user's Passport with one of the three following methods:

1. `Permissions.{Project}.{Permission}`, e.g. `Permissions.Player.View_Page`
2. **(Preferred)** `Require(Permissions.Player.Edit, Permissions.Player.CanRename);`
3. `RequireOneOf(Permissions.Player.Edit, Permissions.Player.CanRename);`

This second method will throw a `PermissionInvalidException` if a user is not authorized to access a particular resource.  If **any** boolean passed into the `Require()` method is false, the exception will cause the user to be redirected to a 403 page.

## View Usage

Similar to controllers, views need to inherit from `PortalView`.  Unlike the regular C# inheritance, views are extended by adding the following to the top of the code file:

```
@inherits TowerPortal.Views.Shared.PortalView
```

After that, usage is the same as the controller usage, though the preferred usage is situational.

<hr />
Every page's codefile should begin with a call to `Require()` or `RequireOneOf()`; this will guarantee that users are authenticated and have the permissions necessary to view the page in the first place.  Consider the following example:

```
@inherits TowerPortal.Views.Shared.PortalView
@using Microsoft.AspNetCore.Mvc.TagHelpers
@using TowerPortal.Models.Permissions

@{ Require(Permissions.Portal.ManagePermissions); }
...
```

Here we can easily see that to access this page, the current user _must_ have the ability to `ManagePermissions`.  If they lack this permission, they will be redirected to the error page.

<hr />

For conditional rendering, the usage should rely on directly accessing properties instead of the `Require()` methods, since a failed `Require()` will redirect the user to the unauthorized page.

```
@if (Permissions.Player.CanRename)
{
    <div class="foobar">
        /* Forms to rename the player go here */
    </div>
}
```

## Adding New Groups

Whenever Platform embarks on a new project that has any integration with Portal, we will have to implement a new group for that project.

To add a new `PermissionGroup`:
1. Create a new class in `Models.Permissions`.  The naming should be `{ProjectName}Permissions`.
2. Extend the `PermissionGroup` class.  This will involve implementing required permissions.
3. In `Models.Permission.Passport.cs`, add a new getter property:
```
    public {Name}Permissions {Name} => Fetch<{Name}Permissions>();
```

The group can then be used as any other group.

## Default Permission Sets

For now, creating default permission sets happens in `Models.Permission.Passport.cs`.  For any custom permission set you want to add, two things need to happen:

1. You expand one of the readonly string arrays at the top of the file, or create a new one to describe the new role.
2. You modify `Passport.GetDefaultPermissions()` if necessary.

For example, if we want to create a new group that has only readonly permissions by default, but can rename players, we could do:

```
private static readonly string[] RENAME_PLAYERS_ONLY =
{
    "joemcfugal@domain.com"
}
...
public static Passport GetDefaultPermissions(string email)
{
    if (SUPERUSERS.Contains(email))
        return new Passport(PassportType.Superuser);
    if (RENAME_PLAYERS_ONLY.Contains(email))
    {
        Passport output = new Passport(PassportType.Readonly);
        output.Player.CanRename = true;
        return output;
    }
    if (READONLY_DOMAINS.Any(email.EndsWith))
        return new Passport(PassportType.Readonly);
    return new Passport(PassportType.Unauthorized);
}
```

## Managing Allowed Domains

As alluded to above, all default permissions are granted through the `Passport.GetDefaultPermissions()` method.  If a domain appears in one of the default permission set arrays, it will be allowed into the Portal in at least _some_ capacity.  If the email doesn't match any pattern, a `PermissionInvalidException` will be thrown, redirecting the user to the error page.  No data will be inserted into Mongo.

## Dev vs. Prod

Production environments necessitate more restrictive permissions.  We need to be extremely careful with who can interface with our services when user data can be affected.  As permissions expand, keep this in mind.  Leverage boolean properties such as `PlatformEnvironment.IsProd` to create different Passports specifically for prod for custom roles.

## Future Enhancement: Roles

Roles will allow a more easily configurable experience for user management.  Roles will be managed via the Portal.

* A `Role` will be an enum with the `Flags` attribute.  Roles will correspond to a specific Passport.  
* On startup, a comprehensive `Dictionary<Role, Passport>` will be created.  Every possible combination of `Role` flags will be available in the dictionary.
* Accounts will be able to be assigned a `Role` via flags, e.g.

```
account.Role = Role.CustomerService | Role.ChatAdmin | Role.DevSuperuser;
```
* Whenever a Passport is read from Mongo, the account will compare the Passport permissions with the Role permissions.  The account Passport will be merged with the corresponding Role's Passport via the dictionary.  If any values are changed, the account Passport will be updated on Mongo.
    * When merging, the most permissive value is used.  So if the account lacks permission to `Player.CanRename` and contains six roles, of which only one has access to `Player.CanRename`, their resultant Passport will have access to `Player.CanRename`.