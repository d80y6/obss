import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { SubnetDto } from '@/api/generated/dto'

export function useSubnets() {
  return useQuery({
    queryKey: queryKeys.networks.subnets.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/subnets")
      return res.data as SubnetDto[]
    },
  })
}

export function useSubnet(id: string) {
  return useQuery({
    queryKey: queryKeys.networks.subnets.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/subnets/${id}`)
      return res.data as SubnetDto
    },
    enabled: !!id,
  })
}

export function useCreateSubnet() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post("/api/v1/network/subnets", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.subnets.all })
    },
  })
}

export function useUpdateSubnet() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: Record<string, unknown>) => {
      const res = await api.put(`/api/v1/network/subnets/${id}`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.subnets.detail(variables.id as string) })
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.subnets.all })
    },
  })
}
