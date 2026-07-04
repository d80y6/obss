"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useProductSpecifications } from "@/api/hooks/useProductSpecifications"
import type { ProductSpecificationDto } from "@/api/generated"

export default function ProductSpecificationsPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters: Record<string, string> = {
    ...(search ? { SearchTerm: search } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useProductSpecifications(filters)

  const columns: Column<ProductSpecificationDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    {
      id: "brand",
      header: "Brand",
      cell: (row) => row.brand ?? "-",
    },
    {
      id: "version",
      header: "Version",
      cell: (row) => row.version ?? "-",
    },
    {
      id: "productNumber",
      header: "Product Number",
      cell: (row) => row.productNumber ?? "-",
    },
    {
      id: "status",
      header: "Status",
      cell: (row) => <StatusBadge status={row.lifecycleStatus} />,
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Product Specifications"
        description="Manage product specifications, characteristics, and relationships"
        createHref="/product-specifications/new"
        createLabel="New Specification"
      />

      <Card>
        <CardHeader className="pb-3">
          <SearchBar
            placeholder="Search specifications..."
            value={search}
            onChange={(v) => { setSearch(v); setPage(1) }}
          />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No specifications found"
            emptyDescription="Get started by creating your first specification."
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/product-specifications/${row.id}`)}
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
