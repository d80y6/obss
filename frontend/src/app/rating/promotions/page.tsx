"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { usePromotions } from "@/api/hooks/use-rating"
import type { PromotionDto } from "@/api/generated"
import { Tag } from "lucide-react"
import { useRouter } from "next/navigation"
import { formatCurrency, formatDate } from "@/lib/formatters"

export default function PromotionsPage() {
  const router = useRouter()
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = usePromotions(filters)

  const columns: Column<PromotionDto>[] = [
    { id: "code", header: "Code", accessorKey: "code" },
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "discountType", header: "Discount Type", accessorKey: "discountType" },
    {
      id: "discountValue",
      header: "Value",
      cell: (row) => formatCurrency(row.discountValue),
      sortable: true,
    },
    {
      id: "isActive",
      header: "Status",
      cell: (row) => <StatusBadge status={row.isActive ? "Active" : "Inactive"} />,
    },
    {
      id: "validFrom",
      header: "Valid From",
      cell: (row) => formatDate(row.validFrom),
    },
    {
      id: "validTo",
      header: "Valid To",
      cell: (row) => (row.validTo ? formatDate(row.validTo) : "-"),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Promotions"
        createHref="/rating/promotions/new"
        createLabel="New Promotion"
        actions={
          <Button variant="outline" size="sm" asChild>
            <Link href="/rating/promotions/applicable">Check Applicable</Link>
          </Button>
        }
      />

      <Card>
        <CardHeader className="pb-3">
          <p className="text-sm text-muted-foreground">
            Manage promotional discounts and offers that can be applied to orders and subscriptions.
          </p>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No promotions"
            emptyDescription="Create your first promotion to offer discounts."
            emptyIcon={Tag}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/rating/promotions/${row.id}`)}
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
