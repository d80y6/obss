import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProvisioningTemplateDto } from '@/api/generated/dto'
import type { CreateProvisioningTemplateCommand } from '@/api/generated'

export function useProvisioningTemplates(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.provisioning.templates.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/provisioning/templates?${params.toString()}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as ProvisioningTemplateDto[], total }
    },
  })
}

export function useProvisioningTemplate(id: string) {
  return useQuery({
    queryKey: queryKeys.provisioning.templates.detail(id),
    queryFn: async () => {
      const res = await api.get("/api/v1/provisioning/templates")
      const items = res.data as ProvisioningTemplateDto[]
      return items.find((t) => t.id === id) ?? null
    },
    enabled: !!id,
  })
}

export function useCreateProvisioningTemplate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateProvisioningTemplateCommand) => {
      const res = await api.post("/api/v1/provisioning/templates", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.provisioning.templates.all })
    },
  })
}

export function useUpdateProvisioningTemplate() {
  return useMutation({
    mutationFn: async () => {
      throw new Error("Update provisioning template is not available")
    },
  })
}
