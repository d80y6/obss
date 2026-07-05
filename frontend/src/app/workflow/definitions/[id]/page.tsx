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
import { useWorkflowDefinition } from "@/api/hooks/use-workflow-definitions"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { toast } from "@/components/ui/toast"
import { Plus, Trash2 } from "lucide-react"

export default function WorkflowDefinitionDetailPage() {
  const params = useParams()
  const id = params.id as string
  const queryClient = useQueryClient()
  const [newStepName, setNewStepName] = useState("")
  const [newStepType, setNewStepType] = useState("task")
  const [newStepConfig, setNewStepConfig] = useState("{}")

  const { data: def, isLoading } = useWorkflowDefinition(id)

  const { data: auditEntries } = useAuditLog("WorkflowDefinition", id)

  const addStep = useMutation({
    mutationFn: async () => {
      const res = await api.post(`/api/v1/workflow/definitions/${id}/steps`, {
        name: newStepName,
        type: newStepType,
        config: JSON.parse(newStepConfig || "{}"),
        order: (def?.steps?.length ?? 0) + 1,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.definitions.all })
      toast({ title: "Step added" })
      setNewStepName("")
      setNewStepType("task")
      setNewStepConfig("{}")
    },
    onError: () => toast({ title: "Failed to add step", variant: "destructive" }),
  })

  const removeStep = useMutation({
    mutationFn: async (stepId: string) => {
      const res = await api.delete(`/api/v1/workflow/definitions/${id}/steps/${stepId}`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.definitions.all })
      toast({ title: "Step removed" })
    },
    onError: () => toast({ title: "Failed to remove step", variant: "destructive" }),
  })

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Definition Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: def?.name ?? "-" },
            { label: "Description", value: def?.description ?? "-" },
            { label: "Version", value: def ? String(def.version) : "-" },
            { label: "Active", value: def ? <StatusBadge status={def.isActive ? "active" : "inactive"} /> : "-" },
            { label: "Steps", value: def ? String(def.steps?.length ?? 0) : "-" },
          ]}
        />
      ),
    },
    {
      id: "steps",
      label: "Steps",
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Workflow Steps</CardTitle></CardHeader>
          <CardContent>
            <div className="mb-4 p-4 border rounded-md space-y-3">
              <h4 className="text-sm font-semibold">Add Step</h4>
              <div className="flex items-end gap-2">
                <div className="flex-1">
                  <label className="text-xs font-medium">Name</label>
                  <Input value={newStepName} onChange={(e) => setNewStepName(e.target.value)} placeholder="Step name" />
                </div>
                <div className="w-32">
                  <label className="text-xs font-medium">Type</label>
                  <select className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm"
                    value={newStepType} onChange={(e) => setNewStepType(e.target.value)}>
                    <option value="task">Task</option>
                    <option value="approval">Approval</option>
                    <option value="notification">Notification</option>
                  </select>
                </div>
                <div className="flex-1">
                  <label className="text-xs font-medium">Config (JSON)</label>
                  <Input value={newStepConfig} onChange={(e) => setNewStepConfig(e.target.value)} placeholder='{"key": "value"}' />
                </div>
                <Button size="sm" onClick={() => addStep.mutate()} disabled={!newStepName || addStep.isPending}>
                  <Plus className="mr-1 h-4 w-4" /> Add
                </Button>
              </div>
            </div>
            {(!def?.steps || def.steps.length === 0) ? (
              <p className="text-sm text-muted-foreground">No steps defined.</p>
            ) : (
              def.steps.sort((a, b) => a.stepNumber - b.stepNumber).map((step) => (
                <div key={step.id} className="border-b py-3 flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{step.stepNumber}.</span>
                    <span className="font-medium">{step.name}</span>
                    <StatusBadge status={step.stepType} />
                  </div>
                  <Button variant="ghost" size="icon" onClick={() => removeStep.mutate(step.id)}>
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
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
      <EntityHeader title={def?.name ?? "Workflow Definition"} subtitle={`v${def?.version ?? ""}`} status={def?.isActive ? "active" : "inactive"} backHref="/workflow" loading={isLoading} />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
