"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { FilterBar } from "@/components/shared/FilterBar"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useRefunds } from "@/api/hooks/useRefunds"
import type { RefundDto } from "@/api/generated/dto"
import { RotateCcw } from "lucide-react"

const statusOptions = [
  { label: "Pending", value: "PENDING" },
  { label: "Completed", value: "COMPLETED" },
  { label: "Failed", value: "FAILED" },
]

export default function RefundsPage() {
  const [statusFilter, setStatusFilter] = useState("")
  const [fromDate, setFromDate] = useState("")
  const [toDate, setToDate] = useState("")

  const filters: Record<string, string> = {
    ...(statusFilter ? { status: statusFilter } : {}),
    ...(fromDate ? { fromDate } : {}),
    ...(toDate ? { toDate } : {}),
  }

  const { data, isLoading, error } = useRefunds(filters)

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
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <FilterBar
              filters={[
                {
                  id: "status",
                  label: "Status",
                  type: "select",
                  options: statusOptions,
                  value: statusFilter,
                  onChange: (v: string) => setStatusFilter(v === "all" ? "" : v),
                  placeholder: "All Statuses",
                },
                {
                  id: "fromDate",
                  label: "From",
                  type: "date-range",
                  value: fromDate,
                  onChange: setFromDate,
                },
                {
                  id: "toDate",
                  label: "To",
                  type: "date-range",
                  value: toDate,
                  onChange: setToDate,
                },
              ]}
              onClear={() => { setStatusFilter(""); setFromDate(""); setToDate("") }}
            />
          </div>
        </CardHeader>
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
