# One-Time Password (OTP) Overview

## Introduction

Sending sensitive information from one application to another can be a tricky task, and depending on how this is accomplished, there can be security vulnerabilities.  These can sometimes be mitigated with the usage of locking a value behind an OTP, such that the second application in the flow is guaranteed to only be able to get a value exactly once.

## Use Case: Passing Tokens

The game client needs to pass a user token along with its browser instance to allow the NFT marketplace to act on that player's behalf.  The initial implementation of this just passed the token in as a query string parameter.  This is _unsafe_:

* It is susceptible to a Man-In-The-Middle (MITM) attack.  Someone listening to web traffic can uncover the request and query string.  If this is a user's token which is valid for several days, the attacker then gains access to a player's account for a good length of time.
* If a player copies and shares the URL with the query string, they've just given away their account access.  In the worst-case scenario, the marketplace shows a url like `/nftsite?token=ey6b...` once the page is fully loaded.  The user then copies the link to share it with their friends and unknowingly granted full access to their account - _and_ for several days, too.

## Solution

Currently, there are only two OTP endpoints for use.  This can easily be built out to cover more scenarios, but an initial version only addresses the above use case, where we specifically need to send a token along.  Use the following flow for implementation:

### Game Client

1. The player triggers an event that would direct them to the marketplace.
2. The game client makes a request, using the token as authorization:
```
// Sample Request
POST /dmz/otp/token
{
}

// Sample Response
{
    "otp": "632a4fc1750e75589d311c3a"
}
```
3. The game client launches a browser to the marketplace with the above `otp` value as a query parameter.

### Marketplace

1. The marketplace grabs the `otp` and sends the following request:
```
// Sample Request
DELETE /dmz/otp?id=632a4fc1750e75589d311c3a

// Sample Response
{
    "value": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhaWQiOiJGaWN0aW9uYWwgU2VydmljZSBVc2VyIiwiZXhwIjoxOTUwOTAzNTA5LCJpc3MiOiJSdW1ibGUgVG9rZW4gU2VydmljZSIsImlhdCI6MTYzNTU0MzUwOSwiYXVkIjpbIjU3OTAxYzZkZjgyYTQ1NzA4MDE4YmE3M2I4ZDE2MDA0Il0sInNuIjoiUG9zdG1hblRlc3QgKHdtYXluYXJkX2xvY2FsKSIsImQiOjMxNDEsIkAiOiI4eGNVWDFyVEh0emVoZHk4YXZxQWxrYUw1R1ZoSi9HKzQwWWNmYkRBeXYwPSIsImlwIjoiOjoxIiwic3UiOnRydWV9.EMQtVy_4qRJY2lbdFXTjiPsxmhSxrXPskS6bR3d3Iu9zF2ASkqHh2jiQOta2SQldS1PSPWS14PMALcJyoP1Wqe"
}
```
2. The marketplace stores the `value` above as the user's token in a cookie.
3. The marketplace rewrites the URL bar so that the OTP is no longer visible.

## Additional Information

* When an OTP is created, a value, account ID, false boolean, and a timestamp is created in MongoDB.
* When an OTP is claimed, the value is set to null and the status is set to `claimed`.
  * The value is set to null to guarantee that a value can never be retrieved more than once.
* When an OTP is claimed a second time, an exception is thrown to indicate / log that the claim has been triggered again.
* OTPs are cleared out from the database after 4 days (the same lifetime as a token).

## Troubleshooting

If we see a lot of activity on duplicate OTP calls, a few things might be happening:
* The marketplace or other consumer of OTPs is erroneously trying to claim them more than once.  This can be fixed in those applications.
* A malicious actor is trying to use the OTP to gain access somewhere.

If needed, we can invalidate user tokens on duplicate OTP calls.  While this would guarantee their account tokens would be safe for the short term, it would also interrupt their game flow and marketplace flow.  Consequently it should only be implemented if there is a serious problem with dropped OTPs.