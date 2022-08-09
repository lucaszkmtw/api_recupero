namespace CentralOperativa.Domain.System.Workflows
{
    public enum WellKnownWorkflowTypes : short
    {
        Claim = 1,
        TreatmentRequest = 2,
        PurchaseOrder = 3,
        InvoiceApproval = 4,
        Project = 5,
        FeedbackTicket = 6,
        LoanApproval = 7,
        LeadApproval = 8,
        InventoryDispatch = 9,
        InventoryReceipt = 10,
        Collection = 11,
        DebtCollection = 12
    }
}
