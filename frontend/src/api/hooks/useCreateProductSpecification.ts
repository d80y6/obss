import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProductSpecificationDto } from '@/api/generated/dto'

export interface CreateProductSpecificationPayload {
  tenantId: string;
  name: string;
  description: string | null;
  brand: string | null;
  version: string | null;
  productNumber: string | null;
}

export function useCreateProductSpecification() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateProductSpecificationPayload) => {
      const res = await api.post<ProductSpecificationDto>("/api/v1/catalog/product-specifications", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.lists() })
    },
  })
}
