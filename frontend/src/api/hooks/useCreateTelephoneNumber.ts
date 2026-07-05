import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { TelephoneNumberDto } from '@/api/generated/dto'

interface CreateTelephoneNumberPayload {
  number: string;
  numberType: string;
  cost?: number;
  currency?: string;
  notes?: string;
}

export function useCreateTelephoneNumber() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateTelephoneNumberPayload) => {
      const res = await api.post<TelephoneNumberDto>("/api/v1/number-inventory/numbers", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.numberInventory.numbers.lists() })
    },
  })
}
