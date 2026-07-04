import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"

export function useDeletePriceRange() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ offerId, pricingId, rangeId }: { offerId: string; pricingId: string; rangeId: string }) => {
      await api.delete(`/api/v1/catalog/offers/${offerId}/pricing/${pricingId}/price-ranges/${rangeId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["price-ranges", variables.pricingId] })
    },
  })
}
