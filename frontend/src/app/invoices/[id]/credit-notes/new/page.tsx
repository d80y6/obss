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

export default function NewCreditNotePage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post(`/api/v1/invoices/invoices/${id}/credit-note`, {
        reason: data.reason,
        amount: parseFloat(data.amount),
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.creditNotes.all })
      toast({ title: "Credit note issued" })
      router.push(`/invoices/${id}`)
    },
    onError: () => {
      toast({ title: "Failed to issue credit note", variant: "destructive" })
    },
  })

  return (
    <FormPageLayout title="Issue Credit Note" backHref={`/invoices/${id}`} onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Credit Note Details">
        <FormField label="Reason" error={errors.reason} registration={register("reason")} placeholder="e.g. Service credit, billing error" required />
        <FormField label="Amount" error={errors.amount} registration={register("amount")} type="text" placeholder="0.00" required />
      </FormSection>
      <FormActions backHref={`/invoices/${id}`} loading={mutation.isPending} submitLabel="Issue Credit Note" />
    </FormPageLayout>
  )
}
