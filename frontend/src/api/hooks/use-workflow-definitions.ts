import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { WorkflowDefinitionDto } from '@/api/generated/dto'
import type { CreateWorkflowDefinitionCommand } from '@/api/generated'

export function useWorkflowDefinitions(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.workflow.definitions.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/workflow/definitions?${params.toString()}`)
      return res.data as WorkflowDefinitionDto[]
    },
  })
}

export function useWorkflowDefinition(id: string) {
  return useQuery({
    queryKey: queryKeys.workflow.definitions.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/workflow/definitions/${id}`)
      return res.data as WorkflowDefinitionDto
    },
    enabled: !!id,
  })
}

export function useCreateWorkflowDefinition() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateWorkflowDefinitionCommand) => {
      const res = await api.post("/api/v1/workflow/definitions", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.definitions.all })
    },
  })
}

export function useUpdateWorkflowDefinition() {
  return useMutation({
    mutationFn: async () => {
      throw new Error("Update workflow definition is not available")
    },
  })
}

export function useAddWorkflowStep() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ definitionId, data }: { definitionId: string; data: { name: string; type: string; config: Record<string, unknown> } }) => {
      const res = await api.post(`/api/v1/workflow/definitions/${definitionId}/steps`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.definitions.all })
    },
  })
}

export function useRemoveWorkflowStep() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ definitionId, stepId }: { definitionId: string; stepId: string }) => {
      const res = await api.delete(`/api/v1/workflow/definitions/${definitionId}/steps/${stepId}`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.workflow.definitions.all })
    },
  })
}
