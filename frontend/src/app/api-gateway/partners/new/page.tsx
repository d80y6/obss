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
import { useCreatePartner } from "@/api/hooks/use-api-gateway"

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  contactEmail: z.string().email("Invalid email").optional().or(z.literal("")),
  contactPhone: z.string().optional(),
  status: z.string().min(1, "Status is required"),
})

type FormData = z.infer<typeof schema>

const statusOptions = [
  { label: "Active", value: "ACTIVE" },
  { label: "Inactive", value: "INACTIVE" },
  { label: "Suspended", value: "SUSPENDED" },
]

export default function NewPartnerPage() {
  const router = useRouter()
  const createMutation = useCreatePartner()

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = (data: FormData) => {
    createMutation.mutate({
      name: data.name,
      contactEmail: data.contactEmail || "",
      contactName: data.name,
      allowedIPs: [],
      slaLevel: "Standard",
      maxRequestsPerDay: 10000,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any, {
      onSuccess: () => {
        toast({ title: "Partner created", description: "Partner has been created." })
        router.push("/api-gateway/partners")
      },
      onError: () => toast({ title: "Error", description: "Failed to create partner.", variant: "destructive" }),
    })
  }

  return (
    <FormPageLayout title="New Partner" backHref="/api-gateway/partners" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Partner Information">
        <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="Partner name" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormField label="Contact Email" error={errors.contactEmail} registration={register("contactEmail")} type="email" placeholder="partner@example.com" />
          <FormField label="Contact Phone" error={errors.contactPhone} registration={register("contactPhone")} placeholder="+1-555-123-4567" />
        </div>
        <FormSelectField label="Status" required error={errors.status} options={statusOptions} value="" onValueChange={(v) => setValue("status", v)} placeholder="Select status" />
      </FormSection>
      <FormActions backHref="/api-gateway/partners" loading={createMutation.isPending} submitLabel="Create Partner" />
    </FormPageLayout>
  )
}
