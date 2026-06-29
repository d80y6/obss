import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RoleDto } from '@/api/generated/dto'

export function useCreateRole() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: Partial<RoleDto>) => {
      const res = await api.post<RoleDto>("/api/v1/iam/roles", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.roles.lists() })
    },
  })
}
