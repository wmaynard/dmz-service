using System;
using System.Linq;
using System.Text.Json.Serialization;
using RCL.Logging;
using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Interop;
using Rumble.Platform.Common.Models;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Data;

namespace Dmz.Interop;

public static class Mongo
{
    private const string API_PREFIX = "mongoApi";
    private const string URL_PREFIX = "mongoUrl";
    
    private static ApiService _apiService;
    
    // Sensitive CI Values
    private static string Username => PlatformEnvironment.Optional<string>("MONGO_API_USERNAME");
    private static string ApiKey => PlatformEnvironment.Optional<string>($"MONGO_API_PRIVATE_KEY");
    
    // DC Values
    private static string UrlLogin => DynamicConfig.Instance?.Optional<string>($"{URL_PREFIX}Login");
    private static string UrlList => DynamicConfig
        .Instance?
        .Optional<string>($"{URL_PREFIX}Whitelist")
        .Replace("{{groupId}}", DynamicConfig.Instance.Require<string>($"{API_PREFIX}GroupId"));
    
    private static string UrlNewEntry => DynamicConfig.Instance?.Optional<string>($"{URL_PREFIX}Login");
    private static string GroupId => DynamicConfig.Instance?.Optional<string>($"{URL_PREFIX}GroupId");
    
    private static string OrganizationId => DynamicConfig.Instance?.Optional<string>($"{API_PREFIX}OrganizationId");
    private static string AcceptHeader => DynamicConfig.Instance?.Optional<string>($"{API_PREFIX}AcceptHeader");

    private static ApiRequest Request(string url) => (_apiService ??= PlatformService.Require<ApiService>())?.Request(url);

    /// <summary>
    /// Used to get an API access token.  This is used for advanced Mongo API calls.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>True if the request was successful and the token is non-empty / non-null.</returns>
    private static bool Login(out string token)
    {
        token = null;

        string _token = null;
        Request(UrlLogin)
            .SetPayload(new RumbleJson
            {
                {"username", Username},
                {"apiKey", ApiKey}
            })
            .OnSuccess(response => _token = response?.Optional<string>("access_token"))
            .OnFailure(response => Log.Error(Owner.Will, "Unable to login to Mongo; cannot whitelist user.", data: new
            {
                response = response
            }))
            .Post();

        token = _token;

        return !string.IsNullOrWhiteSpace(token);
    }

    /// <summary>
    /// Lists the current IP whitelist entries.
    /// </summary>
    /// <param name="entries"></param>
    /// <returns>True if the request was successful and at least 1 entry was returned.</returns>
    public static bool GetCurrentWhitelist(out MongoWhitelistEntry[] entries)
    {
        entries = null;

        MongoWhitelistEntry[] _entries = Array.Empty<MongoWhitelistEntry>();
        Request(UrlList)
            .AddDigestAuth(UrlList, Username, ApiKey)
            .AddHeader("Accept", AcceptHeader)
            .AddParameter("itemsPerPage", "500")
            .OnSuccess(response => _entries = response.Require<MongoWhitelistEntry[]>("results"))
            .OnFailure(response => Log.Error(Owner.Will, "Unable to fetch Mongo whitelist", data: new
            {
                Response = response
            }))
            .Get();

        entries = _entries;
        return entries != null && entries.Any();
    }

    public static string GenerateComment(string name) => $"{name} (via DMZ)";

    /// <summary>
    /// Creates or updates an IP whitelist entry in Mongo.  Uses a comment like "Will Maynard (via DMZ)".
    /// </summary>
    /// <param name="name">A user's name that should be attached to the comment.</param>
    /// <param name="ip">The user's IP address.</param>
    /// <returns>True if the request was successful.</returns>
    public static bool AddWhitelistEntry(string name, string ip)
    {
        Request(UrlList)
            .AddDigestAuth(UrlList, Username, ApiKey)
            .AddHeader("Accept", AcceptHeader)
            .EncapsulatePayload("[", "]")
            .SetPayload(new RumbleJson
            {
                { "comment", GenerateComment(name) },
                { "ipAddress", ip }
            })
            .OnSuccess(_ => Log.Local(Owner.Will, $"Successfully updated Mongo whitelist for {name}"))
            .OnFailure(response => Log.Error(Owner.Will, "Unable to update Mongo whitelist", data: new
            {
                Response = response
            }))
            .Post(out _, out int code);

        bool output = code.Between(200, 299);

        if (output)
            SlackDiagnostics
                .Log(
                    title: "User whitelisted for Mongo via DMZ",
                    message: $"{name} ({ip}) has logged into Portal and been granted an IP whitelist entry for nonprod Mongo."
                )
                .Tag(Owner.Will)
                .Send()
                .Wait();

        return output;
    }

    public static bool DeleteWhitelistEntry(MongoWhitelistEntry entry)
    {
        Request(entry.SelfReferenceUrl)
            .AddDigestAuth(UrlList, Username, ApiKey)
            .AddHeader("Accept", AcceptHeader)
            .OnSuccess(_ => Log.Info(Owner.Will, "Deleted a Mongo whitelist entry", data: new
            {
                MongoWhitelistEntry = entry
            }))
            .OnFailure(response => Log.Error(Owner.Will, "Unable to delete Mongo whitelist entry", data: new
            {
                Response = response,
                MongoWhitelistEntry = entry
            }))
            .Delete(out _, out int code);

        return code.Between(200, 299);
    }
}