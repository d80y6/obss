import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { RadiusSessionDto } from "@/api/generated/dto"

export function useSession(id: string) {
  return useQuery({
    queryKey: queryKeys.aaa.sessions.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/aaa/sessions/${id}`)
      return res.data as RadiusSessionDto
    },
    enabled: !!id,
  })
}
