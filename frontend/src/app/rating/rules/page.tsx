"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useRatingRules } from "@/api/hooks/use-rating"
import type { RatingRuleDto } from "@/api/generated"
import { Scale } from "lucide-react"
import { useRouter } from "next/navigation"

export default function RatingRulesPage() {
  const router = useRouter()
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useRatingRules(filters)

  const columns: Column<RatingRuleDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "description", header: "Description", accessorKey: "description" },
    {
      id: "priority",
      header: "Priority",
      accessorKey: "priority",
      sortable: true,
    },
    {
      id: "isActive",
      header: "Status",
      cell: (row) => <StatusBadge status={row.isActive ? "Active" : "Inactive"} />,
    },
    {
      id: "createdAt",
      header: "Created",
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Rating Rules"
        createHref="/rating/rules/new"
        createLabel="New Rule"
      />

      <Card>
        <CardHeader className="pb-3">
          <p className="text-sm text-muted-foreground">
            Configure rating rules that determine how usage records are rated and charged.
          </p>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No rating rules"
            emptyDescription="Create your first rating rule to start rating usage."
            emptyIcon={Scale}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/rating/rules/${row.id}`)}
            pagination={{
              page,
              pageSize,
              total: data?.length ?? 0,
              onPageChange: setPage,
              onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
            }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
