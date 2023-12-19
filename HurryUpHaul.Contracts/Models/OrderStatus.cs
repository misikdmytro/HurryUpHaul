namespace HurryUpHaul.Contracts.Models
{
    public enum OrderStatus
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