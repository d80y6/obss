import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

export function useNasDevice(id: string) {
  return useQuery({
    queryKey: queryKeys.aaa.nas.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/aaa/nas/${id}`)
      return res.data as NasDto
    },
    enabled: !!id,
  })
}
