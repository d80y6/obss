"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { useCreateProduct } from "@/api/hooks/useCreateProduct"
import type { CreateProductCommand } from "@/api/generated"

const productSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().min(1, "Description is required"),
  productType: z.string().min(1, "Product type is required"),
  isShippable: z.boolean().default(false),
  taxable: z.boolean().default(true),
  taxCategory: z.string().min(1, "Tax category is required"),
  categoryId: z.string().optional(),
})

type ProductForm = z.infer<typeof productSchema>

const productTypes = [
  { label: "Physical", value: "Physical" },
  { label: "Digital", value: "Digital" },
  { label: "Service", value: "Service" },
]

const taxCategories = [
  { label: "Standard", value: "Standard" },
  { label: "Reduced", value: "Reduced" },
  { label: "Exempt", value: "Exempt" },
  { label: "Zero Rated", value: "ZeroRated" },
]

export default function NewProductPage() {
  const router = useRouter()
  const createProduct = useCreateProduct()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<ProductForm>({
    resolver: zodResolver(productSchema),
    defaultValues: { isShippable: false, taxable: true },
  })

  const onSubmit = (data: ProductForm) => {
    const payload: CreateProductCommand = {
      name: data.name,
      description: data.description,
      productType: data.productType as CreateProductCommand["productType"],
      isShippable: data.isShippable,
      taxable: data.taxable,
      taxCategory: data.taxCategory,
      categoryId: data.categoryId || null,
      specifications: null,
    }

    createProduct.mutate(payload, {
      onSuccess: (product) => {
        toast({ title: "Product created", description: `${product.name} has been created successfully.` })
        router.push("/products")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create product.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="New Product" backHref="/products" onSubmit={handleSubmit(onSubmit)}>
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
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Product Type"
            required
            error={errors.productType}
            options={productTypes}
            value=""
            onValueChange={(v) => setValue("productType", v)}
            placeholder="Select type"
          />
          <FormSelectField
            label="Tax Category"
            required
            error={errors.taxCategory}
            options={taxCategories}
            value=""
            onValueChange={(v) => setValue("taxCategory", v)}
            placeholder="Select tax category"
          />
        </div>
      </FormSection>
      <FormActions backHref="/products" loading={createProduct.isPending} submitLabel="Create Product" />
    </FormPageLayout>
  )
}
