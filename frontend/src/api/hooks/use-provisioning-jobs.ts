import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { ProvisioningJobDto, ProvisioningLogEntryDto } from '@/api/generated/dto'
import type { CreateProvisioningJobCommand } from '@/api/generated'

export function useProvisioningJobs(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.provisioning.jobs.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/provisioning/jobs?${params.toString()}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as ProvisioningJobDto[], total }
    },
  })
}

export function useProvisioningJob(id: string) {
  return useQuery({
    queryKey: queryKeys.provisioning.jobs.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/provisioning/jobs/${id}`)
      return res.data as ProvisioningJobDto
    },
    enabled: !!id,
  })
}

export function useProvisioningJobLogs(id: string) {
  return useQuery({
    queryKey: queryKeys.provisioning.jobs.logs(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/provisioning/jobs/${id}/logs`)
      return res.data as ProvisioningLogEntryDto[]
    },
    enabled: !!id,
  })
}

export function useCreateProvisioningJob() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateProvisioningJobCommand) => {
      const res = await api.post("/api/v1/provisioning/jobs", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.provisioning.jobs.all })
    },
  })
}

export function useStartProvisioningJob() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/provisioning/jobs/${id}/start`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.provisioning.jobs.all })
    },
  })
}

export function useRetryProvisioningJob() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/provisioning/jobs/${id}/retry`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.provisioning.jobs.all })
    },
  })
}
