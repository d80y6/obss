"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { usePayment } from "@/api/hooks/usePayment"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { useCompletePayment } from "@/api/hooks/useCompletePayment"
import { formatCurrency } from "@/lib/formatters"
import { toast } from "@/components/ui/toast"
import Link from "next/link"
import { CheckCircle, RotateCcw } from "lucide-react"

export default function PaymentDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: payment, isLoading } = usePayment(id)

  const { data: auditEntries } = useAuditLog("Payment", id)

  const completeMutation = useCompletePayment(id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Payment Details"
          loading={isLoading}
          fields={[
            { label: "Payment #", value: payment?.paymentNumber ?? "-" },
            { label: "Invoice", value: payment?.invoiceNumber ?? "-" },
            { label: "Customer", value: payment?.customerName ?? "-" },
            { label: "Amount", value: payment ? formatCurrency(payment.amount, payment.currency) : "-" },
            { label: "Method", value: payment?.paymentMethod?.replace(/_/g, " ") ?? "-" },
            { label: "Status", value: payment ? <StatusBadge status={payment.status} /> : "-" },
            { label: "Transaction ID", value: payment?.transactionId ?? "-" },
            { label: "Paid At", value: payment?.paidAt ? new Date(payment.paidAt).toLocaleString() : "-" },
            { label: "Created", value: payment?.createdAt ? new Date(payment.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Audit Trail</CardTitle>
          </CardHeader>
          <CardContent>
            {(auditEntries ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No audit entries.</p>
            ) : (
              auditEntries?.map((entry) => (
                <div key={entry.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{entry.action}</span>
                    <span className="text-sm text-muted-foreground">{new Date(entry.performedAt).toLocaleString()}</span>
                  </div>
                  <p className="text-sm text-muted-foreground">By: {entry.performedByName}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={`Payment ${payment?.paymentNumber ?? ""}`}
        subtitle={payment?.invoiceNumber ? `Invoice ${payment.invoiceNumber}` : undefined}
        status={payment?.status}
        backHref="/payments"
        loading={isLoading}
      />
      {payment && payment.status === "PENDING" && (
        <div className="flex gap-2">
          <Button variant="default" size="sm" onClick={() => {
            completeMutation.mutate(undefined, {
              onSuccess: () => toast({ title: "Payment completed" }),
              onError: () => toast({ title: "Failed to complete payment", variant: "destructive" }),
            })
          }}>
            <CheckCircle className="mr-1 h-4 w-4" /> Complete
          </Button>
        </div>
      )}
      {payment && payment.status !== "REFUNDED" && (
        <Button variant="outline" size="sm" asChild>
          <Link href={`/payments/${id}/refund`}>
            <RotateCcw className="mr-1 h-4 w-4" /> Refund
          </Link>
        </Button>
      )}
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
