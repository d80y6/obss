"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { AlertTriangle } from "lucide-react"
import { useRouter } from "next/navigation"

interface AuditAlertRuleDto {
  id: string
  name: string
  alertType: string
  severity: string
  threshold: number
  isActive: boolean
  createdAt: string
}

export default function AuditAlertRulesPage() {
  const router = useRouter()

  const { data, isLoading, error } = useQuery({
    queryKey: ["audit-alert-rules"],
    queryFn: async () => {
      const res = await api.get("/api/v1/audit/alert-rules")
      return res.data as AuditAlertRuleDto[]
    },
  })

  const columns: Column<AuditAlertRuleDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "alertType", header: "Event Type", accessorKey: "alertType" },
    { id: "severity", header: "Severity", cell: (row) => <StatusBadge status={row.severity} /> },
    { id: "threshold", header: "Threshold", accessorKey: "threshold" },
    { id: "isActive", header: "Active", cell: (row) => <StatusBadge status={row.isActive ? "ACTIVE" : "INACTIVE"} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Audit Alert Rules" backHref="/audit" createHref="/audit/alert-rules/new" createLabel="New Rule" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No alert rules"
            emptyIcon={AlertTriangle}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/audit/alert-rules/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
