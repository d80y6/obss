import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { TelephoneNumberDto } from '@/api/generated/dto'

export function useTelephoneNumber(id: string) {
  return useQuery({
    queryKey: queryKeys.numberInventory.numbers.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/number-inventory/numbers/${id}`)
      return res.data as TelephoneNumberDto
    },
    enabled: !!id,
  })
}
