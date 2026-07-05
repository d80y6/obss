"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useBillingJobs } from "@/api/hooks/useBillingJobs"
import type { BillingJobDto } from "@/api/generated"
import { Settings } from "lucide-react"
import { useRouter } from "next/navigation"

export default function BillingJobsPage() {
  const router = useRouter()
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters: Record<string, string> = {
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useBillingJobs(filters)

  const columns: Column<BillingJobDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "type", header: "Type", accessorKey: "type" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "totalProcessed", header: "Processed", cell: (row) => String(row.totalProcessed) },
    { id: "totalErrors", header: "Errors", cell: (row) => String(row.totalErrors) },
    { id: "startedAt", header: "Started", cell: (row) => row.startedAt ? new Date(row.startedAt).toLocaleString() : "-" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Billing Jobs" backHref="/billing" createHref="/billing/jobs/new" createLabel="New Job" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No billing jobs"
            emptyIcon={Settings}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/billing/jobs/${row.id}`)}
            pagination={{ page, pageSize, total: data?.total ?? data?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
