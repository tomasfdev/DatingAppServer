namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime date)
        {
            var todayDate = DateTime.Today;

            var age = todayDate.Year - date.Year;

            if (date.Date > todayDate.AddYears(-age))
                age--;

            return age;
        }
    }
}
