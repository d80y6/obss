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
import { queryKeys } from "@/lib/query-keys"

const schema = z.object({
  type: z.string().min(1, "Job type is required"),
  params: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const jobTypeOptions = [
  { label: "Generate Bills", value: "GENERATE_BILLS" },
  { label: "Process Payments", value: "PROCESS_PAYMENTS" },
  { label: "Send Invoices", value: "SEND_INVOICES" },
  { label: "Apply Adjustments", value: "APPLY_ADJUSTMENTS" },
]

export default function NewBillingJobPage() {
  const router = useRouter()
  const queryClient = useQueryClient()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post(`/api/v1/billing/jobs?jobType=${encodeURIComponent(data.type)}`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.jobs.all })
      toast({ title: "Job created" })
      router.push("/billing/jobs")
    },
    onError: () => {
      toast({ title: "Failed to create job", variant: "destructive" })
    },
  })

  return (
    <FormPageLayout title="New Billing Job" backHref="/billing/jobs" onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Job Details">
        <FormSelectField label="Job Type" error={errors.type} options={jobTypeOptions} value={watch("type")} onValueChange={(v) => setValue("type", v)} required placeholder="Select job type" />
        <FormField label="Parameters (JSON)" error={errors.params} registration={register("params")} placeholder='e.g. {"cycleId": "..."}' />
      </FormSection>
      <FormActions backHref="/billing/jobs" loading={mutation.isPending} />
    </FormPageLayout>
  )
}
