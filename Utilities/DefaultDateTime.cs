using System;

namespace TowerPortal.Utilities;

public static class DefaultDateTime
{
    public static string UtcDateTimeString(int days = 0)
    {
        DateTime dt = DateTime.UtcNow;
        dt = dt.AddDays(days);
        int monthInt = dt.Month;
        string month = "";
        if (monthInt < 10)
        {
            month = "0" + monthInt;
        }
        else
        {
            month += monthInt;
        }

        int dayInt = dt.Day;
        string day = "";
        if (dayInt < 10)
        {
            day = "0" + dayInt;
        }
        else
        {
            day += dayInt;
        }
        
        int year = dt.Year;
        
        return year + "-" + month + "-" + day + "T00:00";
    }
}