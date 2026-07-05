"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useOffers } from "@/api/hooks/useOffers"
import type { OfferDto } from "@/api/generated"
import { Tags } from "lucide-react"
import { useRouter } from "next/navigation"

export default function OffersPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(search ? { searchTerm: search } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useOffers(filters)

  const columns: Column<OfferDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "offerType", header: "Type", accessorKey: "offerType" },
    { id: "price", header: "Price", cell: (row) => {
      const p = row.pricings?.[0]
      return p ? `${p.currency} ${(p.recurringPrice || p.oneTimePrice || p.usagePrice || 0).toFixed(2)}` : "-"
    }},
    { id: "billingPeriod", header: "Billing Period", accessorKey: "billingPeriod" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.isActive ? "Active" : "Inactive"} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Offers" backHref="/products" createHref="/products/offers/new" createLabel="New Offer" />

      <Card>
        <CardHeader className="pb-3">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search offers..." />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No offers found"
            emptyIcon={Tags}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/products/offers/${row.id}`)}
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
