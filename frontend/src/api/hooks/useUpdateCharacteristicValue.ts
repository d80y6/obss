import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProductSpecificationCharacteristicValueDto } from '@/api/generated/dto'
import { queryKeys } from "@/lib/query-keys"

export function useUpdateCharacteristicValue() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ specId, charId, valueId, data }: { specId: string; charId: string; valueId: string; data: Record<string, unknown> }) => {
      const res = await api.put<ProductSpecificationCharacteristicValueDto>(`/api/v1/catalog/product-specifications/${specId}/characteristics/${charId}/values/${valueId}`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(variables.specId) })
    },
  })
}
