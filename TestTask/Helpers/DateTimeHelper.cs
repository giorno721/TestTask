namespace TestTask.Helpers;

public static class DateTimeHelper
{
    public static bool IsDateActive(DateTime? startDate, DateTime? endDate)
    {
        var now = DateTime.Now;
        return startDate < now && (endDate == null || endDate > now);
    }
}
