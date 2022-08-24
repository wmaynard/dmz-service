# Tower Portal Backend
A service to support the Tower Portal frontend.

# Introduction
This service contains endpoints that route to other services that the Tower Portal requires functionality for. The service also 
allows for a customized security policy as well as supporting permissions for users on the Tower Portal.

# Required Environment Variables
|                 Variable | Description                                         |
|-------------------------:|:----------------------------------------------------|
| HELM_UPGRADE_VALUES_FILE | File to manage specific environment deploy settings |
|              MONGODB_URI | The connection string for the environment's MongoDB |
|         RUMBLE_COMPONENT | The name of the service.                            |
|        RUMBLE_DEPLOYMENT | Signifies the deployment environment.               |
| RUMBLE_REGISTRATION_NAME | Moniker for the service for Dynamic Config          |

# Glossary
| Term | Description |
|-----:|:------------|

# Using the Service
All non-health endpoints require a valid admin token. This is required as a security measure to limit requests to the portal
and specific users on the portal depending on their permissions. The admin token is used in an `Authorization` header 
as a `Bearer {token}`.

# Endpoints
**All endpoints are reached with the base route `/portal/`** (subject to change soon as of time of writing).
Any following endpoints listed are appended on to the base route. If the parameters for the endpoints on the corresponding 
services are updated, the endpoints here will reflect those changes.

## Top Level
No tokens are required for this endpoint.

| Method | Endpoint  | Description                                                            | Required Parameters | Optional Parameters |
|-------:|:----------|:-----------------------------------------------------------------------|:--------------------|:--------------------|
|    GET | `/health` | Health check on the status of the following services: `AccountService` |                     |                     |

## Permissions
All non-health endpoints require a valid token.
**All endpoints are prefixed with `/permissions/`.**

| Method | Endpoint   | Description                                           | Required Parameters                | Optional Parameters |
|-------:|:-----------|:------------------------------------------------------|:-----------------------------------|:--------------------|
|    GET | `/list`    | Lists all user accounts on the portal                 |                                    |                     |
|    GET | `/account` | Returns the account associated with the provided `id` | *string* `id`                      |                     |
|  PATCH | `/update`  | Modifies permissions on the account provided          | *string* `id`<br />*object* `data` |                     |

## Chat Service
All non-health endpoints require a valid token.
**All endpoints are prefixed with `/chat/`.**

| Method | Endpoint                | Description                             | Required Parameters                  | Optional Parameters                                                                                 |
|-------:|:------------------------|:----------------------------------------|:-------------------------------------|:----------------------------------------------------------------------------------------------------|
|    GET | `/announcements`        | Fetches all chat announcements          |                                      |                                                                                                     |
|   POST | `/announcements/send`   | Sends a new chat announcement           | *string* `text`                      | *long* `durationInSeconds`<br/>*long* `expiration`<br/>*long* `visibleFrom`<br/>*string* `language` |
|  PATCH | `/announcements/edit`   | Edits an existing chat announcement     |                                      |                                                                                                     |
|   POST | `/announcements/delete` | Deletes an existing chat announcement   | *string* `messageId`                 |                                                                                                     |
|   POST | `/player`               | Fetches reports and bans for the player | *string* `aid`                       |                                                                                                     |
|    GET | `/reports`              | Lists all reports                       |                                      |                                                                                                     |
|   POST | `/reports/ignore`       | Ignores(archives) a specific report     | *string* `reportId`                  |                                                                                                     |
|   POST | `/reports/delete`       | Deletes a specific report               | *string* `reportId`                  |                                                                                                     |
|   POST | `/ban`                  | Chat bans a player                      | *string* `aid`<br/>*string* `reason` | *long* `durationInSeconds`<br/>*string* `reportId`                                                  |
|   POST | `/unban`                | Chat unbans a player                    | *string* `accountId`                 |                                                                                                     |

## Dynamic Config Service
All non-health endpoints require a valid token.
**All endpoints are prefixed with `/config/`.**

| Method | Endpoint    | Description                                    | Required Parameters                                                            | Optional Parameters |
|-------:|:------------|:-----------------------------------------------|:-------------------------------------------------------------------------------|:--------------------|
|    GET | `/settings` | Fetches all dynamic config settings            |                                                                                |                     |
|   POST | `/section`  | Creates a new section in config                | *string* `name`<br/>*string* `friendlyName`                                    |                     |
|  PATCH | `/update`   | Updates an existing value or creates a new key | *string* `name`<br/>*string* `key`<br/>*string* `value`<br/>*string* `comment` |                     |
| DELETE | `/delete`   | Deletes an existing key                        | *string* `name`<br/>*string* `key`                                             |                     |

## Mailbox Service
All non-health endpoints require a valid token.
**All endpoints are prefixed with `/mailbox/`.**

| Method | Endpoint         | Description                           | Required Parameters                                   | Optional Parameters |
|-------:|:-----------------|:--------------------------------------|:------------------------------------------------------|:--------------------|
|    GET | `/global`        | Fetches all global messages           |                                                       |                     |
|   POST | `/global/send`   | Sends a new global message            | *GlobalMessage* `globalMessage`                       |                     |
|  PATCH | `/global/edit`   | Edits an existing global message      | *GlobalMessage* `globalMessage`                       |                     |
|  PATCH | `/global/expire` | Expires an existing global message    | *string* `messageId`                                  |                     |
|   POST | `/direct/send`   | Sends a new direct(or group) message  | *List<*string*>* `accountIds`<br/>*Message* `message` |                     |
|    GET | `/inbox`         | Views inbox for the specified player  | *query* `accountId`                                   |                     |
|  PATCH | `/inbox/edit`    | Edits a message in a player's inbox   | *string* `accountId`<br/>*Message* `message`          |                     |
|  PATCH | `/inbox/expire`  | Expires a message in a player's inbox | *string* `accountId`<br/>*string* `messageId`         |                     |

## Player Service
All non-health endpoints require a valid token.
**All endpoints are prefixed with `/player/`.**

| Method | Endpoint         | Description                             | Required Parameters                            | Optional Parameters |
|-------:|:-----------------|:----------------------------------------|:-----------------------------------------------|:--------------------|
|    GET | `/search`        | Searches for a list of players by query |                                                |                     |
|    GET | `/details`       | Fetches data for a player               | *query* `accountId`                            |                     |
|  PATCH | `/screenname`    | Modifies a player's screenname          | *string* `accountId`<br/>*string* `screenname` |                     |
|  PATCH | `/wallet/add`    | Adds currency to a player's wallet      | *Wallet* `component`                           |                     |
|  PATCH | `/wallet/remove` | Removes currency from a player's wallet | *Wallet* `component`                           |                     |
| DELETE | `/unlink`        | Unlinks a google account from a player  | *string* `email`                               |                     |

## Token Service
All non-health endpoints require a valid token.
**All endpoints are prefixed with `/token/`.**

| Method | Endpoint      | Description                                  | Required Parameters | Optional Parameters |
|-------:|:--------------|:---------------------------------------------|:--------------------|:--------------------|
|  PATCH | `/ban`        | Bans a player permanently or for a duration  | *string* `aid`      | *long* `duration`   |
|  PATCH | `/unban`      | Unbans a player                              | *string* `aid`      |                     |
|  PATCH | `/invalidate` | Invalidates a player's token (logs them out) | *string* `aid`      |                     |