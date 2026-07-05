"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import api from "@/services/api"
import type { CategoryDto } from "@/api/generated"
import { useEffect } from "react"

const categorySchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  parentCategoryId: z.string().optional(),
})

type CategoryForm = z.infer<typeof categorySchema>

export default function EditCategoryPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: category, isLoading, error } = useQuery({
    queryKey: ["product-categories", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/categories/${id}`)
      return res.data as CategoryDto
    },
    enabled: !!id,
  })

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
    reset,
    setValue,
    formState: { errors },
  } = useForm<CategoryForm>({
    resolver: zodResolver(categorySchema),
  })

  useEffect(() => {
    if (category) {
      reset({
        name: category.name,
        description: category.description ?? "",
        parentCategoryId: category.parentCategoryId || "",
      })
    }
  }, [category, reset])

  const updateMutation = useMutation({
    mutationFn: async (data: CategoryForm) => {
      const res = await api.put(`/api/v1/catalog/categories/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Category updated", description: "Category has been updated successfully." })
      router.push(`/products/categories/${id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to update category.", variant: "destructive" })
    },
  })

  const onSubmit = (data: CategoryForm) => {
    updateMutation.mutate(data)
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !category) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load category" /></div>

  const categoryOptions = (categories ?? [])
    .filter((c) => c.id !== id && !c.parentCategoryId)
    .map((c) => ({ label: c.name, value: c.id }))

  return (
    <FormPageLayout title="Edit Category" backHref={`/products/categories/${id}`} onSubmit={handleSubmit(onSubmit)}>
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
        <FormSelectField
          label="Parent Category"
          error={errors.parentCategoryId}
          options={categoryOptions}
          value=""
          onValueChange={(v) => setValue("parentCategoryId", v)}
          placeholder="No parent"
        />
      </FormSection>
      <FormActions backHref={`/products/categories/${id}`} loading={updateMutation.isPending} submitLabel="Update Category" />
    </FormPageLayout>
  )
}
