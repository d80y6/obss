"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useMutation } from "@tanstack/react-query"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { useQueryClient } from "@tanstack/react-query"

const catalogSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  catalogType: z.string().min(1, "Catalog type is required"),
  version: z.string().optional(),
  validFrom: z.string().optional(),
  validTo: z.string().optional(),
})

type CatalogForm = z.input<typeof catalogSchema>

const catalogTypes = [
  { label: "Product Catalog", value: "ProductCatalog" },
  { label: "Service Catalog", value: "ServiceCatalog" },
  { label: "Resource Catalog", value: "ResourceCatalog" },
]

export default function NewCatalogPage() {
  const router = useRouter()
  const queryClient = useQueryClient()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<CatalogForm>({
    resolver: zodResolver(catalogSchema),
    defaultValues: { version: "1" },
  })

  const createMutation = useMutation({
    mutationFn: async (data: CatalogForm) => {
      const res = await api.post("/api/v1/catalog/catalogs", {
        tenantId: "default",
        name: data.name,
        description: data.description || null,
        catalogType: data.catalogType,
        version: Number(data.version) || 1,
        validFrom: data.validFrom ? new Date(data.validFrom).toISOString() : null,
        validTo: data.validTo ? new Date(data.validTo).toISOString() : null,
      })
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Catalog created", description: "Catalog has been created successfully." })
      queryClient.invalidateQueries({ queryKey: ["catalogs"] })
      router.push("/catalogs")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create catalog.", variant: "destructive" })
    },
  })

  const onSubmit = (data: CatalogForm) => {
    createMutation.mutate(data)
  }

  return (
    <FormPageLayout title="New Catalog" backHref="/catalogs" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Catalog Information">
        <FormField
          label="Name"
          required
          error={errors.name}
          registration={register("name")}
          placeholder="Catalog name"
        />
        <FormField
          label="Description"
          error={errors.description}
          registration={register("description")}
          placeholder="Catalog description"
        />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Catalog Type"
            required
            error={errors.catalogType}
            options={catalogTypes}
            value=""
            onValueChange={(v) => setValue("catalogType", v)}
            placeholder="Select type"
          />
          <FormField
            label="Version"
            error={errors.version}
            registration={register("version")}
            type="number"
            placeholder="1"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Valid From"
            error={errors.validFrom}
            registration={register("validFrom")}
            type="date"
          />
          <FormField
            label="Valid To"
            error={errors.validTo}
            registration={register("validTo")}
            type="date"
          />
        </div>
      </FormSection>
      <FormActions backHref="/catalogs" loading={createMutation.isPending} submitLabel="Create Catalog" />
    </FormPageLayout>
  )
}
