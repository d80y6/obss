import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export interface GeographicAddressDto {
  street: string
  city: string
  state: string | null
  postalCode: string | null
  country: string
}

export interface AlternateProposalDto {
  serviceId: string
  serviceName: string
  resultType: string
  estimatedInstallDate: string | null
  guaranteedUntil: string | null
}

export interface QualificationItemDto {
  id: string
  serviceId: string
  serviceName: string
  resultType: string
  state: string
  estimatedInstallDate: string | null
  estimatedCompletionDate: string | null
  eligibilityUnavailableReason: string | null
  alternateProposals: AlternateProposalDto[]
}

export interface ServiceQualificationDto {
  id: string
  customerId: string
  address: GeographicAddressDto
  state: string
  requestedDate: string
  expirationDate: string | null
  description: string | null
  items: QualificationItemDto[]
}

export interface QualificationRequestItem {
  serviceId: string
  serviceName: string
}

export interface CheckServiceQualificationCommand {
  customerId: string
  address: GeographicAddressDto
  requestedServices: QualificationRequestItem[]
  description: string | null
}

export function useServiceQualifications() {
  return useQuery<ServiceQualificationDto[]>({
    queryKey: queryKeys.serviceQualification.lists(),
    queryFn: async () => {
      const res = await api.get("/api/v1/service-qualification")
      return res.data as ServiceQualificationDto[]
    },
  })
}

export function useServiceQualification(id: string) {
  return useQuery<ServiceQualificationDto>({
    queryKey: queryKeys.serviceQualification.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-qualification/${id}`)
      return res.data as ServiceQualificationDto
    },
    enabled: !!id,
  })
}

export function useCheckServiceQualification() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (command: CheckServiceQualificationCommand) => {
      const res = await api.post("/api/v1/service-qualification", command)
      return res.data as ServiceQualificationDto
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.serviceQualification.all })
    },
  })
}
