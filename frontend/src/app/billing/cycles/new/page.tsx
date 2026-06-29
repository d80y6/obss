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
  name: z.string().min(1, "Name is required"),
  billingDay: z.string().min(1, "Billing day is required"),
  frequency: z.string().min(1, "Frequency is required"),
  periodStart: z.string().min(1, "Period start is required"),
  periodEnd: z.string().min(1, "Period end is required"),
})

type FormData = z.infer<typeof schema>

const frequencyOptions = [
  { label: "Monthly", value: "MONTHLY" },
  { label: "Quarterly", value: "QUARTERLY" },
  { label: "Annual", value: "ANNUAL" },
]

export default function NewBillingCyclePage() {
  const router = useRouter()
  const queryClient = useQueryClient()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post("/api/v1/billing/cycles", {
        customerId: "00000000-0000-0000-0000-000000000000",
        billingPeriod: data.frequency,
        lastBillingDate: data.periodStart,
        nextBillingDate: data.periodEnd,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.cycles.all })
      toast({ title: "Billing cycle created" })
      router.push("/billing/cycles")
    },
    onError: () => {
      toast({ title: "Failed to create billing cycle", variant: "destructive" })
    },
  })

  return (
    <FormPageLayout title="New Billing Cycle" backHref="/billing/cycles" onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Cycle Details">
        <FormField label="Name" error={errors.name} registration={register("name")} placeholder="e.g. Monthly Billing" required />
        <FormField label="Billing Day" error={errors.billingDay} registration={register("billingDay")} type="text" placeholder="1-31" required />
        <FormSelectField label="Frequency" error={errors.frequency} options={frequencyOptions} value={watch("frequency")} onValueChange={(v) => setValue("frequency", v)} required placeholder="Select frequency" />
        <FormField label="Period Start" error={errors.periodStart} registration={register("periodStart")} type="date" required />
        <FormField label="Period End" error={errors.periodEnd} registration={register("periodEnd")} type="date" required />
      </FormSection>
      <FormActions backHref="/billing/cycles" loading={mutation.isPending} />
    </FormPageLayout>
  )
}
