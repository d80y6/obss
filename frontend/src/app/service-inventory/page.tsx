"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useRouter } from "next/navigation"
import { useServices } from "@/api/hooks/use-service-inventory"
import type { ServiceDto } from "@/api/generated"
import { Network } from "lucide-react"

export default function ServiceInventoryPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")
  const [typeFilter, setTypeFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters = {
    ...(search ? { search } : {}),
    ...(statusFilter ? { status: statusFilter } : {}),
    ...(typeFilter ? { serviceType: typeFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }
  const { data, isLoading, error } = useServices(filters)

  const columns: Column<ServiceDto>[] = [
    { id: "serviceIdentifier", header: "Name", accessorKey: "serviceIdentifier", sortable: true },
    { id: "serviceType", header: "Type", accessorKey: "serviceType" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "customerId", header: "Customer", accessorKey: "customerId" },
    { id: "subscriptionId", header: "Subscription", accessorKey: "subscriptionId" },
  ]

  const filterConfig = [
    { id: "status", label: "Status", type: "select" as const, value: statusFilter, onChange: (v: string) => { setStatusFilter(v === "all" ? "" : v); setPage(1) }, options: [{ label: "Active", value: "active" }, { label: "Suspended", value: "suspended" }, { label: "Decommissioned", value: "decommissioned" }] },
    { id: "type", label: "Type", type: "select" as const, value: typeFilter, onChange: (v: string) => { setTypeFilter(v === "all" ? "" : v); setPage(1) }, options: [] },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Service Inventory" createHref="/service-inventory/new" createLabel="New Service" />
      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search services..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setTypeFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No services found"
            emptyIcon={Network}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/service-inventory/${row.id}`)}
            pagination={{ page, pageSize, total: data?.total ?? data?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
