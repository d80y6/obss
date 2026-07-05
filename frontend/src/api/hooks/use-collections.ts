import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CollectionActionDto, CollectionCaseDto, PaymentArrangementDto, DunningPolicyDto } from '@/api/generated/dto'
import type { RecordArrangementPaymentCommand } from '@/api/generated'

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

export function useDunningPolicies(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.collections.dunningPolicies.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/collections/dunning-policies?${params.toString()}`)
      return res.data as DunningPolicyDto[]
    },
  })
}

export function useDunningPolicy(id: string) {
  return useQuery({
    queryKey: queryKeys.collections.dunningPolicies.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/collections/dunning-policies/${id}`)
      return res.data as DunningPolicyDto
    },
    enabled: !!id,
  })
}

export function useCreateDunningPolicy() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/collections/dunning-policies", data)
      return res.data as DunningPolicyDto
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.dunningPolicies.all })
    },
  })
}

export function useUpdateDunningPolicy() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: Record<string, unknown>) => {
      const res = await api.put(`/api/v1/collections/dunning-policies/${id}`, data)
      return res.data as DunningPolicyDto
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.dunningPolicies.detail(variables.id as string) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.dunningPolicies.all })
    },
  })
}

export function useDeleteDunningPolicy() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/collections/dunning-policies/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.dunningPolicies.all })
    },
  })
}

export function useRecordArrangementPayment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RecordArrangementPaymentCommand) => {
      const res = await api.post(`/api/v1/collections/arrangements/${data.paymentArrangementId}/payments`, data)
      return res.data as CollectionCaseDto
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.all })
    },
  })
}

export function useCollectionDashboard() {
  return useQuery({
    queryKey: queryKeys.collections.dashboard(),
    queryFn: async () => {
      const [casesRes, agingRes] = await Promise.all([
        api.get("/api/v1/collections/cases"),
        api.get("/api/v1/collections/reports/aging"),
      ])
      const cases = casesRes.data as CollectionCaseDto[]
      const aging = agingRes.data as { totalCustomers: number; totalCases: number; grandTotalOverdue: number }
      return {
        openCases: cases.filter((c) => c.status === "Open" || c.status === "InProgress").length,
        activeArrangements: cases.reduce((sum, c) =>
          sum + c.paymentArrangements.filter((pa) => pa.status === "Active").length, 0),
        dunningNoticesSent: cases.reduce((sum, c) =>
          sum + c.actions.filter((a) => a.actionType === "DunningNotice").length, 0),
        totalOverdue: aging?.grandTotalOverdue ?? 0,
        totalCustomers: aging?.totalCustomers ?? 0,
        totalCases: aging?.totalCases ?? 0,
      }
    },
  })
}
