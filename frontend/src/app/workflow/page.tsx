"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import type { WorkflowDefinitionDto, WorkflowInstanceDto } from "@/api/generated"
import { Waypoints, PlayCircle, Plus, ListTodo } from "lucide-react"
import { useRouter } from "next/navigation"
import Link from "next/link"
import { useWorkflowDefinitions } from "@/api/hooks/use-workflow-definitions"
import { useWorkflowInstances, useStartWorkflowInstance } from "@/api/hooks/use-workflow-instances"
import { toast } from "@/components/ui/toast"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"

export default function WorkflowPage() {
  const router = useRouter()
  const queryClient = useQueryClient()
  const [showStartDialog, setShowStartDialog] = useState(false)
  const [selectedDefId, setSelectedDefId] = useState("")

  const { data: definitions } = useWorkflowDefinitions()
  const { data: instances } = useWorkflowInstances()
  const startInstance = useStartWorkflowInstance()

  const defColumns: Column<WorkflowDefinitionDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "version", header: "Version", accessorKey: "version" },
    { id: "active", header: "Active", cell: (row) => <StatusBadge status={row.isActive ? "active" : "inactive"} /> },
    { id: "steps", header: "Steps", cell: (row) => String(row.steps?.length ?? 0) },
  ]

  const instColumns: Column<WorkflowInstanceDto>[] = [
    { id: "definition", header: "Definition", accessorKey: "workflowDefinitionName" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "startedAt", header: "Started", cell: (row) => row.startedAt ? new Date(row.startedAt).toLocaleDateString() : "-" },
    { id: "createdBy", header: "Created By", accessorKey: "createdBy" },
  ]

  const handleStartInstance = () => {
    if (!selectedDefId) return
    startInstance.mutate({ definitionId: selectedDefId, createdBy: "admin" }, {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: queryKeys.workflow.instances.all })
        toast({ title: "Instance started" })
        setShowStartDialog(false)
        setSelectedDefId("")
      },
      onError: () => toast({ title: "Failed to start instance", variant: "destructive" }),
    })
  }

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Workflow"
        actions={
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" asChild>
              <Link href="/workflow/slas"><ListTodo className="mr-1 h-4 w-4" /> SLAs</Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/workflow/instances"><PlayCircle className="mr-1 h-4 w-4" /> Instances</Link>
            </Button>
          </div>
        }
      />
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-base">Definitions</CardTitle>
          <div className="flex items-center gap-2">
            <Dialog open={showStartDialog} onOpenChange={setShowStartDialog}>
              <DialogTrigger asChild>
                <Button variant="outline" size="sm"><PlayCircle className="mr-1 h-4 w-4" /> Start Instance</Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader><DialogTitle>Start Workflow Instance</DialogTitle></DialogHeader>
                <div className="space-y-4 py-4">
                  <div className="space-y-2">
                    <label className="text-sm font-medium">Definition</label>
                    <Select value={selectedDefId} onValueChange={setSelectedDefId}>
                      <SelectTrigger><SelectValue placeholder="Select definition" /></SelectTrigger>
                      <SelectContent>
                        {(definitions ?? []).map((d) => (
                          <SelectItem key={d.id} value={d.id}>{d.name} v{d.version}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <Button onClick={handleStartInstance} disabled={!selectedDefId || startInstance.isPending} className="w-full">
                    {startInstance.isPending ? "Starting..." : "Start"}
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
            <Button size="sm" asChild>
              <Link href="/workflow/definitions/new"><Plus className="mr-1 h-4 w-4" /> New Definition</Link>
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={defColumns}
            data={definitions ?? []}
            loading={false}
            emptyTitle="No workflow definitions"
            emptyIcon={Waypoints}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/workflow/definitions/${row.id}`)}
          />
          <CardTitle className="text-base mt-8 mb-4">Recent Instances</CardTitle>
          <DataTable
            columns={instColumns}
            data={(instances ?? []).slice(0, 10)}
            loading={false}
            emptyTitle="No workflow instances"
            emptyIcon={Waypoints}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/workflow/instances/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
