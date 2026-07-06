"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { useRouter } from "next/navigation"
import { useServiceCandidates } from "@/api/hooks/use-service-catalog"
import type { ServiceCandidateDto } from "@/api/hooks/use-service-catalog"

export default function ServiceCandidatesPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(statusFilter ? { status: statusFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading } = useServiceCandidates(filters)

  const columns: Column<ServiceCandidateDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "lifecycleStatus", header: "Status", cell: (row) => <StatusBadge status={row.lifecycleStatus} /> },
    { id: "serviceSpecificationName", header: "Specification", accessorKey: "serviceSpecificationName" },
    { id: "featureSpecification", header: "Features", accessorKey: "featureSpecification" },
  ]

  const filterConfig = [
    {
      id: "status",
      label: "Status",
      type: "select" as const,
      options: [
        { label: "Draft", value: "Draft" },
        { label: "Active", value: "Active" },
        { label: "Retired", value: "Retired" },
      ],
      value: statusFilter,
      onChange: (v: string) => { setStatusFilter(v === "all" ? "" : v); setPage(1) },
      placeholder: "All Statuses",
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Service Candidates"
        description="Service specification candidates for different contexts"
        createHref="/service-catalog/candidates/new"
        createLabel="New Candidate"
        backHref="/service-catalog"
      />
      <div className="flex items-center gap-4">
        <SearchBar placeholder="Search candidates..." value={search} onChange={(v) => { setSearch(v); setPage(1) }} />
        <FilterBar
          filters={filterConfig}
          onClear={() => { setStatusFilter(""); setPage(1) }}
        />
      </div>
      <DataTable
        columns={columns}
        data={data?.items ?? []}
        loading={isLoading}
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/service-catalog/candidates/${row.id}`)}
        pagination={{
          page,
          pageSize,
          total: data?.total ?? 0,
          onPageChange: setPage,
          onPageSizeChange: (s) => { setPageSize(s); setPage(1) },
        }}
      />
    </div>
  )
}
