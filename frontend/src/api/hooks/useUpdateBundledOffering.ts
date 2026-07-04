import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { BundledProductOfferingDto } from '@/api/generated/dto'

export function useUpdateBundledOffering() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ offerId, id, data }: { offerId: string; id: string; data: Record<string, unknown> }) => {
      const res = await api.put<BundledProductOfferingDto>(`/api/v1/catalog/offers/${offerId}/bundled-offerings/${id}`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["offers", variables.offerId, "bundled-offerings"] })
    },
  })
}
