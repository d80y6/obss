"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useSubscriptions } from "@/api/hooks/useSubscriptions"
import type { SubscriptionDto } from "@/api/generated"
import { ClipboardList } from "lucide-react"
import { useRouter } from "next/navigation"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { toast } from "@/components/ui/toast"
import { queryKeys } from "@/lib/query-keys"

const statusOptions = [
  { label: "Active", value: "ACTIVE" },
  { label: "Suspended", value: "SUSPENDED" },
  { label: "Cancelled", value: "CANCELLED" },
  { label: "Expired", value: "EXPIRED" },
]

export default function SubscriptionsPage() {
  const router = useRouter()
  const [statusFilter, setStatusFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(statusFilter ? { status: statusFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useSubscriptions(filters)
  const queryClient = useQueryClient()

  const bulkMutation = useMutation({
    mutationFn: async ({ ids, action, body }: { ids: string[]; action: string; body?: Record<string, unknown> }) => {
      const promises = ids.map((id) => api.post(`/api/v1/subscriptions/subscriptions/${id}/${action}`, body))
      await Promise.all(promises)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.lists() })
      toast({ title: "Success", description: `${variables.ids.length} items updated.` })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to perform action.", variant: "destructive" })
    },
  })

  const columns: Column<SubscriptionDto>[] = [
    { id: "id", header: "ID", cell: (row) => row.id.slice(0, 8) },
    { id: "customerName", header: "Customer", accessorKey: "customerName" },
    { id: "offerName", header: "Offer", accessorKey: "offerName" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "price", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.price ?? 0).toLocaleString()}` },
    { id: "startDate", header: "Start", cell: (row) => new Date(row.startDate).toLocaleDateString() },
  ]

  const filterConfig = [
    {
      id: "status",
      label: "Status",
      type: "select" as const,
      options: statusOptions,
      value: statusFilter,
      onChange: (v: string) => { setStatusFilter(v === "all" ? "" : v); setPage(1) },
      placeholder: "All Statuses",
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Subscriptions" />

      <Card>
        <CardHeader className="pb-3">
          <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setPage(1) }} />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No subscriptions found"
            emptyIcon={ClipboardList}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/subscriptions/${row.id}`)}
            pagination={{
              page,
              pageSize,
              total: data?.total ?? data?.items?.length ?? 0,
              onPageChange: setPage,
              onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
            }}
            bulkActions={[
              { label: "Activate", onClick: (ids) => bulkMutation.mutate({ ids, action: "activate" }) },
              { label: "Suspend", onClick: (ids) => bulkMutation.mutate({ ids, action: "suspend" }), variant: "destructive" },
              { label: "Cancel Selected", onClick: (ids) => bulkMutation.mutate({ ids, action: "cancel", body: { reason: "Bulk cancellation", effectiveDate: new Date().toISOString() } }), variant: "destructive" },
            ]}
          />
        </CardContent>
      </Card>
    </div>
  )
}
