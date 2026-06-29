"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useAuditAlerts, useAcknowledgeAlert } from "@/api/hooks/use-audit-alerts"
import { AuditAlertDto } from "@/types/api"
import { AlertTriangle } from "lucide-react"
import { toast } from "@/components/ui/toast"

export default function AuditAlertsPage() {
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const { data, isLoading } = useAuditAlerts()
  const acknowledgeMutation = useAcknowledgeAlert()

  const handleAcknowledge = (id: string) => {
    acknowledgeMutation.mutate(id, {
      onSuccess: () => toast({ title: "Alert acknowledged" }),
      onError: () => toast({ title: "Error", description: "Failed to acknowledge.", variant: "destructive" }),
    })
  }

  const columns: Column<AuditAlertDto>[] = [
    { id: "ruleName", header: "Rule", accessorKey: "ruleName", sortable: true },
    { id: "severity", header: "Severity", cell: (row) => <StatusBadge status={row.severity} /> },
    { id: "message", header: "Message", accessorKey: "message" },
    { id: "acknowledged", header: "Status", cell: (row) => <StatusBadge status={row.acknowledged ? "RESOLVED" : "OPEN"} /> },
    { id: "createdAt", header: "Created", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
    {
      id: "actions",
      header: "Actions",
      cell: (row) => !row.acknowledged ? (
        <Button variant="outline" size="sm" onClick={(e) => { e.stopPropagation(); handleAcknowledge(row.id) }}>
          Acknowledge
        </Button>
      ) : null,
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Audit Alerts" backHref="/audit" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No alerts"
            emptyIcon={AlertTriangle}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
          />
        </CardContent>
      </Card>
    </div>
  )
}
