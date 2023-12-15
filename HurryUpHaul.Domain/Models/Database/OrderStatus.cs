namespace HurryUpHaul.Domain.Models.Database
{
    internal enum OrderStatus
    {
        Created,
        OrderAccepted,
        InProgress,
        WaitingDelivery,
        Delivering,
        Completed,
        Cancelled
    }
}