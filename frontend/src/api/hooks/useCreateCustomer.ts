import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CustomerDto } from '@/api/generated/dto'
import type { CreateCustomerCommand } from '@/api/generated'

export function useCreateCustomer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateCustomerCommand) => {
      const res = await api.post<CustomerDto>("/api/v1/crm/customers", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.lists() })
    },
  })
}
