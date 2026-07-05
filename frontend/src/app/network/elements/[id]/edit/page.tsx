"use client"

import { useParams, useRouter } from "next/navigation"
import { useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { useNetworkElement } from "@/api/hooks/useNetworkElement"
import { useUpdateNetworkElement } from "@/api/hooks/useUpdateNetworkElement"
import type { UpdateNetworkElementCommand } from "@/api/generated"

const elementSchema = z.object({
  name: z.string().min(1, "Name is required"),
  hostname: z.string().min(1, "Hostname is required"),
  vendor: z.string().min(1, "Vendor is required"),
  model: z.string().min(1, "Model is required"),
  location: z.string().optional(),
})

type ElementFormData = z.input<typeof elementSchema>

export default function EditNetworkElementPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: element, isLoading, error } = useNetworkElement(id)
  const updateNetworkElement = useUpdateNetworkElement(id)

  const { register, handleSubmit, reset, formState: { errors } } = useForm<ElementFormData>({
    resolver: zodResolver(elementSchema),
  })

  useEffect(() => {
    if (element) {
      reset({
        name: element.name,
        hostname: element.hostname,
        vendor: element.vendor,
        model: element.model,
        location: element.location ?? undefined,
      })
    }
  }, [element, reset])

  const onSubmit = (data: ElementFormData) => {
    updateNetworkElement.mutate({
      id,
      name: data.name,
      hostname: data.hostname,
      vendor: data.vendor,
      model: data.model,
      softwareVersion: null,
      serialNumber: null,
      location: data.location || null,
      managementIP: null,
      snmpCommunity: null,
      isManaged: true,
    } as UpdateNetworkElementCommand, {
      onSuccess: () => {
        toast({ title: "Element updated", description: "Network element has been updated successfully." })
        router.push(`/network/elements/${id}`)
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to update network element.", variant: "destructive" })
      },
    })
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !element) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load network element" /></div>

  return (
    <FormPageLayout title="Edit Network Element" backHref={`/network/elements/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Element Details">
        <FormField label="Name" required registration={register("name")} error={errors.name} placeholder="Element name" />
        <FormField label="Hostname" required registration={register("hostname")} error={errors.hostname} placeholder="e.g. olt-01.example.com" />
        <FormField label="Vendor" required registration={register("vendor")} error={errors.vendor} placeholder="e.g. Cisco, Huawei" />
        <FormField label="Model" required registration={register("model")} error={errors.model} placeholder="e.g. ASR-9000" />
        <FormField label="Location" registration={register("location")} error={errors.location} placeholder="e.g. Data Center A" />
      </FormSection>
      <FormActions backHref={`/network/elements/${id}`} loading={updateNetworkElement.isPending} submitLabel="Update Network Element" />
    </FormPageLayout>
  )
}
