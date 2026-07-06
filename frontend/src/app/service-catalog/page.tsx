"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useRouter } from "next/navigation"
import { useServiceSpecifications } from "@/api/hooks/use-service-catalog"
import type { ServiceSpecificationDto } from "@/api/hooks/use-service-catalog"
import { FolderTree, GitBranch } from "lucide-react"
import Link from "next/link"

export default function ServiceCatalogPage() {
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

  const { data, isLoading } = useServiceSpecifications(filters)

  const columns: Column<ServiceSpecificationDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "brand", header: "Brand", accessorKey: "brand" },
    { id: "version", header: "Version", accessorKey: "version" },
    { id: "lifecycleStatus", header: "Status", cell: (row) => <StatusBadge status={row.lifecycleStatus} /> },
    { id: "isBundle", header: "Bundle", cell: (row) => row.isBundle ? "Yes" : "No" },
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
        title="Service Catalog"
        description="Manage service specifications, categories, and candidates"
        createHref="/service-catalog/specifications/new"
        createLabel="New Specification"
        actions={
          <>
            <Button variant="outline" size="sm" asChild>
              <Link href="/service-catalog/categories">
                <FolderTree className="mr-1 h-4 w-4" /> Categories
              </Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/service-catalog/candidates">
                <GitBranch className="mr-1 h-4 w-4" /> Candidates
              </Link>
            </Button>
          </>
        }
      />
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <SearchBar placeholder="Search specifications..." value={search} onChange={(v) => { setSearch(v); setPage(1) }} />
            <FilterBar
              filters={filterConfig}
              onClear={() => { setStatusFilter(""); setPage(1) }}
            />
          </div>
          <div className="mt-4">
            <DataTable
              columns={columns}
              data={data?.items ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
              onRowClick={(row) => router.push(`/service-catalog/specifications/${row.id}`)}
              pagination={{
                page,
                pageSize,
                total: data?.total ?? 0,
                onPageChange: setPage,
                onPageSizeChange: (s) => { setPageSize(s); setPage(1) },
              }}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
