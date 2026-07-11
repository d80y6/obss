"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import type { CategoryDto } from "@/api/generated"

export default function CategoryDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: category, isLoading } = useQuery({
    queryKey: ["product-categories", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/categories/${id}`)
      return res.data as CategoryDto
    },
    enabled: !!id,
  })

  const { data: categories } = useQuery({
    queryKey: ["product-categories"],
    queryFn: async () => {
      const res = await api.get("/api/v1/catalog/categories")
      return res.data as CategoryDto[]
    },
  })

  const parentCategory = categories?.find((c) => c.id === category?.parentCategoryId)

  const { data: auditEntries } = useAuditLog("Category", id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Category Details"
          loading={isLoading}
          columns={2}
          fields={[
            { label: "Name", value: category?.name ?? "-" },
            { label: "Description", value: category?.description ?? "-" },
            { label: "Parent Category", value: parentCategory?.name ?? "-" },
            { label: "Status", value: category ? <StatusBadge status={category.lifecycleStatus} /> : "-" },
            { label: "Is Root", value: category?.parentCategoryId === null ? "Yes" : "No" },
            { label: "Sort Order", value: category ? String(category.sortOrder) : "-" },
            { label: "Version", value: category ? String(category.version) : "-" },
            { label: "Created", value: category?.createdAt ? new Date(category.createdAt).toLocaleDateString() : "-" },
            { label: "Updated", value: category?.updatedAt ? new Date(category.updatedAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Audit Trail</CardTitle>
          </CardHeader>
          <CardContent>
            {(auditEntries ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No audit entries found.</p>
            ) : (
              auditEntries?.map((entry) => (
                <div key={entry.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{entry.action}</span>
                    <span className="text-sm text-muted-foreground">{new Date(entry.performedAt).toLocaleString()}</span>
                  </div>
                  <p className="text-sm text-muted-foreground">By: {entry.performedByName}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={category?.name ?? "Category"}
        subtitle={category?.description ?? undefined}
        status={category?.lifecycleStatus}
        backHref="/products/categories"
        editHref={`/products/categories/${id}/edit`}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
