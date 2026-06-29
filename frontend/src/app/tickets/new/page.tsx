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
import { useCreateTicket } from "@/api/hooks/useCreateTicket"
import type { CreateTicketCommand } from "@/api/generated"

const ticketSchema = z.object({
  customerId: z.string().min(1, "Customer ID is required"),
  subject: z.string().min(1, "Subject is required"),
  description: z.string().min(1, "Description is required"),
  category: z.string().min(1, "Category is required"),
  priority: z.string().min(1, "Priority is required"),
})

type TicketForm = z.infer<typeof ticketSchema>

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

export default function NewTicketPage() {
  const router = useRouter()
  const createTicket = useCreateTicket()

  const { register, handleSubmit, setValue, formState: { errors } } = useForm<TicketForm>({
    resolver: zodResolver(ticketSchema),
  })

  const onSubmit = (data: TicketForm) => {
    createTicket.mutate({
      ...data,
      customerName: "",
      source: "Portal",
    } as CreateTicketCommand, {
      onSuccess: () => {
        toast({ title: "Ticket created", description: "Ticket has been created." })
        router.push("/tickets")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create ticket.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="New Ticket" backHref="/tickets" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Ticket Information">
        <FormField label="Customer ID" required error={errors.customerId} registration={register("customerId")} placeholder="Customer ID" />
        <FormField label="Subject" required error={errors.subject} registration={register("subject")} placeholder="Brief description" />
        <FormField label="Description" required error={errors.description} registration={register("description")} placeholder="Detailed description" />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField label="Category" required error={errors.category} options={categories} value="" onValueChange={(v) => setValue("category", v)} placeholder="Select category" />
          <FormSelectField label="Priority" required error={errors.priority} options={priorities} value="" onValueChange={(v) => setValue("priority", v)} placeholder="Select priority" />
        </div>
      </FormSection>
      <FormActions backHref="/tickets" loading={createTicket.isPending} submitLabel="Create Ticket" />
    </FormPageLayout>
  )
}
