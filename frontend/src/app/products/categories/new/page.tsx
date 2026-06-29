"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery } from "@tanstack/react-query"
import { useQueryClient } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import type { CategoryDto } from "@/api/generated"

const categorySchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  parentCategoryId: z.string().optional(),
  sortOrder: z.coerce.number().default(0),
})

type CategoryForm = z.infer<typeof categorySchema>

export default function NewCategoryPage() {
  const router = useRouter()
  const queryClient = useQueryClient()

  const { data: categories } = useQuery({
    queryKey: ["product-categories"],
    queryFn: async () => {
      const res = await api.get("/api/v1/catalog/categories")
      return res.data as CategoryDto[]
    },
  })

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<CategoryForm>({
    resolver: zodResolver(categorySchema),
    defaultValues: { sortOrder: 0 },
  })

  const createMutation = useMutation({
    mutationFn: async (data: CategoryForm) => {
      const res = await api.post<CategoryDto>("/api/v1/catalog/categories", {
        name: data.name,
        description: data.description || null,
        parentCategoryId: data.parentCategoryId || null,
        sortOrder: data.sortOrder,
      })
      return res.data
    },
    onSuccess: (data) => {
      toast({ title: "Category created", description: `${data.name} has been created successfully.` })
      queryClient.invalidateQueries({ queryKey: ["product-categories"] })
      router.push(`/products/categories/${data.id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create category.", variant: "destructive" })
    },
  })

  const onSubmit = (data: CategoryForm) => {
    createMutation.mutate(data)
  }

  const categoryOptions = (categories ?? [])
    .filter((c) => !c.parentCategoryId)
    .map((c) => ({ label: c.name, value: c.id }))

  return (
    <FormPageLayout title="New Category" backHref="/products/categories" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Category Information">
        <FormField
          label="Name"
          required
          error={errors.name}
          registration={register("name")}
          placeholder="Category name"
        />
        <FormField
          label="Description"
          error={errors.description}
          registration={register("description")}
          placeholder="Category description"
        />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Parent Category"
            error={errors.parentCategoryId}
            options={categoryOptions}
            value=""
            onValueChange={(v) => setValue("parentCategoryId", v)}
            placeholder="No parent"
          />
          <FormField
            label="Sort Order"
            error={errors.sortOrder}
            registration={register("sortOrder")}
            type="number"
            placeholder="0"
          />
        </div>
      </FormSection>
      <FormActions backHref="/products/categories" loading={createMutation.isPending} submitLabel="Create Category" />
    </FormPageLayout>
  )
}
