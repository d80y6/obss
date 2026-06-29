"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useProducts } from "@/api/hooks/useProducts"
import type { ProductDto } from "@/api/generated"
import { Package, Tags, ListTree } from "lucide-react"
import { useRouter } from "next/navigation"
import Link from "next/link"

export default function ProductsPage() {
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

  const { data, isLoading, error } = useProducts(filters)

  const columns: Column<ProductDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "productType", header: "Product Type", accessorKey: "productType" },
    { id: "taxCategory", header: "Tax Category", accessorKey: "taxCategory" },
    {
      id: "status",
      header: "Status",
      cell: (row) => <StatusBadge status={row.status} />,
      sortable: true,
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
        title="Products"
        createHref="/products/new"
        createLabel="New Product"
        actions={
          <>
            <Button variant="outline" size="sm" asChild>
              <Link href="/products/offers">
                <Tags className="mr-1 h-4 w-4" /> Offers
              </Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/products/categories">
                <ListTree className="mr-1 h-4 w-4" /> Categories
              </Link>
            </Button>
          </>
        }
      />

      <Card>
        <CardHeader className="pb-3">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search products..." />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No products found"
            emptyDescription="Get started by creating your first product."
            emptyIcon={Package}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/products/${row.id}`)}
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
