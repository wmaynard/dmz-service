using System;
using System.Collections.Generic;
using System.Linq;
using TowerPortal.Models;

namespace TowerPortal.Utilities;

public class ParseMessageData
{
    public static long ParseDateTime(string dateTime)
    {
        if (dateTime == null)
        {
            return 0;
        }
        return ((DateTimeOffset) DateTime.Parse(dateTime)).ToUnixTimeSeconds();
    }

    public static string ParseEmpty(string input)
    {
        if (input == null)
        {
            return "";
        }
        return input;
    }

    public static List<Attachment> ParseAttachments(string attachments)
    {
        if (attachments == "")
        {
            return new List<Attachment>();
        }
        
        char[] delimiters = {'|', ','};
        string[] split = attachments.Split(delimiters);
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = split[i].Trim();
        }

        List<Attachment> parsedAttachments = new List<Attachment>();

        foreach (string entry in split)
        {
            string[] properties = entry.Split(" ");
            Attachment attachment = new Attachment(type: properties[0], rewardId: properties[1],
                quantity: properties.Length == 3 ? int.Parse(properties[2]) : 1);
            parsedAttachments.Add(attachment);
        }

        return parsedAttachments;
    }

    public static List<string> ParseIds(string playerIds)
    {
        char[] delimiters = {'|', ','};
        string[] split = playerIds.Split(delimiters);
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = split[i].Trim();
        }

        List<string> parsedIds = new List<string>();

        foreach (string entry in split)
        {
            parsedIds.Add(entry);
        }

        return parsedIds;
    }
}