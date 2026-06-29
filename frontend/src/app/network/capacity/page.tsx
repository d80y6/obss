"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { MetricCard } from "@/components/shared/MetricCard"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { CapacityOverviewDto, CapacityAlertDto } from "@/types/api"
import { AlertTriangle, HardDrive, Activity, Wifi } from "lucide-react"

export default function CapacityPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: queryKeys.networks.capacity.overview(),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/capacity/overview")
      return res.data as CapacityOverviewDto
    },
  })

  const usagePercent = data?.usedBandwidth && data?.totalBandwidth
    ? Math.round((data.usedBandwidth / data.totalBandwidth) * 100)
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
        <MetricCard title="Total Elements" value={data?.totalElements ?? "-"} icon={HardDrive} loading={isLoading} />
        <MetricCard title="Bandwidth Usage" value={`${usagePercent}%`} icon={Activity} loading={isLoading} />
        <MetricCard title="Alerts" value={data?.alerts?.length ?? 0} icon={AlertTriangle} loading={isLoading} />
      </div>
      <Card>
        <CardHeader><CardTitle className="text-base">Capacity Alerts</CardTitle></CardHeader>
        <CardContent>
          <DataTable
            columns={alertColumns}
            data={data?.alerts ?? []}
            loading={isLoading}
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
