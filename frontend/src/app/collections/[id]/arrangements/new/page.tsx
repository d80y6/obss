"use client"
import { useParams, useRouter } from "next/navigation"
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
  installmentCount: z.string().min(1, "Installment count is required"),
  amountPerInstallment: z.string().min(1, "Amount per installment is required"),
  frequency: z.string().min(1, "Frequency is required"),
  startDate: z.string().min(1, "Start date is required"),
})

type FormData = z.infer<typeof schema>

const frequencyOptions = [
  { label: "Weekly", value: "WEEKLY" },
  { label: "Monthly", value: "MONTHLY" },
]

export default function NewArrangementPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post(`/api/v1/collections/cases/${id}/arrangements`, {
        installmentCount: parseInt(data.installmentCount, 10),
        amountPerInstallment: parseFloat(data.amountPerInstallment),
        frequency: data.frequency,
        startDate: data.startDate,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.arrangements(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(id) })
      toast({ title: "Payment arrangement created" })
      router.push(`/collections/${id}`)
    },
    onError: () => {
      toast({ title: "Failed to create arrangement", variant: "destructive" })
    },
  })

  return (
    <FormPageLayout title="New Payment Arrangement" backHref={`/collections/${id}`} onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Arrangement Details">
        <FormField label="Installment Count" error={errors.installmentCount} registration={register("installmentCount")} type="text" placeholder="e.g. 6" required />
        <FormField label="Amount Per Installment" error={errors.amountPerInstallment} registration={register("amountPerInstallment")} type="text" placeholder="0.00" required />
        <FormSelectField label="Frequency" error={errors.frequency} options={frequencyOptions} value={watch("frequency")} onValueChange={(v) => setValue("frequency", v)} required placeholder="Select frequency" />
        <FormField label="Start Date" error={errors.startDate} registration={register("startDate")} type="date" required />
      </FormSection>
      <FormActions backHref={`/collections/${id}`} loading={mutation.isPending} submitLabel="Create Arrangement" />
    </FormPageLayout>
  )
}
