namespace Obss.SharedKernel.Application.Authorization;

public static class Permissions
{
    public const string PolicyPrefix = "permission:";

    public static class Iam
    {
        public const string Platform = "iam:platform";
        public const string TenantRead = "iam:tenants:read";
        public const string TenantWrite = "iam:tenants:write";
        public const string UserRead = "iam:users:read";
        public const string UserWrite = "iam:users:write";
        public const string UserDeactivate = "iam:users:deactivate";
        public const string RoleRead = "iam:roles:read";
        public const string RoleWrite = "iam:roles:write";
        public const string PermissionManage = "iam:permissions:manage";
    }

    public static class Billing
    {
        public const string BillRead = "billing:bills:read";
        public const string BillWrite = "billing:bills:write";
        public const string BillFinalize = "billing:bills:finalize";
        public const string BillAdjust = "billing:bills:adjust";
        public const string AccountRead = "billing:accounts:read";
        public const string AccountWrite = "billing:accounts:write";
        public const string TaxManage = "billing:tax:manage";
        public const string CycleManage = "billing:cycles:manage";
    }

    public static class Payments
    {
        public const string PaymentRead = "payments:read";
        public const string PaymentWrite = "payments:write";
        public const string PaymentRefund = "payments:refund";
        public const string PaymentGatewayManage = "payments:gateways:manage";
        public const string PaymentReconciliation = "payments:reconciliation";
    }

    public static class Invoices
    {
        public const string InvoiceRead = "invoices:read";
        public const string InvoiceWrite = "invoices:write";
        public const string InvoiceFinalize = "invoices:finalize";
        public const string InvoiceSend = "invoices:send";
        public const string InvoiceCreditNote = "invoices:credit-notes";
        public const string InvoiceDisputeManage = "invoices:disputes:manage";
    }

    public static class Provisioning
    {
        public const string JobRead = "provisioning:jobs:read";
        public const string JobWrite = "provisioning:jobs:write";
        public const string JobExecute = "provisioning:jobs:execute";
        public const string TemplateManage = "provisioning:templates:manage";
        public const string ServiceOrderRead = "provisioning:service-orders:read";
        public const string ServiceOrderWrite = "provisioning:service-orders:write";
    }

    public static class Orders
    {
        public const string OrderRead = "orders:read";
        public const string OrderWrite = "orders:write";
        public const string OrderApprove = "orders:approve";
        public const string OrderCancel = "orders:cancel";
        public const string OrderFulfill = "orders:fulfill";
    }

    public static class Customers
    {
        public const string CustomerRead = "customers:read";
        public const string CustomerWrite = "customers:write";
        public const string CustomerKycVerify = "customers:kyc-verify";
        public const string CustomerSegmentManage = "customers:segments:manage";
    }

    public static class Crm
    {
        public const string QuoteRead = "crm:quotes:read";
        public const string QuoteWrite = "crm:quotes:write";
        public const string QuoteApprove = "crm:quotes:approve";
        public const string AgreementRead = "crm:agreements:read";
        public const string AgreementWrite = "crm:agreements:write";
    }

    public static class Subscriptions
    {
        public const string SubscriptionRead = "subscriptions:read";
        public const string SubscriptionWrite = "subscriptions:write";
        public const string SubscriptionActivate = "subscriptions:activate";
        public const string SubscriptionSuspend = "subscriptions:suspend";
        public const string SubscriptionCancel = "subscriptions:cancel";
        public const string ProductRead = "subscriptions:products:read";
        public const string ProductWrite = "subscriptions:products:write";
    }

    public static class Catalog
    {
        public const string ProductRead = "catalog:products:read";
        public const string ProductWrite = "catalog:products:write";
        public const string CategoryRead = "catalog:categories:read";
        public const string CategoryWrite = "catalog:categories:write";
        public const string OfferRead = "catalog:offers:read";
        public const string OfferWrite = "catalog:offers:write";
        public const string SpecificationRead = "catalog:specifications:read";
        public const string SpecificationWrite = "catalog:specifications:write";
    }

    public static class Audit
    {
        public const string AuditRead = "audit:read";
        public const string AuditWrite = "audit:write";
        public const string AuditPurge = "audit:purge";
        public const string AuditAlertManage = "audit:alerts:manage";
        public const string AuditPolicyManage = "audit:policies:manage";
    }

    public static class Collections
    {
        public const string CaseRead = "collections:cases:read";
        public const string CaseWrite = "collections:cases:write";
        public const string DunningManage = "collections:dunning:manage";
        public const string ArrangementManage = "collections:arrangements:manage";
    }

    public static class Network
    {
        public const string ElementRead = "network:elements:read";
        public const string ElementWrite = "network:elements:write";
        public const string CapacityRead = "network:capacity:read";
        public const string TopologyRead = "network:topology:read";
    }

    public static class Notifications
    {
        public const string NotificationRead = "notifications:read";
        public const string NotificationSend = "notifications:send";
        public const string TemplateManage = "notifications:templates:manage";
        public const string PreferenceManage = "notifications:preferences:manage";
    }

    public static string PolicyName(string permission) => $"{PolicyPrefix}{permission}";
}
