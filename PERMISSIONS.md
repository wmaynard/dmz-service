# Managing Permissions

## Introduction

Permissions are an incredibly important feature of the Portal.  Permissions allow granular control over how users interact with our system, from viewing navigation to updating player records directly.  Because this is an interface for all of Platform services, there are some incredibly sensitive areas of the website.  Consequently, we need to lock down as much as we can.

This document will walk you through the basics of how the permissions work in Portal as well as proper maintenance for them.

## Glossary

|          Term | Definition                                                                                                                                                                                                                    |
|--------------:|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|        Filter | For the purposes of this document, this term refers to, specifically, the piece of code that runs before any endpoint is hit, in the `PermissionsFilter`.  The Filter prepares the Passport for use by controllers and views. |
|         Group | A full permission set for a Platform project.  Every Platform project has its own group, including Portal (DMZ).                                                                                                              |
|      Passport | A complete collection of all of a user's permissions.  A Passport represents the merged values of an account's individual Permissions and the Permissions granted to it by Roles.                                             |
|    Permission | A boolean value indicating whether or not a user has access to a particular resource.  Permissions should be named in an English-friendly manner such that the code almost reads as if spoken.                                |
|          Role | A defined set of Permissions.  Roles have all the same keys that a Player's permission set does.                                                                                                                              |
| Readonly User | Someone who can only view data, but not make any direct changes to any aspect of Platform projects.                                                                                                                           |

## Use in C# Controllers

The #1 benefit we get from DMZ is that we can stack an extra, more granular auth layer on every admin function we have in all of Platform.  In most of our codebase, we typically only care about whether or not an access token is an administrator or not.  This is great for Platform developers, but not great when we're opening those same functions to other internal users - **especially** when we're discussing production user data.

### Use Case: Different Levels of Customer Service

Consider the needs of CS.  Let's assume that we have a product that's been released at scale.  We have millions of users and dozens of CS reps to support them.  Now think about the following actions:

* Force a username change
* Grant compensation to an individual player
* Grant compensation to all players
* Issue in-app announcements
* Ban users
* Unban users
* Merge / link accounts

This is just a very small sampling of available admin actions, but hopefully it's obvious that some of these actions are more impactful than others.  We don't want a new CS hire - untrained and, for the short term, untrusted - to be able to perform all of these actions.  But, since all of the Portal accounts have admin tokens, there's technically nothing stopping them from using our system by default.

This is where the Permissions & Roles come in to play.  In this projects `Models/Permissions` directory, we have a bunch of `PermissionGroup` classes.  Each one of these represents a Platform project and the associated permissions for it.  Let's look at one now:

```csharp
public class ChatPermissions : PermissionGroup
{
    public override string Name => "Chat Service";
  
    public bool Send_Announcements   { get; set; }
    public bool Delete_Announcements { get; set; }
    public bool Ban                  { get; set; }
    public bool Unban                { get; set; }
    public bool Ignore_Reports       { get; set; }
    public bool Delete_Reports       { get; set; }
}
```

On its face it doesn't seem like this really does much.  It's just a class with some booleans in it, right?  However, we define them this way because we have a system built on reflection in the project that translates these into a permission set for every Portal account.  **There is no need** to use these classes directly.  They just need to be defined, and for any new permission, just add a property to the target `PermissionGroup`.

If you're adding a completely new `PermissionGroup`, you need to add a new model and add a property of that type to the `Passport` model.

### How Permissions Are Enforced

DMZ typically has a controller for each Platform project it's used to interface with.  These controllers inherit from a special parent class, `DmzController`.  While you're likely familiar with `Require<T>(string)` already, DMZ Controllers are also equipped with a special `Require(bool[])` method; this allows us to check the current user's Permissions before continuing any further in our code.  They're also granted a special property, `Permissions`, that represents the requesting account's Passport.  **Every endpoint should begin with a permissions check.**

#### Example: Sending Chat Announcements

```csharp
[HttpPost, Route("announcements/send")]
public ActionResult AnnouncementsSend()
{
    Require(Permissions.Chat.Send_Announcements);

    return Forward("/chat/admin/messages/sticky");
}
```

* You'll notice the Route is different from the actual endpoint.  This is in part security through obscurity.  Portal tokens only have access to DMZ, so the tokens themselves wouldn't be able to access the _real_ endpoint the request gets forwarded to.  However, this also adds another hurdle to someone trying to get around DMZ's permission system.  Especially if a user has access to Dynamic Config, they will have access to full-fledged admin tokens that are valid on other services.
* You could easily just check `if (Permissions.Chat.Send_Announcements)` and add your own logic for what should happen if a permission is invalid.  `Require()` here is just a shorthand / standard to throw an exception when an account doesn't have access to a resource.  Stick to the standard; don't add your own logic unless you have an explicit reason to.
* No other logic is necessary in this endpoint.  DMZ is mostly an access layer to other resources; let the target service do the processing in almost all situations.

## Managing Permissions via API (Portal Web Client)

Now we get to the meat of this document: the API calls needed to manage permissions and roles.  We have six endpoints necessary for this:

* Account Level
  * GET /permissions/accounts
  * PUT /permissions/update
* Role Level
  * GET /permissions/roles
  * POST /permissions/roles/create
  * PATCH /permissions/roles/update
  * DELETE /permissions/roles/delete

<hr />

**Important:** Keep in mind that this document may be outdated.  Postman is the authoritative source for request examples and usage.  This document is primarily for concepts and may be stale by the time you're reading this.  This is particularly important for the actual permission keys themselves; more will be added in time, some may disappear, so they should just be considered simple examples.

<hr />


Let's go through them one-by-one.

### Getting an Account / Listing all Accounts

```
GET /permissions/accounts?id=deadbeefdeadbeefdeadbeef

HTTP 200
{
    "accounts": [
        {
            "name": null,
            "email": "william.maynard@rumbleentertainment.com",
            "roleIds": [
                "65824dbd2033ec419bdc9ccd",
                ...
            ],
            "roles": [
                {
                    "name": "foobar",
                    "permissions": {
                        "Calendar_Service.View": false,             // <----
                        "Chat_Service.Send_Announcements": true,    // <----
                        ...
                    },
                    "id": "65824dbd2033ec419bdc9ccd",
                    "createdOn": 1703038397
                },
                ...
            ],
            "permissions": {
                "Calendar_Service.View": true,                      // <----
                "Chat_Service.Send_Announcements": false,           // <----
                "Chat_Service.Delete_Announcements": true,
                "Chat_Service.Ban": true,
                "Chat_Service.Unban": true,
                "Chat_Service.Ignore_Reports": true,
                "Chat_Service.Delete_Reports": true,
                "Chat_Service.View": true,
                "Config.Manage": true,
                "Config.Delete": true,
                "Config.ShowDiffs": true,
                "Config.View": true,
                "Leaderboards.View": true,
                "Mail_Service.Send_Direct_Messages": true,
                "Mail_Service.Send_Global_Messages": true,
                "Mail_Service.Expire_Global_Messages": true,
                "Mail_Service.Modify_Inbox": true,
                "Mail_Service.View": true,
                "Matchmaking.View": true,
                "Multiplayer.View": true,
                "NFT.View": true,
                "Player_Service.Search": true,
                "Player_Service.Screenname": true,
                "Player_Service.Unlink_Accounts": true,
                "Player_Service.Update": true,
                "Player_Service.ForceAccountLink": true,
                "Player_Service.ViewStoreOffers": true,
                "Player_Service.GeneratePlayerTokens": true,
                "Player_Service.View": true,
                "Portal.SuperUser": true,
                "Portal.ManagePermissions": true,
                "Portal.ViewActivityLogs": true,
                "Portal.ViewBouncedEmails": true,
                "Portal.UnbanBouncedAddress": true,
                "Portal.View": true,
                "PvP.View": true,
                "Receipt.View": true,
                "Token_Service.Ban": true,
                "Token_Service.Unban": true,
                "Token_Service.Invalidate": true,
                "Token_Service.View": true
            },
            "activity": null,
            "id": "65823b388be2aefc0e280476",
            "createdOn": 1703033656
        }
    ]
}
```

The `id` parameter here is optional.  If you do not specify an ID, **all portal accounts** will appear in the array instead.

Another important thing to note: take a look at the first two permissions in both the roles.  When DMZ checks for permissions, it combines all roles with an individual permission set to create a `Passport`, and the more permissive value is used.  So, in this particular case, both `Calendar_Service.View` and `Chat_Service.Send_Announcements` would evaluate to true.  One is granted by the account and not the role, and the other is granted by the role but not the account.

Realistically, almost every account will be managed by their assigned roles and not individual permissions.  Think of the individual permission set as an override.

### Updating Permissions and/or Roles for an account

```
PUT /permissions/update
{
    "id": "65823b388be2aefc0e280476"
    "roleIds": [
        "65824dbd2033ec419bdc9ccd"
    ],
    "permissions": {
        "Calendar_Service.View_Navbar": true,
        "Calendar_Service.View_Page": true,
        "Calendar_Service.Edit": false,
        ...
    }
}

HTTP 204 (No Content)
```

One or both of either `roleIds` or `permissions` must be provided.  This is a **replace operation**; that is to say that the account will reflect these exact role IDs or permissions.  If you send an empty array for `roleIds`, that's effectively removing all roles from an account.  Similarly, if you only send one value in `permissions`, all other permissions will be `false`.

Only pass the role IDs in, not the role objects themselves.  You cannot use this endpoint to update a role's permission set; just what permissions an individual account has and the roles it has assigned to it.

### Seeing What Roles Are Available

```
GET /permissions/roles

HTTP 200
{
    "roles": [
        {
            "name": "foobar",
            "permissions": {
                "Calendar_Service.View": false,
                "Chat_Service.Send_Announcements": true,
                "Chat_Service.Delete_Announcements": false,
                ...
            },
            "id": "65824dbd2033ec419bdc9ccd",
            "createdOn": 1703038397
        },
        ...
    ]
}
```

This endpoint will list all of the roles available to DMZ.  The `id` is the most important field to track since this will be necessary for updating / deleting a role and assigning it to users.

### Creating a New Role

```
POST /permissions/roles/create
{
    "name": "foobar",
    "permissions": {
        "Calendar_Service.View": true,
        "Chat_Service.Send_Announcements": true,
        "Chat_Service.Delete_Announcements": true,
        ...
    }
}

HTTP 200
{
    "role":
    {
        "name": "foobar",
        "permissions": {
            "Calendar_Service.View": true,
            "Chat_Service.Send_Announcements": true,
            "Chat_Service.Delete_Announcements": true,
            ...
        },
        "id": "65824dbd2033ec419bdc9ccd",
        "createdOn": 1703038397
    }
}
```

Not much to say about this one, other than the fact it returns the newly-created role with its ID.  Role names are forced to be unique; this is a constraint added to MongoDB, so this request will fail if you try to create a new role with a pre-existing name.

### Updating an Existing Role

```
PUT /permissions/roles/update
{
    "id": "65824dbd2033ec419bdc9ccd",
    "permissions": {
        "Calendar_Service.View": true,
        "Chat_Service.Send_Announcements": true,
        "Chat_Service.Delete_Announcements": false,
        ...
    }
}

HTTP 204 (No Content)
```

Same as above; the updated model is returned for the output.

### Deleting a Role

```
DELETE /permissions/roles/delete?id=65824dbd2033ec419bdc9ccd

HTTP 204 (No Content)
```

This will remove the role entirely after first removing it from all accounts that have been assigned that role.

## Final Notes

There is a `superuser` role that can't be permanently removed.  If deleted, the role will be erased from accounts that previously had it, but DMZ will add it back again on its own.  Similarly, if its permissions are changed to be `false` on anything, DMZ will repair the role.  This is considered a necessary role to have and can not be permanently removed.

These changes can only happen at the manual database-level.  API calls have protections against altering the superuser.

Since permissions can be added at any time from the backend, it's a good idea to build a frontend client in a dynamic way that it draws available values from keys returned by a permissions object, since those will reflect what's actually in the code.  As C# properties are deleted, they'll disappear from the responses; similarly, when one is added, a new property will appear.