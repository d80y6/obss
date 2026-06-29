import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { DiscoveryJobDto, ServiceResourceDto, ServiceDto, ServiceTopologyDto } from '@/api/generated/dto'
import type { CreateServiceCommand } from '@/api/generated'

export function useServices(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.serviceInventory.services.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/service-inventory/services?${params.toString()}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as ServiceDto[], total }
    },
  })
}

export function useService(id: string) {
  return useQuery({
    queryKey: queryKeys.serviceInventory.services.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/services/${id}`)
      return res.data as ServiceDto
    },
    enabled: !!id,
  })
}

export function useServiceTopology(id: string) {
  return useQuery({
    queryKey: queryKeys.serviceInventory.services.topology(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/services/${id}/topology`)
      return res.data as ServiceTopologyDto | null
    },
    enabled: !!id,
  })
}

export function useServiceResources(id: string) {
  return useQuery({
    queryKey: queryKeys.serviceInventory.services.resources(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/services/${id}/resources`)
      return res.data as ServiceResourceDto[]
    },
    enabled: !!id,
  })
}

export function useCreateService() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateServiceCommand) => {
      const res = await api.post("/api/v1/service-inventory/services", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.services.all })
    },
  })
}

export function useActivateService() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/service-inventory/services/${id}/activate`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.services.all })
    },
  })
}

export function useSuspendService() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/service-inventory/services/${id}/suspend`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.services.all })
    },
  })
}

export function useDecommissionService() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/service-inventory/services/${id}/decommission`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.services.all })
    },
  })
}

export function useDiscoveryJobs(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.serviceInventory.discovery.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/service-inventory/services/discovery/jobs?${params.toString()}`)
      return res.data as DiscoveryJobDto[]
    },
  })
}
