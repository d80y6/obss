"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent } from "@/components/ui/card"
import { useReportDefinitions } from "@/api/hooks/use-reporting"
import type { ReportDefinitionDto } from "@/api/generated"
import { FileText } from "lucide-react"
import { useRouter } from "next/navigation"

export default function ReportDefinitionsPage() {
  const router = useRouter()
  const { data, isLoading } = useReportDefinitions()

  const columns: Column<ReportDefinitionDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "description", header: "Description", accessorKey: "description" },
    { id: "type", header: "Type", accessorKey: "type" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Report Definitions" backHref="/reporting" createHref="/reporting/definitions/new" createLabel="New Report" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No reports"
            emptyIcon={FileText}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/reporting/definitions/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
