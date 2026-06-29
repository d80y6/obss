import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { IamUserDto } from '@/api/generated/dto'

export function useUser(id: string) {
  return useQuery({
    queryKey: queryKeys.users.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/iam/users/${id}`)
      return res.data as IamUserDto
    },
    enabled: !!id,
  })
}
