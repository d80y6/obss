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
import { useSubmitUsage } from "@/api/hooks/use-rating"
import type { SubmitUsageCommand } from "@/api/generated"

const usageSchema = z.object({
  subscriptionId: z.string().uuid("Must be a valid UUID"),
  serviceId: z.string().uuid("Must be a valid UUID"),
  recordType: z.string().min(1, "Record type is required"),
  usageType: z.string().min(1, "Usage type is required"),
  startTime: z.string().min(1, "Start time is required"),
  endTime: z.string().min(1, "End time is required"),
  duration: z.coerce.number().min(0, "Duration must be positive"),
  volume: z.coerce.number().min(0, "Volume must be positive"),
  sourceIdentifier: z.string().optional(),
  destinationIdentifier: z.string().optional(),
  currency: z.string().default("USD"),
  rateImmediately: z.boolean().default(false),
})

type UsageForm = z.infer<typeof usageSchema>

const recordTypes = [
  { label: "Voice", value: "VOICE" },
  { label: "Data", value: "DATA" },
  { label: "SMS", value: "SMS" },
  { label: "MMS", value: "MMS" },
]

const usageTypes = [
  { label: "Outbound", value: "OUTBOUND" },
  { label: "Inbound", value: "INBOUND" },
  { label: "Roaming", value: "ROAMING" },
  { label: "Local", value: "LOCAL" },
]

export default function NewUsageRecordPage() {
  const router = useRouter()
  const createMutation = useSubmitUsage()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<UsageForm>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(usageSchema) as any,
    defaultValues: {
      currency: "USD",
      rateImmediately: false,
    },
  })

  const onSubmit = (data: UsageForm) => {
    const command: SubmitUsageCommand = {
      subscriptionId: data.subscriptionId,
      serviceId: data.serviceId,
      recordType: data.recordType,
      usageType: data.usageType,
      startTime: data.startTime,
      endTime: data.endTime,
      duration: data.duration,
      volume: data.volume,
      sourceIdentifier: data.sourceIdentifier ?? "",
      destinationIdentifier: data.destinationIdentifier ?? "",
      currency: data.currency,
      rateImmediately: data.rateImmediately,
    }
    createMutation.mutate(command, {
      onSuccess: () => {
        toast({ title: "Usage submitted", description: "Usage record has been submitted successfully." })
        router.push("/rating/usage")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to submit usage record.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="Submit Usage Record" backHref="/rating/usage" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Usage Details">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Subscription ID"
            required
            error={errors.subscriptionId}
            registration={register("subscriptionId")}
            placeholder="00000000-0000-0000-0000-000000000000"
          />
          <FormField
            label="Service ID"
            required
            error={errors.serviceId}
            registration={register("serviceId")}
            placeholder="00000000-0000-0000-0000-000000000000"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Record Type"
            required
            error={errors.recordType}
            options={recordTypes}
            value=""
            onValueChange={(v) => setValue("recordType", v)}
            placeholder="Select record type"
          />
          <FormSelectField
            label="Usage Type"
            required
            error={errors.usageType}
            options={usageTypes}
            value=""
            onValueChange={(v) => setValue("usageType", v)}
            placeholder="Select usage type"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Start Time"
            required
            type="datetime-local"
            error={errors.startTime}
            registration={register("startTime")}
          />
          <FormField
            label="End Time"
            required
            type="datetime-local"
            error={errors.endTime}
            registration={register("endTime")}
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Duration (seconds)"
            required
            type="number"
            error={errors.duration}
            registration={register("duration")}
            placeholder="0"
          />
          <FormField
            label="Volume"
            required
            type="number"
            error={errors.volume}
            registration={register("volume")}
            placeholder="0"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Source Identifier"
            error={errors.sourceIdentifier}
            registration={register("sourceIdentifier")}
            placeholder="Source phone/address"
          />
          <FormField
            label="Destination Identifier"
            error={errors.destinationIdentifier}
            registration={register("destinationIdentifier")}
            placeholder="Destination phone/address"
          />
        </div>
      </FormSection>
      <FormActions backHref="/rating/usage" loading={createMutation.isPending} submitLabel="Submit Usage" />
    </FormPageLayout>
  )
}
