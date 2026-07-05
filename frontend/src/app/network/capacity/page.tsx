"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { MetricCard } from "@/components/shared/MetricCard"
import { useOverallNetworkCapacity } from "@/api/hooks/useOverallNetworkCapacity"
import { useCapacityAlerts } from "@/api/hooks/useCapacityAlerts"
import type { CapacityAlertDto } from "@/api/generated/dto"
import { AlertTriangle, HardDrive, Activity, Wifi } from "lucide-react"

export default function CapacityPage() {
  const { data: overview, isLoading, error } = useOverallNetworkCapacity()
  const { data: alerts, isLoading: alertsLoading } = useCapacityAlerts()

  const usagePercent = overview?.usedBandwidth && overview?.totalBandwidth
    ? Math.round((overview.usedBandwidth / overview.totalBandwidth) * 100)
    : 0

  const alertColumns: Column<CapacityAlertDto>[] = [
    { id: "elementName", header: "Element", accessorKey: "elementName" },
    { id: "metric", header: "Metric", accessorKey: "metric" },
    { id: "currentValue", header: "Current", accessorKey: "currentValue" },
    { id: "threshold", header: "Threshold", accessorKey: "threshold" },
    { id: "severity", header: "Severity", cell: (row) => <StatusBadge status={row.severity} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Network Capacity" backHref="/network" />
      <div className="grid gap-4 md:grid-cols-3">
        <MetricCard title="Total Elements" value={overview?.totalElements ?? "-"} icon={HardDrive} loading={isLoading} />
        <MetricCard title="Bandwidth Usage" value={`${usagePercent}%`} icon={Activity} loading={isLoading} />
        <MetricCard title="Alerts" value={overview?.alerts?.length ?? 0} icon={AlertTriangle} loading={isLoading} />
      </div>
      <Card>
        <CardHeader><CardTitle className="text-base">Capacity Alerts</CardTitle></CardHeader>
        <CardContent>
          <DataTable
            columns={alertColumns}
            data={alerts ?? []}
            loading={isLoading || alertsLoading}
            emptyTitle="No capacity alerts"
            emptyIcon={Wifi}
            rowKey={(row) => row.id}
            error={error ? "Failed to load data." : undefined}
          />
        </CardContent>
      </Card>
    </div>
  )
}
