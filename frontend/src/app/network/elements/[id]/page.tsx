"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useNetworkElement } from "@/api/hooks/useNetworkElement"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import type { NetworkConnectionDto } from "@/api/generated"

export default function NetworkElementDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: element, isLoading } = useNetworkElement(id)

  const { data: connections, error: connectionsError } = useQuery({
    queryKey: queryKeys.networks.elements.connections(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/elements/${id}/connections`)
      return res.data as NetworkConnectionDto[]
    },
    enabled: !!id,
  })

  const { data: auditEntries, error: auditError } = useAuditLog("NetworkElement", id)

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
          columns={2}
          fields={[
            { label: "Name", value: element?.name ?? "-" },
            { label: "Hostname", value: element?.hostname ?? "-" },
            { label: "IP Address", value: element?.ipAddress ?? "-" },
            { label: "Type", value: element?.elementType ?? "-" },
            { label: "Vendor", value: element?.vendor ?? "-" },
            { label: "Model", value: element?.model ?? "-" },
            { label: "Status", value: element ? <StatusBadge status={element.status} /> : "-" },
            { label: "Location", value: element?.location ?? "-" },
            { label: "Firmware", value: element?.softwareVersion ?? "-" },
            { label: "Serial Number", value: element?.serialNumber ?? "-" },
            { label: "Managed", value: element ? (element.isManaged ? "Yes" : "No") : "-" },
            { label: "Created", value: element?.createdAt ? new Date(element.createdAt).toLocaleDateString() : "-" },
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
        title={element?.name ?? "Network Element"}
        subtitle={element ? `${element.hostname || element.ipAddress}` : undefined}
        status={element?.status}
        backHref="/network/elements"
        editHref={`/network/elements/${id}/edit`}
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
