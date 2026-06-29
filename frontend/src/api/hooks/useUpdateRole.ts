import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RoleDto } from '@/api/generated/dto'

export function useUpdateRole(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: Partial<RoleDto>) => {
      const res = await api.put<RoleDto>(`/api/v1/iam/roles/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.roles.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.roles.lists() })
    },
  })
}
