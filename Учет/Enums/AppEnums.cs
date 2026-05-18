namespace Учет.Enums
{
    public enum UserRole
    {
        Admin,
        Operator
    }

    public enum AssetStatus
    {
        Registered,
        InService,
        UnderRepair,
        UnderMaintenance,
        Expired,
        WrittenOff
    }

    public enum DocType
    {
        TransferAct,      
        DamageAct,         
        WriteOffAct,       
        PurchaseRequest  
    }
}