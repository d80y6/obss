import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export interface QuoteDto {
  id: string
  tenantId: string
  externalId: string | null
  state: string
  quoteDate: string
  category: string | null
  description: string | null
  version: number
  validFrom: string | null
  validUntil: string | null
  expectedQuoteCompletionDate: string | null
  effectiveQuoteCompletionDate: string | null
  expectedFulfillmentStartDate: string | null
  customerId: string
  createdAt: string
  updatedAt: string
  items: QuoteItemDto[]
  relatedParties: RelatedPartyDto[]
  quotePrices: QuotePriceDto[]
  authorizations: QuoteAuthorizationDto[]
  billingAccountRefs: AccountRefDto[]
  agreementRefs: AgreementRefDto[]
  notes: NoteDto[]
}

export interface QuoteItemDto {
  id: string
  action: string
  state: string
  quantity: number
  productOfferingId: string | null
  productOfferingName: string | null
  productId: string | null
  createdAt: string
  updatedAt: string
  prices: QuotePriceDto[]
  itemRelationships: QuoteItemRelationshipDto[]
  notes: NoteDto[]
}

export interface QuotePriceDto {
  priceType: string
  name: string
  dutyFreeAmount: number
  taxIncludedAmount: number
  taxRate: number
  currency: string
  unitOfMeasure: string | null
  recurringPeriod: number | null
  recurringPeriodUnit: string | null
}

export interface QuoteAuthorizationDto {
  state: string
  requestedDate: string
  givenDate: string | null
  approverName: string | null
  approverRole: string | null
}

export interface QuoteItemRelationshipDto {
  itemId: string
  relatedItemId: string
  type: string
}

export interface NoteDto {
  date: string
  author: string
  text: string
}

export interface RelatedPartyDto {
  name: string
  role: string
  referredId: string
  referredType: string
}

export interface AccountRefDto {
  billingAccountId: string
  name: string
  accountType: string
  role: string
  href: string | null
}

export interface AgreementRefDto {
  agreementId: string
  name: string
  agreementType: string
  role: string
  href: string | null
}

export function useQuotes() {
  return useQuery<QuoteDto[]>({
    queryKey: queryKeys.quotes.lists(),
    queryFn: async () => {
      const res = await api.get("/api/v1/crm/quotes")
      return res.data as QuoteDto[]
    },
  })
}

export function useQuote(id: string) {
  return useQuery<QuoteDto>({
    queryKey: queryKeys.quotes.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/quotes/${id}`)
      return res.data as QuoteDto
    },
    enabled: !!id,
  })
}

export function useCustomerQuotes(customerId: string) {
  return useQuery<QuoteDto[]>({
    queryKey: [...queryKeys.quotes.all, "customer", customerId],
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${customerId}/quotes`)
      return res.data as QuoteDto[]
    },
    enabled: !!customerId,
  })
}