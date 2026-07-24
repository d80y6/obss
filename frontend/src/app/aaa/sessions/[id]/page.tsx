"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable } from "@/components/shared/DataTable"
import { useSession } from "@/api/hooks/useSession"
import { useAaaLogs } from "@/api/hooks/useAaaLogs"
import type { AaaAuditLogDto } from "@/api/generated/dto"

function formatDuration(seconds: number): string {
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  return `${h}h ${m}m ${s}s`
}

function formatBytes(bytes: number): string {
  if (bytes === 0) return "0 B"
  const k = 1024
  const sizes = ["B", "KB", "MB", "GB", "TB"]
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`
}

export default function SessionDetailPage({ params }: { params: { id: string } }) {
  const { data: session, isLoading } = useSession(params.id)
  const { data: logs } = useAaaLogs({ username: session?.username ?? "", pageSize: "20" })

  if (isLoading || !session) return null

  const overviewFields = [
    { label: "Session ID", value: session.sessionId },
    { label: "Username", value: session.username },
    { label: "NAS ID", value: session.nasId },
    { label: "Framed IP", value: session.framedIpAddress ?? "—" },
    { label: "Called Station", value: session.calledStationId },
    { label: "Calling Station", value: session.callingStationId },
    { label: "Status", value: <StatusBadge status={session.sessionStatus} /> },
    { label: "Duration", value: formatDuration(session.acctSessionTime) },
    { label: "Data Received", value: formatBytes(session.inputOctets) },
    { label: "Data Sent", value: formatBytes(session.outputOctets) },
    { label: "Started", value: new Date(session.startedAt).toLocaleString() },
    { label: "Stopped", value: session.stoppedAt ? new Date(session.stoppedAt).toLocaleString() : "—" },
    { label: "Last Updated", value: new Date(session.updatedAt).toLocaleString() },
  ]

  const logColumns = [
    { id: "timestamp", header: "Timestamp", accessorKey: "timestamp" as const },
    { id: "eventType", header: "Event", accessorKey: "eventType" as const },
    { id: "nasIpAddress", header: "NAS IP", accessorKey: "nasIpAddress" as const },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: <EntityMetadata fields={overviewFields} columns={2} />,
    },
    {
      id: "logs",
      label: "Audit Log",
      content: (
        <DataTable<AaaAuditLogDto>
          columns={logColumns}
          data={logs?.items ?? []}
          rowKey={(row) => row.id}
          emptyTitle="No audit entries for this session"
        />
      ),
    },
  ]

  return (
    <div className="space-y-4">
      <EntityHeader title={`Session ${session.sessionId.slice(0, 8)}...`} status={session.sessionStatus} backHref="/aaa/sessions" />
      <EntityTabs tabs={tabs} />
    </div>
  )
}
