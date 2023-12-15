namespace HurryUpHaul.Domain.Helpers
{
    internal interface IDateTimeProvider
    {
        DateTime Now { get; }
    }

    internal class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}