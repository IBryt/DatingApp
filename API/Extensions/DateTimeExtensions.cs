namespace API.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateTime dob)
    {
        var currentDate = DateTime.UtcNow;
        int age = currentDate.Year - dob.Year;

        if (dob.Date > currentDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
