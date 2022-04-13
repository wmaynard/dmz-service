using System;
using System.Collections.Generic;
using TowerPortal.Models;

namespace TowerPortal.Utilities;

public class ParseMessageData
{
    public static long ParseDateTime(string dateTime)
    {
        return ((DateTimeOffset) DateTime.Parse(dateTime)).ToUnixTimeSeconds();
    }

    public static string ParseEmpty(string input)
    {
        if (input == null)
        {
            return "";
        }
        else
        {
            return input;
        }
    }

    public static List<Attachment> ParseAttachments(string attachments)
    {
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
                quantity: int.Parse(properties[2]));
            parsedAttachments.Add(attachment);
        }

        return parsedAttachments;
    }
}