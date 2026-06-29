"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { CategoryDto } from "@/types/api"
import { ListTree } from "lucide-react"
import { useRouter } from "next/navigation"

export default function CategoriesPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading, error } = useQuery({
    queryKey: ["product-categories"],
    queryFn: async () => {
      const res = await api.get("/api/v1/catalog/categories")
      return res.data as CategoryDto[]
    },
  })

  const columns: Column<CategoryDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "description", header: "Description", accessorKey: "description" },
    { id: "productCount", header: "Products", cell: (row) => String(row.productCount) },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Categories" backHref="/products" createHref="/products/categories/new" createLabel="New Category" />

      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No categories found"
            emptyIcon={ListTree}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/products/categories/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
