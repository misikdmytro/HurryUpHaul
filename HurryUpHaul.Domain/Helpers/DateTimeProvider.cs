namespace HurryUpHaul.Domain.Helpers
{
    internal interface IDateTimeProvider
    {
        DateTimeOffset Now { get; }
    }

    internal class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset Now => DateTimeOffset.UtcNow;
    }
}