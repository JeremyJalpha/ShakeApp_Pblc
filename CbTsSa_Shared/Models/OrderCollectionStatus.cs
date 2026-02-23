namespace CbTsSa_Shared.Models
{
    public enum OrderCollectionStatus
    {
        Pending = 0,        // Order submitted, not yet collected
        BeingCollected = 1, // Driver collecting items
        EnrouteToCust = 2,  // Driver delivering to customer
        ArrivedAtCust = 3,  // Driver at customer location
        Delivered = 4       // Order delivered to customer
    }
}