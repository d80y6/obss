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
import { ServiceDto, TopologyDto, AuditEntryDto } from "@/types/api"

export default function ServiceDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: service, isLoading, error: serviceError } = useQuery({
    queryKey: ["services", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/services/${id}`)
      return res.data as ServiceDto
    },
    enabled: !!id,
  })

  const { data: topology, error: topologyError } = useQuery({
    queryKey: ["services", id, "topology"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/services/${id}/topology`)
      return res.data as TopologyDto[]
    },
    enabled: !!id,
  })

  const { data: auditEntries, error: auditError } = useQuery({
    queryKey: queryKeys.audit.entity("Service", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/Service/${id}`)
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
          title="Service Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: service?.serviceIdentifier ?? "-" },
            { label: "Type", value: service?.serviceType ?? "-" },
            { label: "Status", value: service ? <StatusBadge status={service.status} /> : "-" },
            { label: "Description", value: service?.configuration ?? "-" },
            { label: "Created", value: service?.createdAt ? new Date(service.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "topology",
      label: `Topology (${(topology ?? []).length})`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Service Topology</CardTitle></CardHeader>
          <CardContent>
            {(!topology || topology.length === 0) ? (
              <p className="text-sm text-muted-foreground">No topology data.</p>
            ) : (
              topology.map((t) => (
                <div key={t.id} className="border-b py-3">
                  <span className="font-medium">{t.type}</span>
                  <span className="text-sm text-muted-foreground ml-2">{t.parentServiceId ? `Parent: ${t.parentServiceId}` : "Root service"}</span>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Audit Trail</CardTitle></CardHeader>
          <CardContent>
            {(!auditEntries || auditEntries.length === 0) ? (
              <p className="text-sm text-muted-foreground">No audit entries.</p>
            ) : (
              auditEntries.map((e) => (
                <div key={e.id} className="border-b py-3">
                  <span className="font-medium">{e.action}</span>
                  <span className="text-sm text-muted-foreground ml-2">{new Date(e.performedAt).toLocaleString()}</span>
                  <p className="text-sm text-muted-foreground">By: {e.performedByName}</p>
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
      <EntityHeader title={service?.serviceIdentifier ?? "Service"} subtitle={service?.serviceType} status={service?.status} backHref="/services" loading={isLoading} />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
