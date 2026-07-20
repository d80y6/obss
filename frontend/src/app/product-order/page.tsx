"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useProductOrders } from "@/api/hooks/useProductOrders"
import type { ProductOrderDto } from "@/api/hooks/useProductOrders"
import { ShoppingCart } from "lucide-react"
import { useRouter } from "next/navigation"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { toast } from "@/components/ui/toast"
import { queryKeys } from "@/lib/query-keys"

const statusOptions = [
  { label: "Active", value: "ACTIVE" },
  { label: "Pending", value: "PENDING" },
  { label: "Completed", value: "COMPLETED" },
  { label: "Cancelled", value: "CANCELLED" },
  { label: "Suspended", value: "SUSPENDED" },
]

export default function ProductOrdersPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")
  const [dateFrom, setDateFrom] = useState("")
  const [dateTo, setDateTo] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(statusFilter ? { status: statusFilter } : {}),
    ...(dateFrom ? { dateFrom } : {}),
    ...(dateTo ? { dateTo } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useProductOrders(filters)
  const queryClient = useQueryClient()

  const bulkMutation = useMutation({
    mutationFn: async ({ ids, action }: { ids: string[]; action: string }) => {
      const promises = ids.map((id) => api.post(`/api/v1/productOrder/orders/${id}/${action}`))
      await Promise.all(promises)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.lists() })
      toast({ title: "Success", description: `${variables.ids.length} items updated.` })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to perform action.", variant: "destructive" })
    },
  })

  const columns: Column<ProductOrderDto>[] = [
    { id: "orderNumber", header: "Order #", accessorKey: "orderNumber", sortable: true },
    { id: "customerName", header: "Customer", accessorKey: "customerName" },
    { id: "grandTotal", header: "Total", cell: (row) => `${row.currency ?? ""} ${(row.grandTotal ?? 0).toLocaleString()}` },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "createdAt", header: "Date", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
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
    {
      id: "dateFrom",
      label: "From",
      type: "date-range" as const,
      value: dateFrom,
      onChange: (v: string) => { setDateFrom(v); setPage(1) },
    },
    {
      id: "dateTo",
      label: "To",
      type: "date-range" as const,
      value: dateTo,
      onChange: (v: string) => { setDateTo(v); setPage(1) },
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Orders" createHref="/orders/new" createLabel="New Order" />

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search orders..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setDateFrom(""); setDateTo(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No orders found"
            emptyIcon={ShoppingCart}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/orders/${row.id}`)}
            pagination={{
              page,
              pageSize,
              total: data?.total ?? data?.items?.length ?? 0,
              onPageChange: setPage,
              onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
            }}
            bulkActions={[
              { label: "Cancel", onClick: (ids) => bulkMutation.mutate({ ids, action: "cancel" }), variant: "destructive" },
              { label: "Approve", onClick: (ids) => bulkMutation.mutate({ ids, action: "approve" }) },
            ]}
          />
        </CardContent>
      </Card>
    </div>
  )
}
