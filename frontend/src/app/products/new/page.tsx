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

const productSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  productType: z.string().min(1, "Product type is required"),
  taxCategory: z.string().min(1, "Tax category is required"),
  categoryId: z.string().optional(),
  isShippable: z.boolean().optional(),
  taxable: z.boolean().optional(),
})

type ProductForm = z.input<typeof productSchema>

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
    createProduct.mutate({
      name: data.name,
      description: data.description || null,
      productType: data.productType,
      isShippable: data.isShippable ?? false,
      taxable: data.taxable ?? true,
      taxCategory: data.taxCategory,
      categoryId: data.categoryId || null,
      specifications: null,
    } as Parameters<typeof createProduct.mutate>[0], {
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
      <FormSection title="Product Settings">
        <div className="flex items-center gap-6">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              {...register("isShippable")}
              className="h-4 w-4 rounded border-gray-300"
            />
            <span className="text-sm font-medium">Shippable</span>
          </label>
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              {...register("taxable")}
              className="h-4 w-4 rounded border-gray-300"
            />
            <span className="text-sm font-medium">Taxable</span>
          </label>
        </div>
      </FormSection>
      <FormActions backHref="/products" loading={createProduct.isPending} submitLabel="Create Product" />
    </FormPageLayout>
  )
}
