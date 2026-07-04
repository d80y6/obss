"use client"

import { useEffect } from "react"
import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { useProductSpecification } from "@/api/hooks/useProductSpecification"
import { useUpdateProductSpecification } from "@/api/hooks/useUpdateProductSpecification"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"

const schema = z.object({
  name: z.string().min(1, "Name is required").max(200),
  description: z.string().max(2000).optional().or(z.literal("")),
  brand: z.string().max(200).optional().or(z.literal("")),
  version: z.string().max(100).optional().or(z.literal("")),
  productNumber: z.string().max(100).optional().or(z.literal("")),
})

type FormData = z.infer<typeof schema>

export default function EditProductSpecificationPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: spec, isLoading } = useProductSpecification(id)
  const updateSpec = useUpdateProductSpecification(id)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  useEffect(() => {
    if (spec) {
      reset({
        name: spec.name,
        description: spec.description ?? "",
        brand: spec.brand ?? "",
        version: spec.version ?? "",
        productNumber: spec.productNumber ?? "",
      })
    }
  }, [spec, reset])

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>

  const onSubmit = async (data: FormData) => {
    try {
      await updateSpec.mutateAsync({
        name: data.name,
        description: data.description || null,
        brand: data.brand || null,
        version: data.version || null,
        productNumber: data.productNumber || null,
      })
      toast({ title: "Updated", description: "Product specification updated." })
      router.push(`/product-specifications/${id}`)
    } catch {
      toast({ title: "Error", description: "Failed to update specification.", variant: "destructive" })
    }
  }

  return (
    <FormPageLayout title={`Edit: ${spec?.name ?? "Specification"}`} backHref={`/product-specifications/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Basic Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField label="Name" required error={errors.name} registration={register("name")} />
          <FormField label="Product Number" error={errors.productNumber} registration={register("productNumber")} />
          <FormField label="Brand" error={errors.brand} registration={register("brand")} />
          <FormField label="Version" error={errors.version} registration={register("version")} />
        </div>
        <div className="mt-4">
          <FormField label="Description" error={errors.description} registration={register("description")} />
        </div>
      </FormSection>
      <FormActions backHref={`/product-specifications/${id}`} loading={isSubmitting} submitLabel="Save Changes" />
    </FormPageLayout>
  )
}
