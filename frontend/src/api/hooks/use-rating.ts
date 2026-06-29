import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RatingRuleDto, PromotionDto, UsageRecordDto } from '@/api/generated/dto'
import type { CreateRatingRuleCommand, CreatePromotionCommand, SubmitUsageCommand, ApplyPromotionCommand } from '@/api/generated'

export function useRatingRules(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.rating.rules.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/rating/rules?${params.toString()}`)
      return res.data as RatingRuleDto[]
    },
  })
}

export function useRatingRule(id: string) {
  return useQuery({
    queryKey: queryKeys.rating.rules.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/rating/rules/${id}`)
      return res.data as RatingRuleDto
    },
    enabled: !!id,
  })
}

export function useCreateRatingRule() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateRatingRuleCommand) => {
      const res = await api.post<RatingRuleDto>("/api/v1/rating/rules", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.rating.rules.lists() })
    },
  })
}

export function usePromotions(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.rating.promotions.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/rating/promotions?${params.toString()}`)
      return res.data as PromotionDto[]
    },
  })
}

export function usePromotion(id: string) {
  return useQuery({
    queryKey: queryKeys.rating.promotions.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/rating/promotions/${id}`)
      return res.data as PromotionDto
    },
    enabled: !!id,
  })
}

export function useCreatePromotion() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreatePromotionCommand) => {
      const res = await api.post<PromotionDto>("/api/v1/rating/promotions", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.rating.promotions.lists() })
    },
  })
}

export function useDeactivatePromotion() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.post(`/api/v1/rating/promotions/${id}/deactivate`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.rating.promotions.all })
    },
  })
}

export function useApplicablePromotions(params: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.rating.promotions.applicable(params),
    queryFn: async () => {
      const qs = new URLSearchParams()
      Object.entries(params).forEach(([k, v]) => { if (v) qs.set(k, v) })
      const res = await api.get(`/api/v1/rating/promotions/applicable?${qs.toString()}`)
      return res.data as PromotionDto[]
    },
    enabled: Object.keys(params).length > 0,
  })
}

export function useApplyPromotion() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: ApplyPromotionCommand) => {
      const res = await api.post("/api/v1/rating/promotions/apply", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.rating.promotions.all })
    },
  })
}

export function useUsageRecords(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.rating.usage.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/rating/usage?${params.toString()}`)
      return res.data as UsageRecordDto[]
    },
  })
}

export function useUsageRecord(id: string) {
  return useQuery({
    queryKey: queryKeys.rating.usage.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/rating/usage/${id}`)
      return res.data as UsageRecordDto
    },
    enabled: !!id,
  })
}

export function useUnratedRecords() {
  return useQuery({
    queryKey: queryKeys.rating.usage.unrated,
    queryFn: async () => {
      const res = await api.get("/api/v1/rating/usage/unrated")
      return res.data as UsageRecordDto[]
    },
  })
}

export function useUsageBySubscription(subscriptionId: string, filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.rating.usage.bySubscription(subscriptionId, filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/rating/usage/subscription/${subscriptionId}?${params.toString()}`)
      return res.data as UsageRecordDto[]
    },
    enabled: !!subscriptionId,
  })
}

export function useSubmitUsage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: SubmitUsageCommand) => {
      const res = await api.post<UsageRecordDto>("/api/v1/rating/usage", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.rating.usage.lists() })
    },
  })
}

export function useRateUsage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const res = await api.post("/api/v1/rating/usage/rate")
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.rating.usage.all })
    },
  })
}

export function useRateUsageRealtime() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/rating/usage/${id}/rate-realtime`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.rating.usage.all })
    },
  })
}
