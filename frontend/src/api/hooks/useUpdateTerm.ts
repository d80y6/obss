import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProductOfferingTermDto } from '@/api/generated/dto'

export function useUpdateTerm() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ offerId, termId, data }: { offerId: string; termId: string; data: Record<string, unknown> }) => {
      const res = await api.put<ProductOfferingTermDto>(`/api/v1/catalog/offers/${offerId}/terms/${termId}`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["offers", variables.offerId, "terms"] })
    },
  })
}
