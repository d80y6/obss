import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProductDto } from '@/api/generated/dto'

export function useProduct(id: string) {
  return useQuery({
    queryKey: queryKeys.products.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/products/${id}`)
      return res.data as ProductDto
    },
    enabled: !!id,
  })
}
