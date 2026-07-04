"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { useCreateProductSpecification } from "@/api/hooks/useCreateProductSpecification"
import { toast } from "@/components/ui/toast"

const schema = z.object({
  name: z.string().min(1, "Name is required").max(200),
  description: z.string().max(2000).optional().or(z.literal("")),
  brand: z.string().max(200).optional().or(z.literal("")),
  version: z.string().max(100).optional().or(z.literal("")),
  productNumber: z.string().max(100).optional().or(z.literal("")),
})

type FormData = z.infer<typeof schema>

export default function NewProductSpecificationPage() {
  const router = useRouter()
  const createSpec = useCreateProductSpecification()

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: "",
      description: "",
      brand: "",
      version: "",
      productNumber: "",
    },
  })

  const onSubmit = async (data: FormData) => {
    try {
      const result = await createSpec.mutateAsync({
        tenantId: "default",
        name: data.name,
        description: data.description || null,
        brand: data.brand || null,
        version: data.version || null,
        productNumber: data.productNumber || null,
      })
      toast({ title: "Created", description: "Product specification created." })
      router.push(`/product-specifications/${result.id}`)
    } catch {
      toast({ title: "Error", description: "Failed to create specification.", variant: "destructive" })
    }
  }

  return (
    <FormPageLayout title="New Product Specification" backHref="/product-specifications" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Basic Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="e.g. Gigabit Fiber" />
          <FormField label="Product Number" error={errors.productNumber} registration={register("productNumber")} placeholder="e.g. SKU-001" />
          <FormField label="Brand" error={errors.brand} registration={register("brand")} placeholder="e.g. Acme Corp" />
          <FormField label="Version" error={errors.version} registration={register("version")} placeholder="e.g. 1.0" />
        </div>
        <div className="mt-4">
          <FormField label="Description" error={errors.description} registration={register("description")} placeholder="Optional description" />
        </div>
      </FormSection>
      <FormActions backHref="/product-specifications" loading={isSubmitting} submitLabel="Create Specification" />
    </FormPageLayout>
  )
}
