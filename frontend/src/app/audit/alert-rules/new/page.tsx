"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  eventType: z.string().min(1, "Event type is required"),
  severity: z.string().min(1, "Severity is required"),
  threshold: z.string().min(1, "Threshold is required"),
  enabled: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const eventTypeOptions = [
  { label: "All Events", value: "ALL" },
  { label: "Create", value: "CREATE" },
  { label: "Update", value: "UPDATE" },
  { label: "Delete", value: "DELETE" },
  { label: "Login", value: "LOGIN" },
  { label: "Failed Login", value: "FAILED_LOGIN" },
]

const severityOptions = [
  { label: "Info", value: "INFO" },
  { label: "Warning", value: "WARNING" },
  { label: "Critical", value: "CRITICAL" },
]

export default function NewAuditAlertRulePage() {
  const router = useRouter()
  const queryClient = useQueryClient()

  const createMutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post("/api/v1/audit/alert-rules", {
        name: data.name,
        alertType: data.eventType,
        severity: data.severity,
        threshold: Number(data.threshold),
        isActive: data.enabled === "true",
        description: "",
        windowMinutes: 60,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["audit-alert-rules"] })
      toast({ title: "Rule created", description: "Alert rule has been created." })
      router.push("/audit/alert-rules")
    },
    onError: () => toast({ title: "Error", description: "Failed to create rule.", variant: "destructive" }),
  })

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = (data: FormData) => createMutation.mutate(data)

  return (
    <FormPageLayout title="New Alert Rule" backHref="/audit/alert-rules" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Rule Configuration">
        <FormField label="Name" required error={errors.name} registration={register("name")} placeholder="Rule name" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField label="Event Type" required error={errors.eventType} options={eventTypeOptions} value="" onValueChange={(v) => setValue("eventType", v)} placeholder="Select event type" />
          <FormSelectField label="Severity" required error={errors.severity} options={severityOptions} value="" onValueChange={(v) => setValue("severity", v)} placeholder="Select severity" />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField label="Threshold" required error={errors.threshold} registration={register("threshold")} type="number" placeholder="5" />
          <FormSelectField label="Enabled" options={[{ label: "Yes", value: "true" }, { label: "No", value: "false" }]} value="" onValueChange={(v) => setValue("enabled", v)} placeholder="Enabled?" />
        </div>
      </FormSection>
      <FormActions backHref="/audit/alert-rules" loading={createMutation.isPending} submitLabel="Create Rule" />
    </FormPageLayout>
  )
}
