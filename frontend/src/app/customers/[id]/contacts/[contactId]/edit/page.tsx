"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import api from "@/services/api"
import { useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { useEffect } from "react"
import { ContactDto } from "@/types/api"

const contactSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().email("Invalid email address"),
  phone: z.string().min(1, "Phone number is required"),
  role: z.string().min(1, "Role is required"),
  isPrimary: z.boolean(),
})

type ContactForm = z.infer<typeof contactSchema>

export default function EditContactPage() {
  const params = useParams()
  const router = useRouter()
  const customerId = params.id as string
  const contactId = params.contactId as string

  const { data: contacts } = useQuery({
    queryKey: queryKeys.customers.contacts(customerId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/customers/${customerId}/contacts`)
      return res.data as ContactDto[]
    },
    enabled: !!customerId,
  })

  const contact = contacts?.find((c) => c.id === contactId)
  const isLoading = !contacts
  const error = contacts && !contact ? new Error("Contact not found") : null

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ContactForm>({
    resolver: zodResolver(contactSchema),
    defaultValues: { isPrimary: false },
  })

  useEffect(() => {
    if (contact) {
      reset({
        firstName: contact.firstName,
        lastName: contact.lastName,
        email: contact.email,
        phone: contact.phoneNumber ?? "",
        role: contact.position ?? "",
        isPrimary: contact.isPrimary,
      })
    }
  }, [contact, reset])

  const updateMutation = useMutation({
    mutationFn: async (data: ContactForm) => {
      const res = await api.put(`/api/v1/crm/customers/${customerId}/contacts/${contactId}`, data)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Contact updated", description: "Contact has been updated successfully." })
      router.push(`/customers/${customerId}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to update contact.", variant: "destructive" })
    },
  })

  const onSubmit = (data: ContactForm) => {
    updateMutation.mutate(data)
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !contact) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load contact" /></div>

  return (
    <FormPageLayout title="Edit Contact" backHref={`/customers/${customerId}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Contact Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="First Name"
            required
            error={errors.firstName}
            registration={register("firstName")}
            placeholder="John"
          />
          <FormField
            label="Last Name"
            required
            error={errors.lastName}
            registration={register("lastName")}
            placeholder="Doe"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Email"
            required
            error={errors.email}
            registration={register("email")}
            type="email"
            placeholder="john@example.com"
          />
          <FormField
            label="Phone"
            required
            error={errors.phone}
            registration={register("phone")}
            placeholder="+1 555-0123"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Role"
            required
            error={errors.role}
            registration={register("role")}
            placeholder="Manager"
          />
          <div className="flex items-center gap-2 pt-8">
            <input type="checkbox" id="isPrimary" {...register("isPrimary")} className="h-4 w-4" />
            <label htmlFor="isPrimary" className="text-sm font-medium">Primary Contact</label>
          </div>
        </div>
      </FormSection>
      <FormActions backHref={`/customers/${customerId}`} loading={updateMutation.isPending} submitLabel="Update Contact" />
    </FormPageLayout>
  )
}
