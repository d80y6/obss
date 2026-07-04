import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProductSpecificationCharacteristicDto } from '@/api/generated/dto'
import { queryKeys } from "@/lib/query-keys"

export function useUpdateCharacteristic() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ specId, charId, data }: { specId: string; charId: string; data: Record<string, unknown> }) => {
      const res = await api.put<ProductSpecificationCharacteristicDto>(`/api/v1/catalog/product-specifications/${specId}/characteristics/${charId}`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(variables.specId) })
    },
  })
}
