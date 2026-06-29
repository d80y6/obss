import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OfferDto } from '@/api/generated/dto'
import type { CreateOfferCommand } from '@/api/generated'

export function useCreateOffer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateOfferCommand) => {
      const res = await api.post<OfferDto>("/api/v1/catalog/offers", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.offers.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.products.all })
    },
  })
}
