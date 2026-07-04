import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import { queryKeys } from "@/lib/query-keys"

export function useDeleteCharacteristicValue() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ specId, charId, valueId }: { specId: string; charId: string; valueId: string }) => {
      await api.delete(`/api/v1/catalog/product-specifications/${specId}/characteristics/${charId}/values/${valueId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(variables.specId) })
    },
  })
}
