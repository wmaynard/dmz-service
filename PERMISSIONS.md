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

As a prerequisite, every controller needs to inherit from `PortalController`.  So long as that's been done, you can access the current user's Passport with one of the two following methods:

1. `Permissions.{Project}.{Permission}`, e.g. `Permissions.Player.View_Page`
2. **(Preferred)** `Require(Permissions.Player.Edit, Permissions.Player.CanRename);`

This second method will throw a `PermissionInvalidException` if a user is not authorized to access a particular resource.  If **any** boolean passed into the `Require()` method is false, the exception will cause the user to be redirected to a 403 page.

## View Usage

Similar to controllers, views need to inherit from `PortalView`.  Unlike the regular C# inheritance, views are extended by adding the following to the top of the code file:

```
@inherits TowerPortal.Views.Shared.PortalView
```

After that, usage is the same as the controller usage, though the preferred usage is flipped.  The `PortalView` can use the same `Require()` method, but when dealing with views, it will often make more sense to not redirect the user, and instead just hide elements from the UI when a user lacks permission.

## Adding New Groups

TODO

## Default Permission Sets

TODO

## Managing Allowed Domains

TODO

## Dev vs. Prod

TODO