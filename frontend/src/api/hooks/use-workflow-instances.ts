import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { WorkflowInstanceDto } from '@/api/generated/dto'

export function useWorkflowInstances(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.workflow.instances.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/workflow/instances?${params.toString()}`)
      return res.data as WorkflowInstanceDto[]
    },
  })
}

export function useWorkflowInstance(id: string) {
  return useQuery({
    queryKey: queryKeys.workflow.instances.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/workflow/instances/${id}`)
      return res.data as WorkflowInstanceDto
    },
    enabled: !!id,
  })
}

export function useStartWorkflowInstance() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: { definitionId: string; createdBy: string }) => {
      const res = await api.post("/api/v1/workflow/instances", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.instances.all })
    },
  })
}

export function useExecuteWorkflowTask() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ instanceId, taskId }: { instanceId: string; taskId: string }) => {
      const res = await api.post(`/api/v1/workflow/instances/${instanceId}/execute/${taskId}`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.instances.all })
    },
  })
}

export function useCompleteWorkflowInstance() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/workflow/instances/${id}/complete`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.instances.all })
    },
  })
}

export function useFailWorkflowInstance() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, reason }: { id: string; reason: string }) => {
      const res = await api.post(`/api/v1/workflow/instances/${id}/fail`, { reason })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.instances.all })
    },
  })
}
