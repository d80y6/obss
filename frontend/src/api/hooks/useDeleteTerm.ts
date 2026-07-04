import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"

export function useDeleteTerm() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ offerId, termId }: { offerId: string; termId: string }) => {
      await api.delete(`/api/v1/catalog/offers/${offerId}/terms/${termId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["offers", variables.offerId, "terms"] })
    },
  })
}
