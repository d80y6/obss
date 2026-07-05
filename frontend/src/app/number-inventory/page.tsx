"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Phone } from "lucide-react"
import { useRouter } from "next/navigation"
import { useTelephoneNumbers } from "@/api/hooks/useTelephoneNumbers"
import type { TelephoneNumberDto } from "@/api/generated"

export default function NumberInventoryPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(search ? { prefix: search } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useTelephoneNumbers(filters)

  const columns: Column<TelephoneNumberDto>[] = [
    { id: "number", header: "Number", accessorKey: "number", sortable: true },
    { id: "numberType", header: "Type", accessorKey: "numberType" },
    {
      id: "status",
      header: "Status",
      cell: (row) => <StatusBadge status={row.status} />,
      sortable: true,
    },
    { id: "customerId", header: "Customer", cell: (row) => row.customerId ?? "-" },
    {
      id: "cost",
      header: "Cost",
      cell: (row) => `${row.currency} ${row.cost.toFixed(2)}`,
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
        title="Number Inventory"
        createHref="/number-inventory/new"
        createLabel="Add Number"
      />

      <Card>
        <CardHeader className="pb-3">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search by number prefix..." />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No numbers found"
            emptyDescription="Get started by adding your first telephone number."
            emptyIcon={Phone}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/number-inventory/${row.id}`)}
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
