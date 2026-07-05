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
import { useService, useUpdateService } from "@/api/hooks/use-service-inventory"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"

const schema = z.object({
  configuration: z.string().optional().or(z.literal("")),
  location: z.string().optional().or(z.literal("")),
})

type FormData = z.infer<typeof schema>

export default function EditServicePage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: service, isLoading } = useService(id)
  const updateService = useUpdateService(id)

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  useEffect(() => {
    if (service) {
      reset({
        configuration: service.configuration ?? "",
        location: service.location ?? "",
      })
    }
  }, [service, reset])

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>

  const onSubmit = async (data: FormData) => {
    try {
      await updateService.mutateAsync({
        configuration: data.configuration || undefined,
        location: data.location || undefined,
      })
      toast({ title: "Updated", description: "Service updated." })
      router.push(`/service-inventory/${id}`)
    } catch {
      toast({ title: "Error", description: "Failed to update service.", variant: "destructive" })
    }
  }

  return (
    <FormPageLayout title={`Edit Service: ${service?.serviceIdentifier ?? ""}`} backHref={`/service-inventory/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Service Configuration">
        <FormField label="Configuration (JSON)" registration={register("configuration")} error={errors.configuration} placeholder='{"key": "value"}' />
        <FormField label="Location" registration={register("location")} error={errors.location} placeholder="e.g. Data Center A" />
      </FormSection>
      <FormActions backHref={`/service-inventory/${id}`} loading={isSubmitting} submitLabel="Save Changes" />
    </FormPageLayout>
  )
}
