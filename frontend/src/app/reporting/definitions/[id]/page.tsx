"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useReportDefinition, useReportExecutions, useExecuteReport } from "@/api/hooks/use-reporting"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { ReportExecutionDto, AuditEntryDto } from "@/types/api"
import { toast } from "@/components/ui/toast"
import { useState } from "react"

export default function ReportDefinitionDetailPage() {
  const params = useParams()
  const id = params.id as string
  const [executing, setExecuting] = useState(false)

  const { data: definition, isLoading } = useReportDefinition(id)
  const { data: executions, refetch: refetchExecutions } = useReportExecutions(id)
  const executeMutation = useExecuteReport()

  const { data: auditEntries } = useQuery({
    queryKey: ["audit", "entity", "ReportDefinition", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/ReportDefinition/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const handleExecute = async () => {
    setExecuting(true)
    executeMutation.mutate(id, {
      onSuccess: () => {
        toast({ title: "Report execution started" })
        refetchExecutions()
        setExecuting(false)
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to execute report.", variant: "destructive" })
        setExecuting(false)
      },
    })
  }

  const executionColumns: Column<ReportExecutionDto>[] = [
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "outputFormat", header: "Format", accessorKey: "outputFormat" },
    { id: "startedAt", header: "Started", cell: (row) => row.startedAt ? new Date(row.startedAt).toLocaleString() : "-" },
    { id: "completedAt", header: "Completed", cell: (row) => row.completedAt ? new Date(row.completedAt).toLocaleString() : "-" },
    { id: "error", header: "Error", cell: (row) => row.error || "-" },
  ]

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          <EntityMetadata
            title="Report Details"
            loading={isLoading}
            fields={[
              { label: "Name", value: definition?.name ?? "-" },
              { label: "Description", value: definition?.description ?? "-" },
              { label: "Type", value: definition?.type ?? "-" },
              { label: "Last Executed", value: executions?.[0]?.startedAt ? new Date(executions[0].startedAt).toLocaleString() : "Never" },
            ]}
          />
          <div className="flex gap-2">
            <Button onClick={handleExecute} disabled={executing}>Execute Now</Button>
          </div>
          <Card>
            <CardHeader><CardTitle className="text-base">Execution History</CardTitle></CardHeader>
            <CardContent>
              <DataTable
                columns={executionColumns}
                data={executions ?? []}
                loading={isLoading}
                emptyTitle="No executions"
                rowKey={(row) => row.id}
              />
            </CardContent>
          </Card>
        </div>
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
      <EntityHeader
        title={definition?.name ?? "Report"}
        subtitle={definition?.type}
        backHref="/reporting/definitions"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
