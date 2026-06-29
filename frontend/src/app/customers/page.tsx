"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useCustomers } from "@/api/hooks/useCustomers"
import { CustomerDto } from "@/types/api"
import { Users } from "lucide-react"
import { useRouter } from "next/navigation"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { toast } from "@/components/ui/toast"
import { queryKeys } from "@/lib/query-keys"

const statusOptions = [
  { label: "Active", value: "ACTIVE" },
  { label: "Inactive", value: "INACTIVE" },
  { label: "Suspended", value: "SUSPENDED" },
]

export default function CustomersPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(statusFilter ? { status: statusFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useCustomers(filters)
  const queryClient = useQueryClient()

  const bulkMutation = useMutation({
    mutationFn: async ({ ids, action }: { ids: string[]; action: string }) => {
      const promises = ids.map((id) => api.post(`/api/v1/crm/customers/${id}/${action}`))
      await Promise.all(promises)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.lists() })
      toast({ title: "Success", description: `${variables.ids.length} items updated.` })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to perform action.", variant: "destructive" })
    },
  })

  const columns: Column<CustomerDto>[] = [
    { id: "displayName", header: "Name", accessorKey: "displayName", sortable: true },
    { id: "email", header: "Email", accessorKey: "email" },
    { id: "phoneNumber", header: "Phone", accessorKey: "phoneNumber" },
    { id: "customerType", header: "Type", accessorKey: "customerType" },
    {
      id: "status",
      header: "Status",
      cell: (row) => <StatusBadge status={row.status} />,
      sortable: true,
    },
    {
      id: "createdAt",
      header: "Created",
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
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
      <PageHeader
        title="Customers"
        createHref="/customers/new"
        createLabel="New Customer"
      />

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search customers..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No customers found"
            emptyDescription="No customers match the current filters."
            emptyIcon={Users}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/customers/${row.id}`)}
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
            ]}
          />
        </CardContent>
      </Card>
    </div>
  )
}
