"use client"

import { useParams, useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { AuditEntryDto, ServiceResourceDto, ServiceTopologyDto, TopologyLinkDto } from "@/types/api"
import { useService, useServiceTopology, useServiceResources } from "@/api/hooks/use-service-inventory"
import { Pencil } from "lucide-react"
import Link from "next/link"

export default function ServiceInventoryDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: service, isLoading } = useService(id)
  const { data: topology } = useServiceTopology(id)
  const { data: resources } = useServiceResources(id)

  const { data: auditEntries, error: auditError } = useQuery({
    queryKey: queryKeys.audit.entity("Service", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/Service/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const resColumns: Column<ServiceResourceDto>[] = [
    { id: "resourceType", header: "Type", accessorKey: "resourceType" },
    { id: "resourceIdentifier", header: "Identifier", accessorKey: "resourceIdentifier" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-4">
          <div className="flex justify-end">
            <Button variant="outline" size="sm" asChild>
              <Link href={`/service-inventory/${id}/edit`}><Pencil className="mr-1 h-4 w-4" /> Edit</Link>
            </Button>
          </div>
          <EntityMetadata
            title="Service Details"
            loading={isLoading}
            fields={[
              { label: "Service ID", value: service?.serviceIdentifier ?? "-" },
              { label: "Type", value: service?.serviceType ?? "-" },
              { label: "Status", value: service ? <StatusBadge status={service.status} /> : "-" },
              { label: "Customer ID", value: service?.customerId ?? "-" },
              { label: "Subscription ID", value: service?.subscriptionId ?? "-" },
              { label: "Configuration", value: service?.configuration ?? "-" },
              { label: "Location", value: service?.location ?? "-" },
              { label: "Created", value: service?.createdAt ? new Date(service.createdAt).toLocaleDateString() : "-" },
              { label: "Updated", value: service?.updatedAt ? new Date(service.updatedAt).toLocaleDateString() : "-" },
            ]}
          />
        </div>
      ),
    },
    {
      id: "topology",
      label: `Topology (${(topology?.links ?? []).length})`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Service Topology</CardTitle></CardHeader>
          <CardContent>
            {!topology || !topology.links || topology.links.length === 0 ? (
              <p className="text-sm text-muted-foreground">No topology data.</p>
            ) : (
              topology.links.map((link) => (
                <div key={link.id} className="border-b py-3">
                  <span className="font-medium">{link.sourceServiceId}</span>
                  <span className="text-sm text-muted-foreground mx-1">→</span>
                  <span className="font-medium">{link.targetServiceId}</span>
                  <span className="text-sm text-muted-foreground ml-2">{link.direction}</span>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
    {
      id: "resources",
      label: `Resources (${(resources ?? []).length})`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Resources</CardTitle></CardHeader>
          <CardContent>
            <DataTable
              columns={resColumns}
              data={resources ?? []}
              loading={false}
              error={auditError ? "Failed to load data." : undefined}
              emptyTitle="No resources"
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
      <EntityHeader title={service?.serviceIdentifier ?? "Service"} subtitle={service?.serviceType} status={service?.status} backHref="/service-inventory" loading={isLoading} />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
