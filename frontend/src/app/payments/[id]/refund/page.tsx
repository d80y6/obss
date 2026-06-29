"use client"
import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"

const schema = z.object({
  reason: z.string().min(1, "Reason is required"),
  amount: z.string().min(1, "Amount is required"),
})

type FormData = z.infer<typeof schema>

export default function RefundPaymentPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post(`/api/v1/payments/payments/${id}/refund`, {
        reason: data.reason,
        amount: parseFloat(data.amount),
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.refunds.all })
      toast({ title: "Refund processed" })
      router.push(`/payments/${id}`)
    },
    onError: () => {
      toast({ title: "Failed to process refund", variant: "destructive" })
    },
  })

  return (
    <FormPageLayout title="Process Refund" backHref={`/payments/${id}`} onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Refund Details">
        <FormField label="Amount" error={errors.amount} registration={register("amount")} type="text" placeholder="0.00" required />
        <FormField label="Reason" error={errors.reason} registration={register("reason")} placeholder="e.g. Customer request" required />
      </FormSection>
      <FormActions backHref={`/payments/${id}`} loading={mutation.isPending} submitLabel="Process Refund" />
    </FormPageLayout>
  )
}
