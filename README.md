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
All endpoints are reached with the base route `/portal/` (subject to change soon as of time of writing.
Any following endpoints listed are appended on to the base route.

## Top Level
No tokens are required for this endpoint.

| Method | Endpoint  | Description                                                                         | Required Parameters | Optional Parameters |
|-------:|:----------|:------------------------------------------------------------------------------------|:--------------------|:--------------------|
|    GET | `/health` | **INTERNAL** Health check on the status of the following services: `AccountService` |                     |                     |

## Permissions
All non-health endpoints require a valid token.

| Method | Endpoint | Description | Required Parameters | Optional Parameters |
|-------:|:---------|:------------|:--------------------|:--------------------|

## Chat Service
All non-health endpoints require a valid token.

| Method | Endpoint | Description | Required Parameters | Optional Parameters |
|-------:|:---------|:------------|:--------------------|:--------------------|

## Dynamic Config Service
All non-health endpoints require a valid token.

| Method | Endpoint | Description | Required Parameters | Optional Parameters |
|-------:|:---------|:------------|:--------------------|:--------------------|

## Mailbox Service
All non-health endpoints require a valid token.

| Method | Endpoint | Description | Required Parameters | Optional Parameters |
|-------:|:---------|:------------|:--------------------|:--------------------|

## Player Service
All non-health endpoints require a valid token.

| Method | Endpoint | Description | Required Parameters | Optional Parameters |
|-------:|:---------|:------------|:--------------------|:--------------------|

## Token Service
All non-health endpoints require a valid token.

| Method | Endpoint | Description | Required Parameters | Optional Parameters |
|-------:|:---------|:------------|:--------------------|:--------------------|