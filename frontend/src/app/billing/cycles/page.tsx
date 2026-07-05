"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useBillingCycles } from "@/api/hooks/useBillingCycles"
import type { BillingCycleDto } from "@/api/generated"
import { Clock } from "lucide-react"
import { useRouter } from "next/navigation"

export default function BillingCyclesPage() {
  const router = useRouter()
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters: Record<string, string> = {
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useBillingCycles(filters)

  const columns: Column<BillingCycleDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "billingDate", header: "Billing Date", cell: (row) => new Date(row.billingDate).toLocaleDateString() },
    { id: "period", header: "Period", accessorKey: "period" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "customerCount", header: "Customer Count", cell: (row) => String(row.customerCount) },
    { id: "totalAmount", header: "Total Amount", cell: (row) => `${(row.totalAmount ?? 0).toLocaleString()}` },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Billing Cycles" backHref="/billing" createHref="/billing/cycles/new" createLabel="New Cycle" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No billing cycles"
            emptyIcon={Clock}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/billing/cycles/${row.id}`)}
            pagination={{ page, pageSize, total: data?.total ?? data?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
