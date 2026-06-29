"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { useReportDefinitions } from "@/api/hooks/use-reporting"

const schema = z.object({
  definitionId: z.string().min(1, "Report definition is required"),
  cron: z.string().min(1, "Cron expression is required"),
  format: z.string().min(1, "Format is required"),
  recipients: z.string().min(1, "Recipients is required"),
})

type FormData = z.infer<typeof schema>

const formatOptions = [
  { label: "PDF", value: "PDF" },
  { label: "CSV", value: "CSV" },
  { label: "Excel", value: "EXCEL" },
]

export default function NewScheduledReportPage() {
  const router = useRouter()
  const { data: definitions } = useReportDefinitions()

  const createMutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post("/api/v1/reporting/schedule", {
        ...data,
        recipients: data.recipients.split(",").map((r: string) => r.trim()),
      })
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Scheduled report created" })
      router.push("/reporting/scheduled")
    },
    onError: () => toast({ title: "Error", description: "Failed to create scheduled report.", variant: "destructive" }),
  })

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const definitionOptions = (definitions ?? []).map((d) => ({
    label: d.name,
    value: d.id,
  }))

  const onSubmit = (data: FormData) => createMutation.mutate(data)

  return (
    <FormPageLayout title="New Scheduled Report" backHref="/reporting/scheduled" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Schedule Information">
        <FormSelectField label="Report Definition" required error={errors.definitionId} options={definitionOptions} value="" onValueChange={(v) => setValue("definitionId", v)} placeholder="Select report" />
        <FormField label="Cron Expression" required error={errors.cron} registration={register("cron")} placeholder="0 8 * * 1" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField label="Format" required error={errors.format} options={formatOptions} value="" onValueChange={(v) => setValue("format", v)} placeholder="Select format" />
          <FormField label="Recipients (comma-separated emails)" required error={errors.recipients} registration={register("recipients")} placeholder="admin@example.com, team@example.com" />
        </div>
      </FormSection>
      <FormActions backHref="/reporting/scheduled" loading={createMutation.isPending} submitLabel="Create Schedule" />
    </FormPageLayout>
  )
}
