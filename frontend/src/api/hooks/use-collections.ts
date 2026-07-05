import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CollectionActionDto, CollectionCaseDto, PaymentArrangementDto } from '@/api/generated/dto'

export function useCollectionCases(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.collections.cases.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/collections/cases?${params.toString()}`)
      return res.data as CollectionCaseDto[]
    },
  })
}

export function useCollectionCase(id: string) {
  return useQuery({
    queryKey: queryKeys.collections.cases.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/collections/cases/${id}`)
      return res.data as CollectionCaseDto
    },
    enabled: !!id,
  })
}

export function useCaseActions(id: string) {
  return useQuery({
    queryKey: queryKeys.collections.cases.actions(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/collections/cases/${id}/actions`)
      return res.data as CollectionActionDto[]
    },
    enabled: !!id,
  })
}

export function useCaseArrangements(id: string) {
  return useQuery({
    queryKey: queryKeys.collections.cases.arrangements(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/collections/cases/${id}/arrangements`)
      return res.data as PaymentArrangementDto[]
    },
    enabled: !!id,
  })
}

export function useCreateCollectionCase() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/collections/cases", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.all })
    },
  })
}

export function useCreateCaseAction(caseId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post(`/api/v1/collections/cases/${caseId}/actions`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.actions(caseId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(caseId) })
    },
  })
}

export function useCreateCaseArrangement(caseId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post(`/api/v1/collections/cases/${caseId}/arrangements`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.arrangements(caseId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(caseId) })
    },
  })
}

export function useResolveCase(caseId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/collections/cases/${caseId}/resolve`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(caseId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.all })
    },
  })
}

export function useSendDunning(caseId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/collections/cases/${caseId}/dunning`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(caseId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.actions(caseId) })
    },
  })
}

export function useAgingReport() {
  return useQuery({
    queryKey: queryKeys.collections.reports.aging(),
    queryFn: async () => {
      const res = await api.get("/api/v1/collections/reports/aging")
      return res.data
    },
  })
}
