import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export interface RelatedPartyDto {
  partyId: string
  partyName: string
  role: string
}

export interface ProductOrderMilestoneDto {
  id: string
  productOrderId: string
  name: string
  description: string
  milestoneDate: string
  status: string
}

export interface ProductOrderItemRelationshipDto {
  id: string
  productOrderItemId: string
  targetItemId: string
  type: string
}

export interface ProductOrderPaymentDto {
  id: string
  orderId: string
  amount: number
  paymentMethod: string
  paymentReference: string
  paidAt: string
  status: string
}

export interface OrderFulfillmentDto {
  id: string
  orderId: string
  status: string
  workflowInstanceId: string | null
  startedAt: string
  completedAt: string | null
  errorMessage: string | null
}

export interface ProductOrderItemDto {
  id: string
  orderId: string
  productId: string
  offerId: string
  productName: string
  offerName: string
  quantity: number
  unitPrice: number
  recurringPrice: number
  discountAmount: number
  taxAmount: number
  totalPrice: number
  billingPeriod: string
  startDate: string
  endDate: string | null
  isActive: boolean
  serviceType?: string | null
  action?: string | null
  itemState?: string | null
  state: string
}

export interface ProductOrderDto {
  id: string
  tenantId: string
  orderNumber: string
  customerId: string
  customerName: string
  orderDate: string
  status: string
  orderType: string
  subTotal: number
  taxTotal: number
  discountTotal: number
  grandTotal: number
  currency: string
  notes: string | null
  billingAddressStreet: string | null
  billingAddressCity: string | null
  billingAddressState: string | null
  billingAddressPostalCode: string | null
  billingAddressCountry: string | null
  shippingAddressStreet: string | null
  shippingAddressCity: string | null
  shippingAddressState: string | null
  shippingAddressPostalCode: string | null
  shippingAddressCountry: string | null
  createdById: string
  approvedById: string | null
  approvedAt: string | null
  cancellationReason: string | null
  description: string | null
  channel: string | null
  priority: string | null
  requestedStartDate: string | null
  requestedCompletionDate: string | null
  expectedCompletionDate: string | null
  notificationContact: string | null
  externalId: string | null
  quoteId: string | null
  href: string | null
  atType: string | null
  atBaseType: string | null
  atSchemaLocation: string | null
  completionDate: string | null
  createdAt: string
  billingAccountId: string | null
  billingAccountHref: string | null
  orderVersion: number
  orderPriority: string
  productOfferingQualificationId: string | null
  productOfferingQualificationHref: string | null
  quoteHref: string | null
  items: ProductOrderItemDto[]
  relatedParties: RelatedPartyDto[] | null
  payments: ProductOrderPaymentDto[]
  fulfillment: OrderFulfillmentDto | null
  milestones: ProductOrderMilestoneDto[] | null
  itemRelationships: ProductOrderItemRelationshipDto[] | null
}

export interface ProductOrderSummaryDto {
  id: string
  orderNumber: string
  customerId: string
  customerName: string
  orderDate: string
  status: string
  orderType: string
  grandTotal: number
  currency: string
  notes: string | null
  description: string | null
  channel: string | null
  priority: string | null
  requestedStartDate: string | null
  requestedCompletionDate: string | null
  href: string | null
  atType: string | null
  atBaseType: string | null
  completionDate: string | null
  orderVersion: number
  orderPriority: string
  relatedParties: RelatedPartyDto[] | null
}

// Queries
export function useProductOrders(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.orders.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/productOrder/orders?${params.toString()}`)
      const total = res.headers["x-total-count"] ? parseInt(res.headers["x-total-count"], 10) : null
      return { items: res.data as ProductOrderDto[], total }
    },
  })
}

export function useProductOrder(id: string) {
  return useQuery({
    queryKey: queryKeys.orders.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/productOrder/orders/${id}`)
      return res.data as ProductOrderDto
    },
    enabled: !!id,
  })
}

export function useProductOrderRelationships(orderId: string) {
  return useQuery({
    queryKey: queryKeys.orders.relationships(orderId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/productOrder/${orderId}/relationships`)
      return res.data as ProductOrderItemRelationshipDto[]
    },
    enabled: !!orderId,
  })
}

export function useProductOrderMilestones(orderId: string) {
  return useQuery({
    queryKey: queryKeys.orders.milestones(orderId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/productOrder/${orderId}/milestones`)
      return res.data as ProductOrderMilestoneDto[]
    },
    enabled: !!orderId,
  })
}

// Mutations
export function useCreateProductOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/productOrder/orders", data)
      return res.data as ProductOrderDto
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.lists() })
    },
  })
}

export function useUpdateProductOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: { id: string } & Record<string, unknown>) => {
      const res = await api.patch(`/api/v1/productOrder/orders/${id}`, data)
      return res.data as ProductOrderDto
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.lists() })
    },
  })
}

export function useDeleteProductOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/productOrder/orders/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useSubmitProductOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.post(`/api/v1/productOrder/orders/${id}/submit`)
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(id) })
    },
  })
}

export function useApproveProductOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.post(`/api/v1/productOrder/orders/${id}/approve`)
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(id) })
    },
  })
}

export function useCancelProductOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, reason }: { id: string; reason: string }) => {
      await api.post(`/api/v1/productOrder/orders/${id}/cancel`, { orderId: id, reason } as Record<string, unknown>)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(variables.id) })
    },
  })
}

export function useValidateProductOrder() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/productOrder/orders/${id}/validate`)
      return res.data
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(id) })
    },
  })
}

// Item state transition mutations
export function useAcknowledgeProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: string; itemId: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/acknowledge`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useStartProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: string; itemId: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/start`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useHoldProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: string; itemId: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/hold`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useResumeProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: string; itemId: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/resume`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useAssessProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: string; itemId: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/assess`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useRejectProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId, reason }: { orderId: string; itemId: string; reason: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/reject`, { reason } as Record<string, unknown>)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useCompleteProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: string; itemId: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/complete`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useFailProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId, error }: { orderId: string; itemId: string; error: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/fail`, { error } as Record<string, unknown>)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

export function useCancelProductOrderItem() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: string; itemId: string }) => {
      await api.post(`/api/v1/productOrder/${orderId}/items/${itemId}/cancel`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.all })
    },
  })
}

// Relationship mutations
export function useAddProductOrderRelationship() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, itemId, targetItemId, type }: {
      orderId: string
      itemId: string
      targetItemId: string
      type: string
    }) => {
      await api.post(`/api/v1/productOrder/${orderId}/relationships`, {
        orderId,
        itemId,
        targetItemId,
        type,
      } as Record<string, unknown>)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.relationships(variables.orderId) })
    },
  })
}

export function useRemoveProductOrderRelationship() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, relationshipId }: { orderId: string; relationshipId: string }) => {
      await api.delete(`/api/v1/productOrder/${orderId}/relationships/${relationshipId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.relationships(variables.orderId) })
    },
  })
}

// Milestone mutations
export function useCreateProductOrderMilestone() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, name, description, milestoneDate }: {
      orderId: string
      name: string
      description: string
      milestoneDate: string
    }) => {
      await api.post(`/api/v1/productOrder/${orderId}/milestones`, {
        orderId,
        name,
        description,
        milestoneDate,
      } as Record<string, unknown>)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.milestones(variables.orderId) })
    },
  })
}

export function useUpdateProductOrderMilestone() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, milestoneId, status, milestoneDate }: {
      orderId: string
      milestoneId: string
      status: string
      milestoneDate?: string
    }) => {
      await api.patch(`/api/v1/productOrder/${orderId}/milestones/${milestoneId}`, {
        status,
        milestoneDate,
      } as Record<string, unknown>)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.milestones(variables.orderId) })
    },
  })
}

export function useRemoveProductOrderMilestone() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ orderId, milestoneId }: { orderId: string; milestoneId: string }) => {
      await api.delete(`/api/v1/productOrder/${orderId}/milestones/${milestoneId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.milestones(variables.orderId) })
    },
  })
}
