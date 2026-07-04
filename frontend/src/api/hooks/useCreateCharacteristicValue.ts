import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProductSpecificationCharacteristicValueDto } from '@/api/generated/dto'
import { queryKeys } from "@/lib/query-keys"

export interface CreateCharacteristicValuePayload {
  specId: string;
  characteristicId: string;
  value: string;
  unitOfMeasure: string | null;
  isDefault: boolean;
  valueFrom: string | null;
  valueTo: string | null;
  rangeInterval: string | null;
  validFrom: string | null;
  validTo: string | null;
}

export function useCreateCharacteristicValue() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateCharacteristicValuePayload) => {
      const res = await api.post<ProductSpecificationCharacteristicValueDto>(`/api/v1/catalog/product-specifications/${data.specId}/characteristics/${data.characteristicId}/values`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(variables.specId) })
    },
  })
}
