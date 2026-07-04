import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"

export function useDeleteBundledOffering() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ offerId, bundledOfferingId }: { offerId: string; bundledOfferingId: string }) => {
      await api.delete(`/api/v1/catalog/offers/${offerId}/bundled-offerings/${bundledOfferingId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["offers", variables.offerId, "bundled-offerings"] })
    },
  })
}
