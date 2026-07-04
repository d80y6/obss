import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import { queryKeys } from "@/lib/query-keys"

export function useDeleteSpecificationRelationship() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ specId, relId }: { specId: string; relId: string }) => {
      await api.delete(`/api/v1/catalog/product-specifications/${specId}/relationships/${relId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(variables.specId) })
    },
  })
}
