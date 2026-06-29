"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { TenantDto } from "@/types/api"

const tenantSchema = z.object({
  name: z.string().min(1, "Name is required"),
  slug: z.string().min(1, "Slug is required").regex(/^[a-z0-9-]+$/, "Slug must be lowercase alphanumeric with dashes"),
  description: z.string().optional(),
})

type TenantForm = z.infer<typeof tenantSchema>

export default function NewTenantPage() {
  const router = useRouter()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TenantForm>({
    resolver: zodResolver(tenantSchema),
  })

  const createMutation = useMutation({
    mutationFn: async (data: TenantForm) => {
      const payload = {
        name: data.name,
        slug: data.slug,
        adminUsername: "admin",
        adminEmail: `admin@${data.slug}.com`,
        adminFirstName: data.name,
        adminLastName: "Admin",
      }
      const res = await api.post<TenantDto>("/api/v1/iam/tenants", payload)
      return res.data
    },
    onSuccess: (data) => {
      toast({ title: "Tenant created", description: `${data.name} has been created successfully.` })
      router.push(`/admin/tenants/${data.id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create tenant.", variant: "destructive" })
    },
  })

  const onSubmit = (data: TenantForm) => {
    createMutation.mutate(data)
  }

  return (
    <FormPageLayout title="New Tenant" backHref="/admin/tenants" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Tenant Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Name"
            required
            error={errors.name}
            registration={register("name")}
            placeholder="Tenant name"
          />
          <FormField
            label="Slug"
            required
            error={errors.slug}
            registration={register("slug")}
            placeholder="my-tenant"
          />
        </div>
        <FormField
          label="Description"
          error={errors.description}
          registration={register("description")}
          placeholder="Tenant description"
        />
      </FormSection>
      <FormActions backHref="/admin/tenants" loading={createMutation.isPending} submitLabel="Create Tenant" />
    </FormPageLayout>
  )
}
