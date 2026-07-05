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

export function useServicesBySubscription(subscriptionId: string) {
  return useQuery({
    queryKey: ["service-inventory", "subscriptions", subscriptionId, "services"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/subscriptions/${subscriptionId}/services`)
      return res.data as ServiceDto[]
    },
    enabled: !!subscriptionId,
  })
}

export function useUpstreamServices(serviceId: string) {
  return useQuery({
    queryKey: ["service-inventory", "services", serviceId, "upstream"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/services/${serviceId}/topology/upstream`)
      return res.data as string[]
    },
    enabled: !!serviceId,
  })
}

export function useDownstreamServices(serviceId: string) {
  return useQuery({
    queryKey: ["service-inventory", "services", serviceId, "downstream"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/services/${serviceId}/topology/downstream`)
      return res.data as string[]
    },
    enabled: !!serviceId,
  })
}

export function useCreateTopology() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ serviceId, topologyType }: { serviceId: string; topologyType: string }) => {
      const res = await api.post(`/api/v1/service-inventory/services/${serviceId}/topology`, { serviceId, topologyType })
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["service-inventory", "services", variables.serviceId, "topology"] })
    },
  })
}

export function useAddTopologyLink() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: { serviceTopologyId: string; sourceServiceId: string; targetServiceId: string; linkType: string; direction: string; attributes?: string }) => {
      const res = await api.post(`/api/v1/service-inventory/services/topology/${data.serviceTopologyId}/links`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["service-inventory", "services", variables.sourceServiceId, "topology"] })
    },
  })
}

export function useStartDiscoveryJob() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: { tenantId: string; discoveryType: string; configuration?: string; createdBy: string }) => {
      const res = await api.post("/api/v1/service-inventory/services/discovery", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.discovery.all })
    },
  })
}

export function useCompleteDiscoveryJob() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: { jobId: string; resourcesFound: number; resourcesMatched: number; errorMessage?: string }) => {
      const res = await api.put(`/api/v1/service-inventory/services/discovery/jobs/${data.jobId}/complete`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.discovery.all })
    },
  })
}

export function useUnmatchedResources(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: ["service-inventory", "discovery", "unmatched", filters],
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/service-inventory/services/discovery/unmatched?${params.toString()}`)
      return res.data as DiscoveryJobDto[]
    },
  })
}

export function useUpdateService(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: { configuration?: string; location?: string }) => {
      const res = await api.patch(`/api/v1/service-inventory/services/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.services.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.services.all })
    },
  })
}

export function useResumeService() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/service-inventory/services/${id}/resume`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.services.all })
    },
  })
}

export function useRemoveTopologyLink() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ topologyId, linkId }: { topologyId: string; linkId: string }) => {
      await api.delete(`/api/v1/service-inventory/services/topology/${topologyId}/links/${linkId}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceInventory.services.all })
    },
  })
}
