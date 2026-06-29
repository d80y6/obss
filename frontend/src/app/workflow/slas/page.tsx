"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { WorkflowSlaDto } from "@/types/api"
import { ListTodo } from "lucide-react"
import { useRouter } from "next/navigation"

export default function WorkflowSlasPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading, error } = useQuery({
    queryKey: ["workflow-slas"],
    queryFn: async () => {
      const res = await api.get("/api/v1/workflow/slas")
      return res.data as WorkflowSlaDto[]
    },
  })

  const columns: Column<WorkflowSlaDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "responseTime", header: "Response Time (min)", accessorKey: "responseTime" },
    { id: "resolutionTime", header: "Resolution Time (min)", accessorKey: "resolutionTime" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="SLA Definitions" backHref="/workflow" createHref="/workflow/slas/new" createLabel="New SLA" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No SLA definitions"
            emptyIcon={ListTodo}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
          />
        </CardContent>
      </Card>
    </div>
  )
}
