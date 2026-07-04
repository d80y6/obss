import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProductSpecificationDto } from '@/api/generated/dto'

export function useProductSpecification(id: string) {
  return useQuery({
    queryKey: queryKeys.productSpecifications.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/product-specifications/${id}`)
      return res.data as ProductSpecificationDto
    },
    enabled: !!id,
  })
}
