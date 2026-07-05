"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { usePaymentSummary } from "@/api/hooks/usePaymentSummary"
import { DollarSign, TrendingUp, TrendingDown, Activity } from "lucide-react"

export default function PaymentSummaryPage() {
  const { data, isLoading } = usePaymentSummary()

  const stats = [
    { label: "Total Payments", value: data?.totalPayments ?? 0, icon: Activity, color: "text-blue-600" },
    { label: "Total Amount", value: data ? `${data.currency ?? "USD"} ${(data.totalAmount ?? 0).toLocaleString()}` : "-", icon: DollarSign, color: "text-green-600" },
    { label: "Completed", value: data ? `${data.currency ?? "USD"} ${(data.totalCompletedAmount ?? 0).toLocaleString()}` : "-", icon: TrendingUp, color: "text-emerald-600" },
    { label: "Refunded", value: data ? `${data.currency ?? "USD"} ${(data.totalRefundedAmount ?? 0).toLocaleString()}` : "-", icon: TrendingDown, color: "text-red-600" },
    { label: "Net", value: data ? `${data.currency ?? "USD"} ${(data.netAmount ?? 0).toLocaleString()}` : "-", icon: DollarSign, color: "text-purple-600" },
    { label: "Pending", value: data?.pendingCount ?? 0, icon: Activity, color: "text-amber-600" },
    { label: "Failed", value: data?.failedCount ?? 0, icon: Activity, color: "text-red-600" },
    { label: "Refunded (count)", value: data?.refundedCount ?? 0, icon: TrendingDown, color: "text-orange-600" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Payment Summary" backHref="/payments" />
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((s) => (
          <Card key={s.label}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium">{s.label}</CardTitle>
              <s.icon className={`h-4 w-4 ${s.color}`} />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{isLoading ? "..." : s.value}</div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
