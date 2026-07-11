"use client"

import { useParams } from "next/navigation"
import { useState } from "react"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import type { WorkflowInstanceDto } from "@/api/generated"
import { toast } from "@/components/ui/toast"
import { Play, CheckCircle, XCircle } from "lucide-react"

export default function WorkflowInstanceDetailPage() {
  const params = useParams()
  const id = params.id as string
  const queryClient = useQueryClient()
  const [failReason, setFailReason] = useState("")

  const { data: instance, isLoading } = useQuery({
    queryKey: queryKeys.workflow.instances.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/workflow/instances/${id}`)
      return res.data as WorkflowInstanceDto
    },
    enabled: !!id,
  })

  const { data: auditEntries } = useAuditLog("WorkflowInstance", id)

  const executeTask = useMutation({
    mutationFn: async (taskId: string) => {
      const res = await api.post(`/api/v1/workflow/instances/${id}/execute/${taskId}`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.instances.all })
      toast({ title: "Task executed" })
    },
    onError: () => toast({ title: "Failed to execute task", variant: "destructive" }),
  })

  const completeInstance = useMutation({
    mutationFn: async () => {
      const res = await api.post(`/api/v1/workflow/instances/${id}/complete`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.instances.all })
      toast({ title: "Instance completed" })
    },
    onError: () => toast({ title: "Failed to complete instance", variant: "destructive" }),
  })

  const failInstance = useMutation({
    mutationFn: async () => {
      const res = await api.post(`/api/v1/workflow/instances/${id}/fail`, { reason: failReason })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.instances.all })
      toast({ title: "Instance marked as failed" })
      setFailReason("")
    },
    onError: () => toast({ title: "Failed to mark instance as failed", variant: "destructive" }),
  })

  const pendingTasks = instance?.tasks?.filter((t) => t.status === "pending") ?? []
  const canComplete = instance?.status === "running"
  const canFail = instance?.status === "running" || instance?.status === "pending"

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-4">
          <div className="flex items-center gap-2 flex-wrap">
            {canComplete && (
              <Button size="sm" onClick={() => completeInstance.mutate()} disabled={completeInstance.isPending}>
                <CheckCircle className="mr-1 h-4 w-4" /> Complete
              </Button>
            )}
            {canFail && (
              <div className="flex items-center gap-2">
                <Input value={failReason} onChange={(e) => setFailReason(e.target.value)} placeholder="Failure reason" className="w-48 h-9" />
                <Button size="sm" variant="destructive" onClick={() => failInstance.mutate()} disabled={!failReason || failInstance.isPending}>
                  <XCircle className="mr-1 h-4 w-4" /> Fail
                </Button>
              </div>
            )}
          </div>
          <EntityMetadata
            title="Instance Details"
            loading={isLoading}
            fields={[
              { label: "Definition", value: instance?.workflowDefinitionName ?? "-" },
              { label: "Status", value: instance ? <StatusBadge status={instance.status} /> : "-" },
              { label: "Started", value: instance?.startedAt ? new Date(instance.startedAt).toLocaleString() : "-" },
              { label: "Completed", value: instance?.completedAt ? new Date(instance.completedAt).toLocaleString() : "-" },
              { label: "Created By", value: instance?.createdBy ?? "-" },
              { label: "Tasks", value: instance ? String(instance.tasks?.length ?? 0) : "-" },
            ]}
          />
        </div>
      ),
    },
    {
      id: "tasks",
      label: `Tasks (${pendingTasks.length} pending)`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Tasks</CardTitle></CardHeader>
          <CardContent>
            {(!instance?.tasks || instance.tasks.length === 0) ? (
              <p className="text-sm text-muted-foreground">No tasks.</p>
            ) : (
              instance.tasks.map((task) => (
                <div key={task.id} className="border-b py-3 flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{task.stepName}</span>
                    <StatusBadge status={task.status} />
                  </div>
                  {task.status === "pending" && (
                    <Button size="sm" variant="outline" onClick={() => executeTask.mutate(task.id)} disabled={executeTask.isPending}>
                      <Play className="mr-1 h-4 w-4" /> Execute
                    </Button>
                  )}
                </div>
              ))
            )}
          </CardContent>
        </Card>
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
      <EntityHeader title={`Instance: ${instance?.workflowDefinitionName ?? ""}`} subtitle={instance?.createdBy} status={instance?.status} backHref="/workflow/instances" loading={isLoading} />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
