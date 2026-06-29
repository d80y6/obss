import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { CategoryDto } from '@/api/generated/dto'
import type { CreateCategoryRequest } from '@/api/generated'

export function useCreateCategory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateCategoryRequest) => {
      const res = await api.post<CategoryDto>("/api/v1/catalog/categories", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["product-categories"] })
    },
  })
}
