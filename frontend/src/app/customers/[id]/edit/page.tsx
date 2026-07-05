"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { useCustomer } from "@/api/hooks/useCustomer"
import { useUpdateCustomer } from "@/api/hooks/useUpdateCustomer"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { UpdateCustomerCommand } from "@/api/generated"
import { useEffect } from "react"

const customerEditSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().email("Invalid email address"),
  phone: z.string().min(1, "Phone number is required"),
  country: z.string().min(1, "Country is required"),
  companyName: z.string().optional(),
  taxNumber: z.string().optional(),
  registrationNumber: z.string().optional(),
  website: z.string().optional(),
  description: z.string().optional(),
  statusReason: z.string().optional(),
  externalId: z.string().optional(),
  validFrom: z.string().optional(),
  validUntil: z.string().optional(),
})

type CustomerEditForm = z.infer<typeof customerEditSchema>

export default function EditCustomerPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()

  const { data: customer, isLoading, error } = useCustomer(id)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CustomerEditForm>({
    resolver: zodResolver(customerEditSchema),
  })

  useEffect(() => {
    if (customer) {
      const nameParts = customer.displayName.split(" ")
      const firstName = nameParts[0] || ""
      const lastName = nameParts.slice(1).join(" ") || ""
      reset({
        firstName,
        lastName,
        email: customer.email,
        phone: customer.phoneNumber ?? "",
        country: customer.countryCode ?? "",
        companyName: customer.companyName ?? "",
        taxNumber: customer.taxNumber ?? "",
        registrationNumber: customer.registrationNumber ?? "",
        website: customer.website ?? "",
        description: customer.description ?? "",
        statusReason: customer.statusReason ?? "",
        externalId: customer.externalId ?? "",
        validFrom: customer.validFrom ? customer.validFrom.split("T")[0] : "",
        validUntil: customer.validUntil ? customer.validUntil.split("T")[0] : "",
      })
    }
  }, [customer, reset])

  const updateCustomer = useUpdateCustomer(id)

  const patchCustomer = useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      await api.patch(`/api/v1/crm/customers/${id}`, { id, ...data })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.detail(id) })
    },
  })

  const onSubmit = (data: CustomerEditForm) => {
    const payload: UpdateCustomerCommand = {
      customerId: id,
      displayName: `${data.firstName} ${data.lastName}`.trim(),
      email: data.email,
      companyName: data.companyName ?? null,
      taxNumber: data.taxNumber ?? null,
      registrationNumber: data.registrationNumber ?? null,
      phoneNumber: data.phone ?? null,
      countryCode: data.country ?? null,
      website: data.website ?? null,
    }
    updateCustomer.mutate(payload, {
      onSuccess: () => {
        const tmfPayload: Record<string, unknown> = {}
        if (data.description) tmfPayload.description = data.description
        if (data.statusReason) tmfPayload.statusReason = data.statusReason
        if (data.externalId) tmfPayload.externalId = data.externalId
        if (data.validFrom) tmfPayload.validFrom = new Date(data.validFrom).toISOString()
        if (data.validUntil) tmfPayload.validUntil = new Date(data.validUntil).toISOString()

        if (Object.keys(tmfPayload).length > 0) {
          patchCustomer.mutate(tmfPayload, {
            onSuccess: () => {
              const displayName = `${data.firstName} ${data.lastName}`.trim()
              toast({ title: "Customer updated", description: `${displayName} has been updated successfully.` })
              router.push(`/customers/${id}`)
            },
          })
        } else {
          const displayName = `${data.firstName} ${data.lastName}`.trim()
          toast({ title: "Customer updated", description: `${displayName} has been updated successfully.` })
          router.push(`/customers/${id}`)
        }
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to update customer.", variant: "destructive" })
      },
    })
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !customer) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load customer" /></div>

  return (
    <FormPageLayout title="Edit Customer" backHref={`/customers/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Customer Information">
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
            label="Company Name"
            error={errors.companyName}
            registration={register("companyName")}
            placeholder="Acme Inc."
          />
          <FormField
            label="Email"
            required
            error={errors.email}
            registration={register("email")}
            type="email"
            placeholder="john@example.com"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Phone"
            required
            error={errors.phone}
            registration={register("phone")}
            placeholder="+1 555-0123"
          />
          <FormField
            label="Country"
            required
            error={errors.country}
            registration={register("country")}
            placeholder="United States"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Tax Number"
            error={errors.taxNumber}
            registration={register("taxNumber")}
            placeholder="TX-123456"
          />
          <FormField
            label="Registration Number"
            error={errors.registrationNumber}
            registration={register("registrationNumber")}
            placeholder="REG-789012"
          />
        </div>
        <FormField
          label="Website"
          error={errors.website}
          registration={register("website")}
          placeholder="https://acme.com"
        />
      </FormSection>
      <FormSection title="TMF Attributes">
        <FormField
          label="Description"
          error={errors.description}
          registration={register("description")}
          placeholder="Customer description"
        />
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Status Reason"
            error={errors.statusReason}
            registration={register("statusReason")}
            placeholder="Reason for current status"
          />
          <FormField
            label="External ID"
            error={errors.externalId}
            registration={register("externalId")}
            placeholder="External system reference"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Valid From"
            error={errors.validFrom}
            registration={register("validFrom")}
            type="date"
          />
          <FormField
            label="Valid Until"
            error={errors.validUntil}
            registration={register("validUntil")}
            type="date"
          />
        </div>
      </FormSection>
      <FormActions backHref={`/customers/${id}`} loading={updateCustomer.isPending || patchCustomer.isPending} submitLabel="Update Customer" />
    </FormPageLayout>
  )
}
