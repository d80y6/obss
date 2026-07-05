import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"

export function useEntitlementCheck(id: string, type: string, amount: number) {
  return useQuery({
    queryKey: ["subscriptions", id, "entitlements", "check", type, amount],
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/subscriptions/${id}/entitlements/check?type=${type}&amount=${amount}`)
      return res.data as boolean
    },
    enabled: !!id && !!type && amount > 0,
  })
}
