"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useCollectionCases } from "@/api/hooks/use-collections"
import type { CollectionCaseDto } from "@/api/generated"
import { Landmark } from "lucide-react"
import { useRouter } from "next/navigation"

const statusOptions = [
  { label: "Open", value: "OPEN" },
  { label: "In Progress", value: "IN_PROGRESS" },
  { label: "Resolved", value: "RESOLVED" },
  { label: "Closed", value: "CLOSED" },
]

export default function CollectionsPage() {
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

  const { data, isLoading, error } = useCollectionCases(filters)

  const columns: Column<CollectionCaseDto>[] = [
    { id: "id", header: "Case ID", cell: (row) => row.id.substring(0, 8) + "..." },
    { id: "customerName", header: "Customer", accessorKey: "customerName" },
    { id: "totalOverdueAmount", header: "Total Debt", cell: (row) => `${row.currency ?? ""} ${(row.totalOverdueAmount ?? 0).toLocaleString()}` },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "assignedTo", header: "Assigned To", cell: (row) => row.assignedTo || "-" },
    { id: "openedAt", header: "Created", cell: (row) => new Date(row.openedAt).toLocaleDateString() },
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
        title="Collections"
        createHref="/collections/new"
        createLabel="New Case"
      />
      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search cases..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
             data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No collection cases"
            emptyIcon={Landmark}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/collections/${row.id}`)}
            pagination={{ page, pageSize, total: data?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
