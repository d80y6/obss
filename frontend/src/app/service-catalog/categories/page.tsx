"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { useRouter } from "next/navigation"
import { useServiceCategories } from "@/api/hooks/use-service-catalog"
import type { ServiceCategoryDto } from "@/api/hooks/use-service-catalog"

export default function ServiceCategoriesPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading } = useServiceCategories(filters)

  const columns: Column<ServiceCategoryDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "lifecycleStatus", header: "Status", cell: (row) => <StatusBadge status={row.lifecycleStatus} /> },
    { id: "isRoot", header: "Root", cell: (row) => row.isRoot ? "Yes" : "No" },
    { id: "version", header: "Version", accessorKey: "version" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Service Categories"
        description="Organize service specifications into categories"
        createHref="/service-catalog/categories/new"
        createLabel="New Category"
        backHref="/service-catalog"
      />
      <SearchBar placeholder="Search categories..." value={search} onChange={(v) => { setSearch(v); setPage(1) }} />
      <DataTable
        columns={columns}
        data={data?.items ?? []}
        loading={isLoading}
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/service-catalog/categories/${row.id}`)}
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
