"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useRefunds } from "@/api/hooks/useRefunds"
import type { RefundDto } from "@/api/generated/dto"
import { RotateCcw } from "lucide-react"

export default function RefundsPage() {
  const { data, isLoading, error } = useRefunds()

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
