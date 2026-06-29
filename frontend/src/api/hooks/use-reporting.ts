import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ReportDefinitionDto, ReportExecutionDto } from '@/api/generated/dto'
import type { CreateReportDefinitionCommand } from '@/api/generated'

export function useReportDefinitions() {
  return useQuery({
    queryKey: queryKeys.reporting.definitions.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/reporting/definitions")
      return res.data as ReportDefinitionDto[]
    },
  })
}

export function useReportDefinition(id: string) {
  return useQuery({
    queryKey: ["reporting", "definitions", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/reporting/definitions/${id}`)
      return res.data as ReportDefinitionDto
    },
    enabled: !!id,
  })
}

export function useCreateReportDefinition() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateReportDefinitionCommand) => {
      const res = await api.post("/api/v1/reporting/definitions", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.reporting.definitions.list() })
    },
  })
}

export function useUpdateReportDefinition() {
  return useMutation({
    mutationFn: async () => {
      throw new Error("Update report definition is not available")
    },
  })
}

export function useReportExecutions(definitionId: string) {
  return useQuery({
    queryKey: ["reporting", "definitions", definitionId, "executions"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/reporting/definitions/${definitionId}/executions`)
      return res.data as ReportExecutionDto[]
    },
    enabled: !!definitionId,
  })
}

export function useExecuteReport() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (definitionId: string) => {
      const res = await api.post(`/api/v1/reporting/definitions/${definitionId}/execute`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reporting", "definitions"] })
    },
  })
}

export function useDashboardWidgets() {
  return useQuery({
    queryKey: queryKeys.reporting.dashboard(),
    queryFn: async () => {
      const res = await api.get("/api/v1/reporting/dashboard")
      return res.data
    },
  })
}

export function useCreateWidget() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/reporting/widgets", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.reporting.dashboard() })
    },
  })
}

export function useScheduledReports() {
  return useQuery({
    queryKey: queryKeys.reporting.schedules.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/reporting/schedule")
      return res.data
    },
  })
}

export function useCreateScheduledReport() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/reporting/schedule", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.reporting.schedules.list() })
    },
  })
}
