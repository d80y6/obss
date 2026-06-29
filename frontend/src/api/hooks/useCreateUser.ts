import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { IamUserDto } from '@/api/generated/dto'
import type { CreateUserCommand } from '@/api/generated'

export function useCreateUser() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateUserCommand) => {
      const res = await api.post<IamUserDto>("/api/v1/iam/users", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.lists() })
    },
  })
}
