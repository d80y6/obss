"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useOffers } from "@/api/hooks/useOffers"
import { OfferDto } from "@/types/api"
import { Tags } from "lucide-react"
import { useRouter } from "next/navigation"

export default function OffersPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading } = useOffers()

  const columns: Column<OfferDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "productName", header: "Product", accessorKey: "productName" },
    { id: "price", header: "Price", cell: (row) => `${row.currency} ${row.price}` },
    { id: "billingPeriod", header: "Billing Period", accessorKey: "billingPeriod" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Offers" backHref="/products" createHref="/products/offers/new" createLabel="New Offer" />

      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No offers found"
            emptyIcon={Tags}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/products/offers/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
