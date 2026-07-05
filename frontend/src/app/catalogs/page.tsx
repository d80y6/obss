"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { Book } from "lucide-react"
import { useRouter } from "next/navigation"
import type { CatalogDto } from "@/api/generated"

export default function CatalogsPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading, error } = useQuery({
    queryKey: ["catalogs", search, page, pageSize],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set("searchTerm", search)
      params.set("page", String(page))
      params.set("pageSize", String(pageSize))
      const res = await api.get(`/api/v1/catalog/catalogs?${params}`)
      return {
        items: res.data as CatalogDto[],
        total: Number(res.headers["x-total-count"] || res.data.length),
      }
    },
  })

  const columns: Column<CatalogDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "catalogType", header: "Type", accessorKey: "catalogType" },
    {
      id: "status",
      header: "Status",
      cell: (row) => <StatusBadge status={row.lifecycleStatus} />,
    },
    {
      id: "createdAt",
      header: "Created",
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Catalogs" createHref="/catalogs/new" createLabel="New Catalog" />

      <Card>
        <CardHeader className="pb-3">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search catalogs..." />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No catalogs found"
            emptyDescription="Get started by creating your first catalog."
            emptyIcon={Book}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/catalogs/${row.id}`)}
            pagination={{
              page,
              pageSize,
              total: data?.total ?? 0,
              onPageChange: setPage,
              onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
            }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
