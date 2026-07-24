"use client"

import { useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable } from "@/components/shared/DataTable"
import { useNasDevice } from "@/api/hooks/useNasDevice"
import { useDeleteNas } from "@/api/hooks/useDeleteNas"
import { useSessions } from "@/api/hooks/useSessions"
import { useAaaLogs } from "@/api/hooks/useAaaLogs"
import { toast } from "@/components/ui/toast"
import type { RadiusSessionDto, AaaAuditLogDto } from "@/api/generated/dto"

export default function NasDetailPage({ params }: { params: { id: string } }) {
  const router = useRouter()
  const { data: nas, isLoading } = useNasDevice(params.id)
  const deleteNas = useDeleteNas()
  const { data: sessions } = useSessions({ nasId: params.id, pageSize: "10" })
  const { data: logs } = useAaaLogs({ nasId: params.id, pageSize: "10" })

  const handleDelete = async () => {
    if (!confirm(`Delete NAS "${nas?.name}"?`)) return
    await deleteNas.mutateAsync(params.id)
    toast({ title: "Deleted", description: "NAS device removed." })
    router.push("/aaa/nas")
  }

  if (isLoading || !nas) return null

  const overviewFields = [
    { label: "Name", value: nas.name },
    { label: "IP Address", value: nas.nasIpAddress },
    { label: "Type", value: nas.nasType },
    { label: "Status", value: <StatusBadge status={nas.status} /> },
    { label: "Location", value: nas.location ?? "—" },
    { label: "Created", value: new Date(nas.createdAt).toLocaleString() },
    { label: "Updated", value: new Date(nas.updatedAt).toLocaleString() },
  ]

  const sessionColumns = [
    { id: "username", header: "Username", accessorKey: "username" as const },
    { id: "framedIpAddress", header: "Framed IP", accessorKey: "framedIpAddress" as const },
    { id: "sessionStatus", header: "Status", cell: (row: RadiusSessionDto) => <StatusBadge status={row.sessionStatus} /> },
    { id: "startedAt", header: "Started", accessorKey: "startedAt" as const },
  ]

  const logColumns = [
    { id: "timestamp", header: "Timestamp", accessorKey: "timestamp" as const },
    { id: "eventType", header: "Event", accessorKey: "eventType" as const },
    { id: "username", header: "Username", accessorKey: "username" as const },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: <EntityMetadata fields={overviewFields} columns={2} />,
    },
    {
      id: "sessions",
      label: "Sessions",
      content: (
        <DataTable<RadiusSessionDto>
          columns={sessionColumns}
          data={sessions?.items ?? []}
          rowKey={(row) => row.id}
          emptyTitle="No sessions for this NAS"
        />
      ),
    },
    {
      id: "logs",
      label: "Audit Log",
      content: (
        <DataTable<AaaAuditLogDto>
          columns={logColumns}
          data={logs?.items ?? []}
          rowKey={(row) => row.id}
          emptyTitle="No audit entries for this NAS"
        />
      ),
    },
  ]

  return (
    <div className="space-y-4">
      <EntityHeader
        title={nas.name}
        status={nas.status}
        backHref="/aaa/nas"
        editHref={`/aaa/nas/${params.id}/edit`}
        onDelete={handleDelete}
      />
      <EntityTabs tabs={tabs} />
    </div>
  )
}
