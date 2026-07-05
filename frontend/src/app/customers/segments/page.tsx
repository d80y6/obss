"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { SearchBar } from "@/components/shared/SearchBar"
import { useSegments } from "@/api/hooks/useSegments"
import type { SegmentDto } from "@/api/generated"
import { Users } from "lucide-react"
import { useRouter } from "next/navigation"

export default function SegmentsPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useSegments(filters)

  const columns: Column<SegmentDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "description", header: "Description", accessorKey: "description" },
    { id: "customerCount", header: "Customers", cell: (row) => String(row.customerCount) },
    { id: "createdAt", header: "Created", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Segments" backHref="/customers" createHref="/customers/segments/new" createLabel="New Segment" />

      <Card>
        <CardHeader className="pb-3">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search segments..." />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No segments found"
            emptyDescription="No segments match the current filters."
            emptyIcon={Users}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/customers/segments/${row.id}`)}
            pagination={{
              page,
              pageSize,
              total: data?.total ?? data?.items?.length ?? 0,
              onPageChange: setPage,
              onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
            }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
