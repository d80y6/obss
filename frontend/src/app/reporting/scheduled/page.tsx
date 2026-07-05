"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useScheduledReports } from "@/api/hooks/use-reporting"
import type { ScheduledReportDto } from "@/api/generated"
import { Clock } from "lucide-react"
import { useRouter } from "next/navigation"

export default function ScheduledReportsPage() {
  const router = useRouter()
  const { data, isLoading } = useScheduledReports()

  const columns: Column<ScheduledReportDto>[] = [
    { id: "reportDefinitionId", header: "Report Definition", accessorKey: "reportDefinitionId" },
    { id: "cronExpression", header: "Schedule", accessorKey: "cronExpression" },
    { id: "recipients", header: "Recipients", cell: (row) => (row.recipients ?? []).join(", ") },
    { id: "isActive", header: "Enabled", cell: (row) => <StatusBadge status={row.isActive ? "ACTIVE" : "INACTIVE"} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Scheduled Reports" backHref="/reporting" createHref="/reporting/scheduled/new" createLabel="New Schedule" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No scheduled reports"
            emptyIcon={Clock}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/reporting/scheduled/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
