"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { NetworkElementDto, NetworkConnectionDto, AuditEntryDto } from "@/types/api"

export default function NetworkElementDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: element, isLoading, error: elementError } = useQuery({
    queryKey: queryKeys.networks.elements.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/elements/${id}`)
      return res.data as NetworkElementDto
    },
    enabled: !!id,
  })

  const { data: connections, error: connectionsError } = useQuery({
    queryKey: queryKeys.networks.elements.connections(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/elements/${id}/connections`)
      return res.data as NetworkConnectionDto[]
    },
    enabled: !!id,
  })

  const { data: auditEntries, error: auditError } = useQuery({
    queryKey: queryKeys.audit.entity("NetworkElement", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/NetworkElement/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const connColumns: Column<NetworkConnectionDto>[] = [
    { id: "connectedElementName", header: "Connected To", accessorKey: "connectedElementName" },
    { id: "type", header: "Type", accessorKey: "type" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "interface_", header: "Interface", accessorKey: "interface_" },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Element Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: element?.name ?? "-" },
            { label: "Type", value: element?.elementType ?? "-" },
            { label: "Model", value: element?.model ?? "-" },
            { label: "Vendor", value: element?.vendor ?? "-" },
            { label: "Status", value: element ? <StatusBadge status={element.status} /> : "-" },
            { label: "Location", value: element?.location ?? "-" },
            { label: "IP Address", value: element?.ipAddress ?? "-" },
            { label: "Firmware", value: element?.softwareVersion ?? "-" },
          ]}
        />
      ),
    },
    {
      id: "connections",
      label: `Connections (${(connections ?? []).length})`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Connections</CardTitle></CardHeader>
          <CardContent>
            <DataTable
              columns={connColumns}
              data={connections ?? []}
              loading={false}
              error={connectionsError ? "Failed to load data." : undefined}
              emptyTitle="No connections"
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
      <EntityHeader title={element?.name ?? "Network Element"} subtitle={`${element?.elementType ?? ""} - ${element?.vendor ?? ""}`} status={element?.status} backHref="/network/elements" loading={isLoading} />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
