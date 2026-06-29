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
import { useCreateApiKey, usePartners } from "@/api/hooks/use-api-gateway"

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  partnerId: z.string().min(1, "Partner is required"),
  expiresAt: z.string().optional(),
})

type FormData = z.infer<typeof schema>

export default function NewApiKeyPage() {
  const router = useRouter()
  const { data: partners } = usePartners()
  const createMutation = useCreateApiKey()

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const partnerOptions = (partners ?? []).map((p) => ({
    label: p.name,
    value: p.id,
  }))

  const onSubmit = (data: FormData) => {
    createMutation.mutate({
      name: data.name,
      partnerId: data.partnerId,
      expiresAt: data.expiresAt || null,
      permissions: [],
      allowedIPs: [],
      rateLimitPerMinute: 100,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any, {
      onSuccess: () => {
        toast({ title: "API key created" })
        router.push("/api-gateway/api-keys")
      },
      onError: () => toast({ title: "Error", description: "Failed to create API key.", variant: "destructive" }),
    })
  }

  return (
    <FormPageLayout title="New API Key" backHref="/api-gateway/api-keys" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="API Key Information">
        <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="Key name" />
        <FormSelectField label="Partner" required error={errors.partnerId} options={partnerOptions} value="" onValueChange={(v) => setValue("partnerId", v)} placeholder="Select partner" />
        <FormField label="Expiry Date" error={errors.expiresAt} registration={register("expiresAt")} type="date" />
      </FormSection>
      <FormActions backHref="/api-gateway/api-keys" loading={createMutation.isPending} submitLabel="Create API Key" />
    </FormPageLayout>
  )
}
