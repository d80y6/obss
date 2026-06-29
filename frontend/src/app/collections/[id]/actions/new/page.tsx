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
  actionType: z.string().min(1, "Action type is required"),
  notes: z.string().min(1, "Notes are required"),
  scheduledDate: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const actionTypeOptions = [
  { label: "Reminder", value: "REMINDER" },
  { label: "Phone Call", value: "CALL" },
  { label: "Letter", value: "LETTER" },
  { label: "Email", value: "EMAIL" },
]

export default function NewActionPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post(`/api/v1/collections/cases/${id}/actions`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.actions(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(id) })
      toast({ title: "Action added" })
      router.push(`/collections/${id}`)
    },
    onError: () => {
      toast({ title: "Failed to add action", variant: "destructive" })
    },
  })

  return (
    <FormPageLayout title="New Collection Action" backHref={`/collections/${id}`} onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Action Details">
        <FormSelectField label="Action Type" error={errors.actionType} options={actionTypeOptions} value={watch("actionType")} onValueChange={(v) => setValue("actionType", v)} required placeholder="Select action type" />
        <FormField label="Notes" error={errors.notes} registration={register("notes")} placeholder="Details about this action" required />
        <FormField label="Scheduled Date" error={errors.scheduledDate} registration={register("scheduledDate")} type="date" />
      </FormSection>
      <FormActions backHref={`/collections/${id}`} loading={mutation.isPending} submitLabel="Add Action" />
    </FormPageLayout>
  )
}
