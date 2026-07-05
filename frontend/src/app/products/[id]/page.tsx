"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useProduct } from "@/api/hooks/useProduct"
import { useQuery, useQueryClient } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { useDeleteProduct } from "@/api/hooks/useDeleteProduct"
import { toast } from "@/components/ui/toast"
import type { OfferDto } from "@/api/generated"

export default function ProductDetailPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: product, isLoading } = useProduct(id)
  const deleteProduct = useDeleteProduct()

  const { data: offers, error: offersError } = useQuery({
    queryKey: queryKeys.products.offers(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/offers?productId=${id}`)
      return res.data as OfferDto[]
    },
    enabled: !!id,
  })

  const { data: auditEntries, error: auditError } = useAuditLog("Product", id)

  const offerColumns: Column<OfferDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "offerType", header: "Type", accessorKey: "offerType" },
    { id: "billingPeriod", header: "Billing", accessorKey: "billingPeriod" },
    { id: "price", header: "Price", cell: (row) => {
      const p = row.pricings?.[0]
      return p ? `${p.currency} ${(p.recurringPrice || p.oneTimePrice || p.usagePrice || 0).toFixed(2)}` : "-"
    }},
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.isActive ? "Active" : "Inactive"} /> },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Product Details"
          loading={isLoading}
          columns={2}
          fields={[
            { label: "Name", value: product?.name ?? "-" },
            { label: "Description", value: product?.description ?? "-" },
            { label: "Product Type", value: product?.productType ?? "-" },
            { label: "Tax Category", value: product?.taxCategory ?? "-" },
            { label: "Status", value: product ? <StatusBadge status={product.lifecycleStatus} /> : "-" },
            { label: "Created", value: product?.createdAt ? new Date(product.createdAt).toLocaleDateString() : "-" },
            { label: "Updated", value: product?.updatedAt ? new Date(product.updatedAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "offers",
      label: `Offers (${(offers ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Offers</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={offerColumns}
              data={offers ?? []}
              emptyTitle="No offers"
              rowKey={(row) => row.id}
              error={offersError ? "Failed to load data." : undefined}
            />
          </CardContent>
        </Card>
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
            <DataTable
              columns={[
                { id: "action", header: "Action", accessorKey: "action" },
                { id: "performedByName", header: "Actor", accessorKey: "performedByName" },
                { id: "performedAt", header: "Timestamp", cell: (row) => row.performedAt ? new Date(row.performedAt).toLocaleString() : "-" },
              ]}
              data={auditEntries ?? []}
              emptyTitle="No audit entries"
              rowKey={(row) => row.id}
              error={auditError ? "Failed to load data." : undefined}
            />
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={product?.name ?? "Product"}
        subtitle={product?.description ?? undefined}
        status={product?.lifecycleStatus}
        backHref="/products"
        editHref={`/products/${id}/edit`}
        onDelete={() => {
          if (confirm("Are you sure you want to delete this product?")) {
            deleteProduct.mutate(id, {
              onSuccess: () => {
                toast({ title: "Product deleted", description: "Product has been deleted." })
                queryClient.invalidateQueries({ queryKey: queryKeys.products.lists() })
                router.push("/products")
              },
              onError: () => {
                toast({ title: "Error", description: "Failed to delete product.", variant: "destructive" })
              },
            })
          }
        }}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
