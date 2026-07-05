import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

interface AllocateIPPayload {
  networkInterfaceId: string | null;
  ipAddress: string;
  subnetMask: string;
  gateway: string | null;
  addressType: string;
  assignedTo: string | null;
}

export function useAllocateIP(elementId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: AllocateIPPayload) => {
      await api.post(`/api/v1/network/elements/${elementId}/ip-addresses`, data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.elements.detail(elementId) })
    },
  })
}
