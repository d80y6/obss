"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { MetricCard } from "@/components/shared/MetricCard"
import { DataTable } from "@/components/shared/DataTable"
import { useAaaMetrics } from "@/api/hooks/useAaaMetrics"
import { useAaaLogs } from "@/api/hooks/useAaaLogs"
import { Shield, Server, Activity, Wifi, Database, ArrowUpDown } from "lucide-react"
import type { AaaAuditLogDto } from "@/api/generated/dto"

function formatBytes(bytes: number): string {
  if (bytes === 0) return "0 B"
  const k = 1024
  const sizes = ["B", "KB", "MB", "GB", "TB"]
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`
}

export default function AaaDashboardPage() {
  const { data: metrics, isLoading: metricsLoading } = useAaaMetrics()
  const { data: recentLogs } = useAaaLogs({ page: "1", pageSize: "10" })

  const logColumns = [
    { id: "timestamp", header: "Timestamp", accessorKey: "timestamp" as const },
    { id: "eventType", header: "Event", accessorKey: "eventType" as const },
    { id: "username", header: "Username", accessorKey: "username" as const },
    { id: "nasIpAddress", header: "NAS IP", accessorKey: "nasIpAddress" as const },
  ]

  return (
    <div className="space-y-6">
      <PageHeader title="AAA Dashboard" />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <MetricCard title="Total NAS Devices" value={metrics?.totalNas ?? 0} icon={Server} loading={metricsLoading} />
        <MetricCard title="Active NAS" value={metrics?.activeNas ?? 0} icon={Activity} loading={metricsLoading} />
        <MetricCard title="Active Sessions" value={metrics?.activeSessions ?? 0} icon={Wifi} loading={metricsLoading} />
        <MetricCard title="Sessions Today" value={metrics?.sessionsToday ?? 0} icon={Database} loading={metricsLoading} />
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <MetricCard title="Data Transferred (In)" value={formatBytes(metrics?.totalInputOctets ?? 0)} icon={ArrowUpDown} loading={metricsLoading} />
        <MetricCard title="Data Transferred (Out)" value={formatBytes(metrics?.totalOutputOctets ?? 0)} icon={ArrowUpDown} loading={metricsLoading} />
      </div>

      <div>
        <h2 className="text-lg font-semibold mb-3">Recent Activity</h2>
        <DataTable<AaaAuditLogDto>
          columns={logColumns}
          data={recentLogs?.items ?? []}
          rowKey={(row) => row.id}
          emptyTitle="No recent activity"
        />
      </div>
    </div>
  )
}
