import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CustomerDto } from '@/api/generated/dto'
import type { UpdateCustomerCommand } from '@/api/generated'

export function useUpdateCustomer(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: UpdateCustomerCommand) => {
      const res = await api.put<CustomerDto>(`/api/v1/crm/customers/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.lists() })
    },
  })
}
