import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProductDto } from '@/api/generated/dto'
import type { UpdateProductCommand } from '@/api/generated'

export function useUpdateProduct(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: UpdateProductCommand) => {
      const res = await api.put<ProductDto>(`/api/v1/catalog/products/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.products.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.products.lists() })
    },
  })
}
