"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { RefundDto } from "@/types/api"
import { RotateCcw } from "lucide-react"

export default function RefundsPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["payment-refunds"],
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/refunds")
      return res.data as RefundDto[]
    },
  })

  const columns: Column<RefundDto>[] = [
    { id: "refundNumber", header: "Refund #", accessorKey: "refundNumber" },
    { id: "paymentNumber", header: "Payment", accessorKey: "paymentNumber" },
    { id: "amount", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.amount ?? 0).toLocaleString()}` },
    { id: "reason", header: "Reason", accessorKey: "reason" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Refunds" backHref="/payments" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No refunds"
            emptyIcon={RotateCcw}
            rowKey={(row) => row.id}
          />
        </CardContent>
      </Card>
    </div>
  )
}
