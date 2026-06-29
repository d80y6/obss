"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { TenantDto, AuditEntryDto } from "@/types/api"

export default function TenantDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: tenant, isLoading, error: tenantError } = useQuery({
    queryKey: queryKeys.tenants.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/iam/tenants/${id}`)
      return res.data as TenantDto
    },
    enabled: !!id,
  })

  const { data: auditEntries, error: auditError } = useQuery({
    queryKey: queryKeys.audit.entity("Tenant", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/Tenant/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Tenant Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: tenant?.name ?? "-" },
            { label: "Slug", value: tenant?.slug ?? "-" },
            { label: "Status", value: tenant ? <StatusBadge status={tenant.isActive ? "ACTIVE" : "INACTIVE"} /> : "-" },
            { label: "Settings", value: tenant?.settings ?? "-" },
            { label: "Created", value: tenant?.createdAt ? new Date(tenant.createdAt).toLocaleDateString() : "-" },
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
        title={tenant?.name ?? "Tenant"}
        subtitle={tenant?.slug}
        status={tenant?.isActive ? "ACTIVE" : "INACTIVE"}
        backHref="/admin/tenants"
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
