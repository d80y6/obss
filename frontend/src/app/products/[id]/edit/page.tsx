"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { useProduct } from "@/api/hooks/useProduct"
import api from "@/services/api"
import type { ProductDto } from "@/api/generated"
import { queryKeys } from "@/lib/query-keys"
import { useQueryClient } from "@tanstack/react-query"
import { useEffect } from "react"

const productSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().min(1, "Description is required"),
  taxCategory: z.string().min(1, "Tax category is required"),
})

type ProductForm = z.input<typeof productSchema>

const taxCategories = [
  { label: "Standard", value: "Standard" },
  { label: "Reduced", value: "Reduced" },
  { label: "Exempt", value: "Exempt" },
  { label: "Zero Rated", value: "ZeroRated" },
]

export default function EditProductPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: product, isLoading, error } = useProduct(id)

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm<ProductForm>({
    resolver: zodResolver(productSchema),
  })

  useEffect(() => {
    if (product) {
      reset({
        name: product.name,
        description: product.description ?? "",
        taxCategory: product.taxCategory ?? "Standard",
      })
    }
  }, [product, reset])

  const updateMutation = useMutation({
    mutationFn: async (data: ProductForm) => {
      const res = await api.put<ProductDto>(`/api/v1/catalog/products/${id}`, {
        productId: id,
        name: data.name,
        description: data.description,
        categoryId: null,
        isShippable: false,
        taxable: true,
        taxCategory: data.taxCategory,
      })
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Product updated", description: "Product has been updated successfully." })
      queryClient.invalidateQueries({ queryKey: queryKeys.products.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.products.lists() })
      router.push(`/products/${id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to update product.", variant: "destructive" })
    },
  })

  const onSubmit = (data: ProductForm) => {
    updateMutation.mutate(data)
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !product) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load product" /></div>

  return (
    <FormPageLayout title="Edit Product" backHref={`/products/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Product Information">
        <FormField
          label="Name"
          required
          error={errors.name}
          registration={register("name")}
          placeholder="Product name"
        />
        <FormField
          label="Description"
          required
          error={errors.description}
          registration={register("description")}
          placeholder="Product description"
        />
        <FormSelectField
          label="Tax Category"
          required
          error={errors.taxCategory}
          options={taxCategories}
          value={product.taxCategory || "Standard"}
          onValueChange={(v) => setValue("taxCategory", v)}
          placeholder="Select tax category"
        />
      </FormSection>
      <FormActions backHref={`/products/${id}`} loading={updateMutation.isPending} submitLabel="Update Product" />
    </FormPageLayout>
  )
}
