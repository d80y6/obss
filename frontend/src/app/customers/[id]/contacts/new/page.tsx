"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import type { ContactDto } from "@/api/generated"

const contactSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().email("Invalid email address"),
  phone: z.string().min(1, "Phone number is required"),
  role: z.string().min(1, "Role is required"),
  isPrimary: z.boolean().optional(),
})

type ContactForm = z.infer<typeof contactSchema>

export default function NewContactPage() {
  const params = useParams()
  const router = useRouter()
  const customerId = params.id as string

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ContactForm>({
    resolver: zodResolver(contactSchema),
    defaultValues: { isPrimary: false },
  })

  const createMutation = useMutation({
    mutationFn: async (data: ContactForm) => {
      const res = await api.post<ContactDto>(`/api/v1/crm/customers/${customerId}/contacts`, data)
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Contact created", description: "Contact has been created successfully." })
      router.push(`/customers/${customerId}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create contact.", variant: "destructive" })
    },
  })

  const onSubmit = (data: ContactForm) => {
    createMutation.mutate(data)
  }

  return (
    <FormPageLayout title="New Contact" backHref={`/customers/${customerId}`} onSubmit={handleSubmit(onSubmit)}>
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
      <FormActions backHref={`/customers/${customerId}`} loading={createMutation.isPending} submitLabel="Create Contact" />
    </FormPageLayout>
  )
}
