import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import { queryKeys } from "@/lib/query-keys"

export function useDeleteCharacteristic() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ specId, charId }: { specId: string; charId: string }) => {
      await api.delete(`/api/v1/catalog/product-specifications/${specId}/characteristics/${charId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.productSpecifications.detail(variables.specId) })
    },
  })
}
