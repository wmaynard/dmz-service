## Introduction

When an email delivery fails, we get a **bounce**.  Every at-scale email provider has a requirement that we take precautions to keep our bounce rate as low as possible.  If our bounce rate remains above a certain threshold, email providers may block us from using their services.  While it's impossible to prevent all bounces ahead of time, there are some defensive measures we can take.  This document will walk you through our policy and tooling.

At the time of this writing, this document is only concerned with _Rumble Accounts_.  We don't send emails to other SSO-provided addresses.

## What is a Bounce?

There are two major categories of bounces: hard and soft.  Hard bounces are those that the email delivery failed in such a way that it's unlikely to ever succeed.  An email address may not exist, or the format may not even be an email address at all.

A soft bounce, however, is one that is likely temporary; a mailbox might be full, and if a user clears out some space, the delivery would succeed.

## Our Policy, at a Glance

### In Nonprod Environments

No email addresses are banned or monitored.  Instead, DynamicConfig contains a CSV whitelist of domain names.  This is done for security to guarantee that access is internal-only, or specific access shared with contractors' work addresses.

However, we do need to be careful.  If we have an internal user who tries to sign up with a bogus Rumble email address enough times, they could land us in a review period with the email provider.  Bounces in nonprod can affect prod!

### In Prod Environments

Hard bounces result in an immediate ban as soon as we find out about them.  By banning these emails before they reach our email provider, we never get dinged for the bounce in the first place.  Soft bounces currently are allowed, but monitored with logs.  Ban appeals must go through a customer service flow.

We also pre-emptively ban email addresses that fail a format validation check.  Signing up without a domain name - or an otherwise invalid format such as multiple `@` symbols - will result in a ban.

## Example User Stories

* A player signs up for the game as `gollum1937@gmal.com`.  The domain is incorrect and has a typo in it.  This address will bounce and be banned.  The client lets them know the email failed and they need to check their email address.  For more information, see [player-service's Login documentation](https://gitlab.cdrentertainment.com/platform-services/player-service-v2/-/blob/master/LOGIN.md#handling-banned-emails).
* A player enters nonsense into the email field, `theresnoatsymbolordotcom`.  This is both validated by the client and platform, and no email goes through to our provider, and no bounce is registered.
* A player signs up with a work email address, then changes jobs.  While they used to receive emails from us, they've since lost account access, and the account was deleted.  The next email - regardless of what it is for - will bounce, and the address will be banned.  They must contact customer service for manual account recovery.

A vast majority of natural bounces will originate from the account creation flow.  There's very little chance of a confirmed account bouncing emails, since we know we've already succeeded in sending them content.

## Security Threats

A scripted attack on the account confirmation flow would threaten our email capabilities.  Banning emails is a start for prevention, but won't solve the entire problem.  We can be more defensive with rate limiting at the load balancer level and add some logs and alerting, but ultimately we may need to implement an account creation queue, and possibly disable account creation entirely depending on Provider requirements.  Decisions further down this path are yet to be discussed.

## Related Endpoints

### Public Endpoints

```
GET /bounces/valid?email={address}

204 (No Content) - Indicates the email address has no problems

400
{
    "message": "Email domain has not been whitelisted.",
    "errorCode": "PLATF-0101: Unauthorized",
    "success": false,
    "platformData": {
        "exception": {
            "details": {
                "address": "foo@bar.com",
                "help": "The whitelist can be configured via Portal."
            },
            "message": "Email domain has not been whitelisted.",
            "type": "EmailNotWhitelistedException",
            "stackTrace": ...
        }
    }
}
```

### Admin Endpoints

Important note - these endpoints will ONLY return true bounce data in prod environments.

### `GET /bounces/stats?email={address}`

This is the preferred method of inspecting bans.  Pass it an email address and you'll see the statistics associated with the address. 
```
200
{
    "bounceData": {
        "email": "foo@bar.com",
        "ban": true,                      // This email has been banned
        "hard": 7,                        // Number of lifetime hard bounces
        "soft": 0,                        // Number of lifetime soft bounces
        "ts": 1682705787,                 // Last time a bounce has been registered
        "id": "644c0d61665ed602f4000097"  // If an address has no ban data, this will be null
    }
}
```

### `GET /bounces/banList?lastBounceTime={optional}`

This endpoint returns an array of addresses that have recently bounced and been banned for it.  Given the scale of expected bounces, this does have a hard limit of 1000 results it will return.  If `lastBounceTime` is specified (Unix timestamp), you can scan through older records; if not, the current timestamp is used.

As a future improvement, this could implement paging.

```
200
{
    "banList": [
        "foo",
        "bar",
    ]
}
```

## Repealing a Ban

TODO