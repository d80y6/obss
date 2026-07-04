"use client"

import { useParams } from "next/navigation"
import { useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable } from "@/components/shared/DataTable"
import type { Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useProductSpecification } from "@/api/hooks/useProductSpecification"
import { useDeleteProductSpecification } from "@/api/hooks/useDeleteProductSpecification"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { toast } from "@/components/ui/toast"
import { queryKeys } from "@/lib/query-keys"
import { useQueryClient } from "@tanstack/react-query"
import type {
  ProductSpecificationCharacteristicDto,
  ProductSpecificationRelationshipDto,
} from "@/api/generated/dto"

export default function ProductSpecificationDetailPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: spec, isLoading } = useProductSpecification(id)
  const deleteSpec = useDeleteProductSpecification()

  const { data: auditEntries } = useAuditLog("ProductSpecification", id)

  const characteristicColumns: Column<ProductSpecificationCharacteristicDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "valueType", header: "ValueType", accessorKey: "valueType" },
    { id: "configurable", header: "Configurable", cell: (row) => row.configurable ? "Yes" : "No" },
    { id: "isRequired", header: "Required", cell: (row) => row.isRequired ? "Yes" : "No" },
    { id: "sortOrder", header: "SortOrder", accessorKey: "sortOrder" },
    {
      id: "values",
      header: "Values",
      cell: (row) =>
        row.values.length > 0 ? (
          <div className="space-y-1">
            {row.values.map((v) => (
              <div key={v.id} className="text-sm">
                <span className="font-medium">{v.value}</span>
                {v.unitOfMeasure && <span className="text-muted-foreground ml-1">{v.unitOfMeasure}</span>}
                {v.isDefault && <span className="text-xs text-muted-foreground ml-1">(default)</span>}
              </div>
            ))}
          </div>
        ) : (
          <span className="text-sm text-muted-foreground">-</span>
        ),
    },
  ]

  const relationshipColumns: Column<ProductSpecificationRelationshipDto>[] = [
    { id: "relationshipType", header: "RelationshipType", accessorKey: "relationshipType" },
    { id: "targetSpecificationId", header: "TargetSpec", accessorKey: "targetSpecificationId" },
    { id: "role", header: "Role", cell: (row) => row.role ?? "-" },
    {
      id: "validFrom",
      header: "ValidFrom",
      cell: (row) => (row.validFrom ? new Date(row.validFrom).toLocaleDateString() : "-"),
    },
    {
      id: "validTo",
      header: "ValidTo",
      cell: (row) => (row.validTo ? new Date(row.validTo).toLocaleDateString() : "-"),
    },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Product Specification Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: spec?.name ?? "-" },
            { label: "Description", value: spec?.description ?? "-" },
            { label: "Brand", value: spec?.brand ?? "-" },
            { label: "Version", value: spec?.version ?? "-" },
            { label: "Product Number", value: spec?.productNumber ?? "-" },
            { label: "Lifecycle Status", value: spec ? <StatusBadge status={spec.lifecycleStatus} /> : "-" },
            { label: "Created", value: spec?.createdAt ? new Date(spec.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "characteristics",
      label: "Characteristics",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Characteristics</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={characteristicColumns}
              data={spec?.characteristics ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "relationships",
      label: "Relationships",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Relationships</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={relationshipColumns}
              data={spec?.relationships ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
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
            {(auditEntries ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No audit entries found.</p>
            ) : (
              auditEntries?.map((entry) => (
                <div key={entry.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{entry.action}</span>
                    <span className="text-sm text-muted-foreground">{new Date(entry.performedAt).toLocaleString()}</span>
                  </div>
                  <p className="text-sm text-muted-foreground">By: {entry.performedByName ?? "-"}</p>
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
        title={spec?.name ?? "Product Specification"}
        subtitle={spec?.description ?? undefined}
        status={spec?.lifecycleStatus}
        backHref="/product-specifications"
        editHref={`/product-specifications/${id}/edit`}
        onDelete={() => {
          if (confirm("Are you sure you want to delete this product specification?")) {
            deleteSpec.mutate(id, {
              onSuccess: () => {
                toast({ title: "Product Specification deleted", description: "Product Specification has been deleted." })
                queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.lists() })
                router.push("/product-specifications")
              },
              onError: () => {
                toast({ title: "Error", description: "Failed to delete product specification.", variant: "destructive" })
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
