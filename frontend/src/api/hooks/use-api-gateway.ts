import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ApiKeyDto, ApiRouteDto, PartnerDto } from '@/api/generated/dto'
import type { RegisterApiRouteCommand, CreateApiKeyCommand, RegisterPartnerCommand } from '@/api/generated'

export function useApiRoutes() {
  return useQuery({
    queryKey: queryKeys.gateway.routes.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/gateway/routes")
      return res.data as ApiRouteDto[]
    },
  })
}

export function useApiRoute(id: string) {
  return useQuery({
    queryKey: ["gateway", "routes", id],
    queryFn: async () => {
      const res = await api.get("/api/v1/gateway/routes")
      const items = res.data as ApiRouteDto[]
      return items.find((r) => r.id === id) ?? null
    },
    enabled: !!id,
  })
}

export function useCreateApiRoute() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RegisterApiRouteCommand) => {
      const res = await api.post("/api/v1/gateway/routes", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.gateway.routes.list() })
    },
  })
}

export function useUpdateApiRoute() {
  return useMutation({
    mutationFn: async () => {
      throw new Error("Update API route is not available")
    },
  })
}

export function useApiKeys() {
  return useQuery({
    queryKey: queryKeys.gateway.apiKeys.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/gateway/api-keys")
      return res.data as ApiKeyDto[]
    },
  })
}

export function useApiKey(id: string) {
  return useQuery({
    queryKey: ["gateway", "api-keys", id],
    queryFn: async () => {
      const res = await api.get("/api/v1/gateway/api-keys")
      const items = res.data as ApiKeyDto[]
      return items.find((k) => k.id === id) ?? null
    },
    enabled: !!id,
  })
}

export function useCreateApiKey() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateApiKeyCommand) => {
      const res = await api.post("/api/v1/gateway/api-keys", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.gateway.apiKeys.list() })
    },
  })
}

export function useRevokeApiKey() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/gateway/api-keys/${id}/revoke`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.gateway.apiKeys.list() })
    },
  })
}

export function usePartners() {
  return useQuery({
    queryKey: queryKeys.gateway.partners.list(),
    queryFn: async () => {
      const res = await api.get("/api/v1/gateway/partners")
      return res.data as PartnerDto[]
    },
  })
}

export function usePartner(id: string) {
  return useQuery({
    queryKey: ["gateway", "partners", id],
    queryFn: async () => {
      const res = await api.get("/api/v1/gateway/partners")
      const items = res.data as PartnerDto[]
      return items.find((p) => p.id === id) ?? null
    },
    enabled: !!id,
  })
}

export function useCreatePartner() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: RegisterPartnerCommand) => {
      const res = await api.post("/api/v1/gateway/partners", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.gateway.partners.list() })
    },
  })
}

export function useUpdatePartner() {
  return useMutation({
    mutationFn: async () => {
      throw new Error("Update partner is not available")
    },
  })
}
