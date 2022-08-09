using System;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Flags]
    public enum BusinessDocumentStatus : byte
    {
        Emitted = 0,
        PendingApproval = 1,
        Approved = 2,
        Rejected = 3,
        Paid = 4,
        PendingDelivery = 5,
        Delivered = 6,
        Canceled = 7,
        Voided = 8,
        Open = 9,
        Control = 10,
        InProcess = 11,
        InTransit = 12,
        Partial = 13,
        Secretary = 20,
        DDDR  =30,
        DCEO = 40,
        DGJ = 50,
        Prosecution = 60
    }
}
