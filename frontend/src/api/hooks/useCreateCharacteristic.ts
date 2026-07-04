import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProductSpecificationCharacteristicDto } from '@/api/generated/dto'
import { queryKeys } from "@/lib/query-keys"

export interface CreateCharacteristicPayload {
  specId: string;
  name: string;
  description: string | null;
  valueType: string;
  configurable: boolean;
  sortOrder: number;
  isRequired: boolean;
  minValue: number | null;
  maxValue: number | null;
  regex: string | null;
  maxCardinality: number | null;
}

export function useCreateCharacteristic() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateCharacteristicPayload) => {
      const res = await api.post<ProductSpecificationCharacteristicDto>(`/api/v1/catalog/product-specifications/${data.specId}/characteristics`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(variables.specId) })
    },
  })
}
