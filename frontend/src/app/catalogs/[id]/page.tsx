"use client"

import { useParams, useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery, useQueryClient } from "@tanstack/react-query"
import { useMutation } from "@tanstack/react-query"
import api from "@/services/api"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { toast } from "@/components/ui/toast"
import type { CatalogDto } from "@/api/generated"

export default function CatalogDetailPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: catalog, isLoading } = useQuery({
    queryKey: ["catalogs", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/catalogs/${id}`)
      return res.data as CatalogDto
    },
    enabled: !!id,
  })

  const { data: auditEntries } = useAuditLog("Catalog", id)

  const deleteMutation = useMutation({
    mutationFn: async () => {
      await api.delete(`/api/v1/catalog/catalogs/${id}`)
    },
    onSuccess: () => {
      toast({ title: "Catalog deleted", description: "Catalog has been deleted." })
      queryClient.invalidateQueries({ queryKey: ["catalogs"] })
      router.push("/catalogs")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to delete catalog.", variant: "destructive" })
    },
  })

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Catalog Details"
          loading={isLoading}
          columns={2}
          fields={[
            { label: "Name", value: catalog?.name ?? "-" },
            { label: "Description", value: catalog?.description ?? "-" },
            { label: "Catalog Type", value: catalog?.catalogType ?? "-" },
            { label: "Version", value: catalog ? String(catalog.version) : "-" },
            { label: "Status", value: catalog ? <StatusBadge status={catalog.lifecycleStatus} /> : "-" },
            { label: "Valid From", value: catalog?.validFrom ? new Date(catalog.validFrom).toLocaleDateString() : "-" },
            { label: "Valid To", value: catalog?.validTo ? new Date(catalog.validTo).toLocaleDateString() : "-" },
            { label: "Created", value: catalog?.createdAt ? new Date(catalog.createdAt).toLocaleDateString() : "-" },
            { label: "Updated", value: catalog?.updatedAt ? new Date(catalog.updatedAt).toLocaleDateString() : "-" },
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
        title={catalog?.name ?? "Catalog"}
        subtitle={catalog?.description ?? undefined}
        status={catalog?.lifecycleStatus}
        backHref="/catalogs"
        editHref={`/catalogs/${id}/edit`}
        onDelete={() => {
          if (confirm("Are you sure you want to delete this catalog?")) {
            deleteMutation.mutate()
          }
        }}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
