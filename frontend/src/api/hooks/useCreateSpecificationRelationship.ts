import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProductSpecificationRelationshipDto } from '@/api/generated/dto'
import { queryKeys } from "@/lib/query-keys"

export interface CreateRelationshipPayload {
  specId: string;
  targetSpecificationId: string;
  relationshipType: string;
  role: string | null;
  validFrom: string | null;
  validTo: string | null;
}

export function useCreateSpecificationRelationship() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateRelationshipPayload) => {
      const res = await api.post<ProductSpecificationRelationshipDto>(`/api/v1/catalog/product-specifications/${data.specId}/relationships`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(variables.specId) })
    },
  })
}
