import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export interface OcsBalanceDto {
  id: string
  subscriptionId: string
  accountId: string
  balanceType: string
  amount: number
  reservedAmount: number
  currency: string
  validFrom: string
  validTo: string | null
  status: string
  lastTransactionAt: string | null
}

export interface OcsCreditPoolDto {
  id: string
  name: string
  poolType: string
  totalAmount: number
  consumedAmount: number
  remainingAmount: number
  currency: string
  validFrom: string
  validTo: string | null
  status: string
}

export interface OcsTransactionDto {
  id: string
  balanceId: string
  transactionType: string
  amount: number
  direction: string
  description: string | null
  createdAt: string
  correlationId: string | null
}

export interface AdjustBalanceRequest {
  amount: number
  direction: string
  description: string | null
}

export function useOcsBalances(filters: Record<string, string> = {}) {
  return useQuery<OcsBalanceDto[]>({
    queryKey: queryKeys.ocs.balances.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams(filters)
      const res = await api.get(`/api/v1/ocs/balances?${params.toString()}`)
      return res.data
    },
  })
}

export function useOcsBalance(id: string) {
  return useQuery<OcsBalanceDto>({
    queryKey: queryKeys.ocs.balances.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/ocs/balances/${id}`)
      return res.data
    },
    enabled: !!id,
  })
}

export function useAdjustBalance(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: AdjustBalanceRequest) => {
      const res = await api.post(`/api/v1/ocs/balances/${id}/adjust`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.ocs.balances.all() })
    },
  })
}

export function useOcsCreditPools(filters: Record<string, string> = {}) {
  return useQuery<OcsCreditPoolDto[]>({
    queryKey: queryKeys.ocs.creditPools.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams(filters)
      const res = await api.get(`/api/v1/ocs/credit-pools?${params.toString()}`)
      return res.data
    },
  })
}

export function useOcsCreditPool(id: string) {
  return useQuery<OcsCreditPoolDto>({
    queryKey: queryKeys.ocs.creditPools.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/ocs/credit-pools/${id}`)
      return res.data
    },
    enabled: !!id,
  })
}

export function useOcsTransactions(filters: Record<string, string> = {}) {
  return useQuery<OcsTransactionDto[]>({
    queryKey: queryKeys.ocs.transactions.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams(filters)
      const res = await api.get(`/api/v1/ocs/transactions?${params.toString()}`)
      return res.data
    },
  })
}

export function useReserveCredit() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: { subscriptionId: string; amount: number; description?: string }) => {
      const res = await api.post("/api/v1/ocs/reserve", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.ocs.balances.all() })
      queryClient.invalidateQueries({ queryKey: queryKeys.ocs.creditPools.all() })
    },
  })
}
