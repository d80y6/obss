import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

interface AddNetworkInterfacePayload {
  name: string;
  description: string | null;
  interfaceType: string;
  speed: number;
  macAddress: string | null;
  mtu: number;
  connectedToInterfaceId: string | null;
}

export function useAddNetworkInterface(elementId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: AddNetworkInterfacePayload) => {
      await api.post(`/api/v1/network/elements/${elementId}/interfaces`, data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.elements.detail(elementId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.elements.connections(elementId) })
    },
  })
}
