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
import { useCreateBillingCycle } from "@/api/hooks/useCreateBillingCycle"

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
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useCreateBillingCycle()

  return (
    <FormPageLayout title="New Billing Cycle" backHref="/billing/cycles" onSubmit={handleSubmit((data) => mutation.mutate({
      customerId: "00000000-0000-0000-0000-000000000000",
      billingPeriod: data.frequency,
      lastBillingDate: data.periodStart,
      nextBillingDate: data.periodEnd,
    }, {
      onSuccess: () => {
        toast({ title: "Billing cycle created" })
        router.push("/billing/cycles")
      },
      onError: () => {
        toast({ title: "Failed to create billing cycle", variant: "destructive" })
      },
    }))}>
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
