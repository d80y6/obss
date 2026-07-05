import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProductDto } from '@/api/generated/dto'

interface CreateProductPayload {
  name: string;
  description: string | null;
  productType: string;
  isShippable: boolean;
  taxable: boolean;
  taxCategory: string | null;
  categoryId: string | null;
  specifications: unknown[] | null;
}

export function useCreateProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateProductPayload) => {
      const res = await api.post<ProductDto>("/api/v1/catalog/products", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.products.lists() })
    },
  })
}
