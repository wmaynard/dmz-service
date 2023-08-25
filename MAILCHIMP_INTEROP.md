# Mailchimp Integrations

On 2023.08.23, we had a meeting around marketing email integrations to help increase retention.  The idea is to allow players to opt-in to marketing emails, which then rewards them with promo offers via email.  This document will walk you through how the Platform support works and how to craft rewards.

## Glossary

| Term              | Definition                                                                                        |
|:------------------|:--------------------------------------------------------------------------------------------------|
| Claim Code / Code | A unique identifier used to grant a player a reward when they interact with an email.             |
| Redirect URL      | Where Platform should send a user after a reward is claimed.                                      |
| Reward            | A package of one or more in-game currencies to be sent to the player upon interacting with email. |

## On the Mailchimp Side

Mailchimp's available fields are somewhat limiting in what they allow us to do.  We can't add our own custom fields to an account, and because Mailchimp uses email addresses as unique identifiers, we need a way to prevent a player from figuring out a way to claim a reward multiple times.  DMZ shares the following information with Mailchimp:

1. Email Address
2. Subscription Status
3. Screenname (stored in Mailchimp's First Name field)
4. Account ID (stored in Mailchimp's Phone Number field)

Since we lack the ability to add a custom field, we'll repurpose the phone number, as it doesn't have the validation requirements that the address does.

All of this information comes from player tokens used in the game client.  If a token is stale (has not yet been regenerated after a screenname change, for example), then the information in Mailchimp will similarly be stale.

## Step 1: Setting Up in Dynamic Config

Dynamic Config requires several variables for Mailchimp to function properly:

```
mailchimpApiKey                This is a private key used to authenticate requests.  You can find it in Bitwarden.
mailchimpFailurePage           A link the player is redirected to on a failed claim.
mailchimpListId                This is the mailing list used to fetch member information from.  May only be discoverable via API.
mailchimpReward_{claim code}   Any number of rewards are supported, so long as they follow this format.
mailchimpSuccessPage           A link the player is redirected to on a successful claim.
mailchimpUrl                   This link must correspond to the correct datacenter our Mailchimp account is on - it's the first subdomain in the link.
```

To set up the success / failure pages with custom messaging, you will need to work with web and platform teams.

Be sure to bring over all necessary values to each environment as DMZ / the mail features get pushed through our environment pipelines.

## Step 2: Defining Rewards

* Log in to Portal in the appropriate environment
* Navigate to `/config/dmz-service`
* Add a new key in the format `mailchimpReward_{code}`, where `{code}` is replaced by a unique string that matches your reward package.
* In the value field, provide a CSV that will be used to craft the in-game mail message.  It should follow the format `subject,body,icon,banner,note,type,rewardId,quantity`.
  * You can specify multiple rewards in the package by repeating the last three fields.
* For the note, you can leave it blank, but it's a good idea to fill it out to describe it.

Example Reward:
```
key:     mailchimpReward_FreeGoldTest
value:   Test,This is a test from DC mailchimp rewards,,,mailchimp test reward,currency,soft_currency,1000
comment: 1000 free gold to players in a test group
```

**IMPORTANT**: if your reward CSV does not pass validation, players will see errors.  Take care to add the appropriate number of fields - you can't just omit them, but you can leave empty strings by still adding more commas.  Similarly, don't use commas anywhere in text as values; in other words, don't provide a message of "Sweet, free gold!" - that will break the rewards parsing, as `Sweet` and ` free gold!` will be considered separate items.

### Claiming a Reward from an Email

We're very limited in the functionality we can embed into an email.  JavaScript code isn't allowed, so we're limited to links that a player can click on.  Consequently, these will necessarily have to be GET requests, and unlike the rest of our API, we can't have a token embedded with it.  However, we can still accomplish what we need with just two query parameters.  A valid link looks like:

```
https://{environment}/dmz/mailchimp/claim?accountId={Account ID}&emailId={claim code}
```

* Environment: If you need help finding this, ask #platform.
* Account ID: Use the Phone Number from mailchimp here.  It should be a 24-digit hex string.
* Claim Code: This can be anything you want.  It can just be a template name, a coupon-style code, or just completely random - **as long as it's unique**, and as long as Dynamic Config has a corresponding entry.

Craft this link in your Mailchimp templates and it will be ready to send out.  Since we base our rewards on Account ID and not email address, no one account will be able to claim the same code twice.  When a user clicks the link, Platform will:

1. Grab the relevant URLs for success / failure from Dynamic Config
2. Parse the reward package from the CSV specified in Dynamic Config.
3. Tentatively marks the claim code as used on the player's account.
4. If this has already been used, or the account not found, the player will be redirected to the failure page.
5. Send the reward to mail-service.
6. If sending the reward fails, the claim code is restored and marked as unused, and the player will be redirected to the failure page.
7. Redirect the player to the success page.

## Step 3: Implementation via API

For frontend implementation, there are only two endpoints that need to be used.  Both of these endpoints require the player's token to be sent as an authorization, as is standard for Platform services.  These endpoints are:

```
GET /dmz/mailchimp/status

// Sample Response
{
    "mailchimpMember": {
        "email_address": "john.doe@gmail.com",
        "status": "subscribed",                  // "subscribed" or "unsubscribed"             
        "createdOn": 1692920571,                 // Unix Timestamp, set on the first request to one of these endpoints
        "accountId": "6375681659c472bca7dabc40", 
        "screenname": "Player25cc11b",
        "merge_fields": null,
        "claimedEmails": [
            "claimCode1"
        ],
        "subscriptionStatus": 1,                 // 0: Unsubscribed, 1: Subscribed, 2: Unknown
        "id": "64e7eafb0b93631fcc9cb7a0"         // Ignore; this is a Mongo ID unrelated to AccountId.
    }
}
```

```
PATCH /dmz/mailchimp/subscription
{
    "subscribed": true
}

// Sample Response
HTTP 204 (No Content)
```

Use `GET /status` to check the player's current subscription status, and `PATCH /subscription` to change it.  Be sure to include their player token in the request; this is how Platform will populate the necessary details.

## Important Notes

* Since this is a third-party integration, there's no way for us to know exactly when a player unsubscribes from our mailing list.  However, DMZ will check, on a timer, to update all of its player records against the information Mailchimp stores.  So, if a player clicks "unsubscribe" from their email, we'll find out about it in a reasonable time window - just not instantly.  Consequently, it is possible for the client to show some stale information for a brief amount of time.  At the time of this writing, the time is set to 1 hour.  Consider the following:
  * Player clicks `unsubscribe` from their email.
  * Player immediately opens the game and checks their subscription status.
  * The client may take anywhere from approximately 0-60 minutes to reflect the change, depending on where the timer is.
* According to Mailchimp's documentation, their API rate limit may be set to 10 simultaneous requests.  At an early scale this isn't a problem, but eventually we may hit that ceiling if enough players are active all at once.  If this happens we will need to transition to batch operations or queueing.
* If we receive a GDPR deletion request, their email will be replaced with dummy text **on the DMZ side only.**