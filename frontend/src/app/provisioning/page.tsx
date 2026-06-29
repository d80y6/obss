"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useProvisioningJobs } from "@/api/hooks/use-provisioning-jobs"
import { ProvisioningJobDto } from "@/types/api"
import { Settings, FileJson } from "lucide-react"
import { useRouter } from "next/navigation"
import Link from "next/link"

export default function ProvisioningPage() {
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
    ...(typeFilter ? { type: typeFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }
  const { data, isLoading, error } = useProvisioningJobs(filters)

  const columns: Column<ProvisioningJobDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "type", header: "Type", accessorKey: "type" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "startedAt", header: "Started", cell: (row) => row.startedAt ? new Date(row.startedAt).toLocaleString() : "-" },
    { id: "completedAt", header: "Completed", cell: (row) => row.completedAt ? new Date(row.completedAt).toLocaleString() : "-" },
  ]

  const filterConfig = [
    { id: "status", label: "Status", type: "select" as const, value: statusFilter, onChange: (v: string) => { setStatusFilter(v === "all" ? "" : v); setPage(1) }, options: [{ label: "Pending", value: "pending" }, { label: "Running", value: "running" }, { label: "Completed", value: "completed" }, { label: "Failed", value: "failed" }] },
    { id: "type", label: "Type", type: "select" as const, value: typeFilter, onChange: (v: string) => { setTypeFilter(v === "all" ? "" : v); setPage(1) }, options: [] },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Provisioning"
        createHref="/provisioning/jobs/new"
        createLabel="New Job"
        actions={
          <Button variant="outline" size="sm" asChild>
            <Link href="/provisioning/templates"><FileJson className="mr-1 h-4 w-4" /> Templates</Link>
          </Button>
        }
      />
      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search jobs..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setTypeFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No provisioning jobs"
            emptyIcon={Settings}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/provisioning/jobs/${row.id}`)}
            pagination={{ page, pageSize, total: data?.total ?? data?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
