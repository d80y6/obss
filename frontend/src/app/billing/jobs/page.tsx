"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { BillingJobDto } from "@/types/api"
import { Settings } from "lucide-react"
import { useRouter } from "next/navigation"

export default function BillingJobsPage() {
  const router = useRouter()
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const { data, isLoading, error } = useQuery({
    queryKey: ["billing-jobs", page, pageSize],
    queryFn: async () => {
      const res = await api.get(`/api/v1/billing/jobs?page=${page}&pageSize=${pageSize}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as BillingJobDto[], total }
    },
  })

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
