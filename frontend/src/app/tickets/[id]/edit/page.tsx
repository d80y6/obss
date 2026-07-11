"use client"

import { useRouter, useParams } from "next/navigation"
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
import { useTicket } from "@/api/hooks/useTicket"
import { useUsers } from "@/api/hooks/useUsers"

const editTicketSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().min(1, "Description is required"),
  category: z.string().min(1, "Category is required"),
  priority: z.string().min(1, "Priority is required"),
  assignedTo: z.string().optional(),
})

type EditTicketForm = z.infer<typeof editTicketSchema>

const categories = [
  { label: "Billing", value: "BILLING" },
  { label: "Technical", value: "TECHNICAL" },
  { label: "Account", value: "ACCOUNT" },
  { label: "Service", value: "SERVICE" },
  { label: "Other", value: "OTHER" },
]

const priorities = [
  { label: "Low", value: "LOW" },
  { label: "Medium", value: "MEDIUM" },
  { label: "High", value: "HIGH" },
  { label: "Critical", value: "CRITICAL" },
]

export default function EditTicketPage() {
  const router = useRouter()
  const params = useParams()
  const id = params.id as string
  const queryClient = useQueryClient()

  const { data: ticket } = useTicket(id)
  const { data: users } = useUsers({})

  const updateMutation = useMutation({
    mutationFn: async (data: EditTicketForm) => {
      const res = await api.put(`/api/v1/ticketing/tickets/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.lists() })
      toast({ title: "Ticket updated", description: "Ticket has been updated." })
      router.push(`/tickets/${id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to update ticket.", variant: "destructive" })
    },
  })

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<EditTicketForm>({
    resolver: zodResolver(editTicketSchema),
    values: ticket ? {
      title: ticket.subject,
      description: ticket.description,
      category: ticket.category,
      priority: ticket.priority,
      assignedTo: ticket.assignedTo || "",
    } : undefined,
  })

  const userOptions = (users?.items ?? []).map((u) => ({
    label: `${u.firstName} ${u.lastName} (${u.username})`,
    value: u.id,
  }))

  const onSubmit = (data: EditTicketForm) => updateMutation.mutate(data)

  return (
    <FormPageLayout title="Edit Ticket" backHref={`/tickets/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Ticket Information">
        <FormField label="Title" required error={errors.title} registration={register("title")} placeholder="Ticket title" />
        <FormField label="Description" required error={errors.description} registration={register("description")} placeholder="Detailed description" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField label="Category" required error={errors.category} options={categories} value={ticket?.category ?? ""} onValueChange={(v) => setValue("category", v)} placeholder="Select category" />
          <FormSelectField label="Priority" required error={errors.priority} options={priorities} value={ticket?.priority ?? ""} onValueChange={(v) => setValue("priority", v)} placeholder="Select priority" />
        </div>
        <FormSelectField label="Assigned To" error={errors.assignedTo} options={userOptions} value={ticket?.assignedTo ?? ""} onValueChange={(v) => setValue("assignedTo", v)} placeholder="Select user" />
      </FormSection>
      <FormActions backHref={`/tickets/${id}`} loading={updateMutation.isPending} submitLabel="Update Ticket" />
    </FormPageLayout>
  )
}
