"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { WorkflowInstanceDto } from "@/types/api"
import { PlayCircle } from "lucide-react"
import { useRouter } from "next/navigation"

export default function WorkflowInstancesPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading, error } = useQuery({
    queryKey: ["workflow-instances-list"],
    queryFn: async () => {
      const res = await api.get("/api/v1/workflow/instances")
      return res.data as WorkflowInstanceDto[]
    },
  })

  const columns: Column<WorkflowInstanceDto>[] = [
    { id: "definition", header: "Definition", accessorKey: "workflowDefinitionName", sortable: true },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "startedAt", header: "Started", cell: (row) => row.startedAt ? new Date(row.startedAt).toLocaleDateString() : "-" },
    { id: "completedAt", header: "Completed", cell: (row) => row.completedAt ? new Date(row.completedAt).toLocaleDateString() : "-" },
    { id: "createdBy", header: "Created By", accessorKey: "createdBy" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Workflow Instances" backHref="/workflow" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No instances"
            emptyIcon={PlayCircle}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/workflow/instances/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
