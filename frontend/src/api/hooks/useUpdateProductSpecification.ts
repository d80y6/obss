import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProductSpecificationDto } from '@/api/generated/dto'

interface UpdateProductSpecificationPayload {
  name: string;
  description: string | null;
  brand: string | null;
  version: string | null;
  productNumber: string | null;
}

export function useUpdateProductSpecification(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: UpdateProductSpecificationPayload) => {
      const res = await api.put<ProductSpecificationDto>(`/api/v1/catalog/product-specifications/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.lists() })
    },
  })
}
