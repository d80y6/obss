import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export interface AccountHolderDto {
  name: string
  email: string | null
  phone: string | null
  contactId: string | null
}

export interface RelatedPartyDto {
  partyId: string
  partyName: string
  role: string
}

export interface BillPresentationMediaDto {
  id: string
  mediaType: string
  emailAddress: string | null
  paperFormat: string | null
  language: string
  isPreferred: boolean
  validFrom: string | null
  validUntil: string | null
}

export interface BillingAccountDto {
  id: string
  customerId: string
  accountType: string
  name: string
  status: string
  creditLimit: number
  currency: string
  validFrom: string | null
  validUntil: string | null
  description: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string
  href: string | null
  atType: string | null
  atBaseType: string | null
  atSchemaLocation: string | null
  externalId: string | null
  relatedParties: RelatedPartyDto[] | null
  accountHolder: AccountHolderDto | null
  billPresentationMedia: BillPresentationMediaDto[] | null
  paymentMethodId: string | null
}

export interface BalanceTransactionDto {
  id: string
  amount: number
  transactionType: string
  description: string
  transactionDate: string
  referenceId: string | null
  referenceType: string | null
}

export interface AccountBalanceDto {
  id: string
  billingAccountId: string
  currentBalance: number
  outstandingBalance: number
  availableCredit: number
  currency: string
  balanceDate: string
  lastUpdatedAt: string
  atType: string | null
  atBaseType: string | null
  atSchemaLocation: string | null
  transactions: BalanceTransactionDto[] | null
}

export function useBillingAccounts(filters: Record<string, string> = {}) {
  return useQuery<BillingAccountDto[]>({
    queryKey: queryKeys.billing.billingAccounts.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams(filters)
      const res = await api.get(`/api/v1/billing/billing-accounts?${params.toString()}`)
      return res.data
    },
  })
}

export function useBillingAccount(id: string) {
  return useQuery<BillingAccountDto>({
    queryKey: queryKeys.billing.billingAccounts.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/billing/billing-accounts/${id}`)
      return res.data
    },
    enabled: !!id,
  })
}

export function useBillingAccountBalance(id: string) {
  return useQuery<AccountBalanceDto>({
    queryKey: queryKeys.billing.billingAccounts.balance(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/billing/billing-accounts/${id}/balance`)
      return res.data
    },
    enabled: !!id,
  })
}

export function useCreateBillingAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      customerId: string
      accountType: string
      name: string
      creditLimit: number
      currency: string
    }) => {
      const res = await api.post("/api/v1/billing/billing-accounts", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.billingAccounts.all })
    },
  })
}

export function useUpdateBillingAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string
      name: string
      creditLimit: number
      currency: string
      description: string | null
    }) => {
      const res = await api.put(`/api/v1/billing/billing-accounts/${id}`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.billingAccounts.detail(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.billingAccounts.lists() })
    },
  })
}

export function useDeleteBillingAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/billing/billing-accounts/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.billingAccounts.all })
    },
  })
}

export function useAddBillingAccountRelatedParty() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ billingAccountId, ...data }: {
      billingAccountId: string
      partyId: string
      partyName: string
      role: string
    }) => {
      const res = await api.post(
        `/api/v1/billing/billing-accounts/${billingAccountId}/related-parties`,
        data,
      )
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.billing.billingAccounts.detail(variables.billingAccountId),
      })
    },
  })
}

export function useRemoveBillingAccountRelatedParty() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ billingAccountId, partyId }: { billingAccountId: string; partyId: string }) => {
      await api.delete(`/api/v1/billing/billing-accounts/${billingAccountId}/related-parties/${partyId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.billing.billingAccounts.detail(variables.billingAccountId),
      })
    },
  })
}

export function useCreateBillPresentationMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ billingAccountId, ...data }: {
      billingAccountId: string
      mediaType: string
      emailAddress: string | null
      paperFormat: string | null
      language: string
      isPreferred: boolean
    }) => {
      const res = await api.post(
        `/api/v1/billing/billing-accounts/${billingAccountId}/presentation-media`,
        data,
      )
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.billing.billingAccounts.detail(variables.billingAccountId),
      })
    },
  })
}

export function useRemoveBillPresentationMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ billingAccountId, mediaId }: { billingAccountId: string; mediaId: string }) => {
      await api.delete(
        `/api/v1/billing/billing-accounts/${billingAccountId}/presentation-media/${mediaId}`,
      )
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.billing.billingAccounts.detail(variables.billingAccountId),
      })
    },
  })
}
