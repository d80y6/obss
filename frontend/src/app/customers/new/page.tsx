"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useQuery, useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { useAuthStore } from "@/stores/auth-store"
import type { CustomerDto } from "@/api/generated"

interface LookupItem {
  value: string
  label: string
}

const customerSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().email("Invalid email address"),
  phoneNumber: z.string().optional(),
  companyName: z.string().optional(),
  taxNumber: z.string().optional(),
  countryCode: z.string().optional(),
  registrationNumber: z.string().optional(),
  website: z.string().optional(),
  customerType: z.string().min(1, "Customer type is required"),
  currency: z.string().min(1, "Currency is required"),
})

type CustomerForm = z.infer<typeof customerSchema>

export default function NewCustomerPage() {
  const router = useRouter()
  const user = useAuthStore((s) => s.user)

  const { data: customerTypes } = useQuery({
    queryKey: ["lookups", "customer-types"],
    queryFn: async () => {
      const res = await api.get<LookupItem[]>("/api/v1/crm/lookups/customer-types")
      return res.data
    },
    staleTime: 300000,
  })

  const { data: currencies } = useQuery({
    queryKey: ["lookups", "currencies"],
    queryFn: async () => {
      const res = await api.get<LookupItem[]>("/api/v1/crm/lookups/currencies")
      return res.data
    },
    staleTime: 300000,
  })

  const {
    register,
    handleSubmit,
    setError,
    setValue,
    watch,
    formState: { errors },
  } = useForm<CustomerForm>({
    resolver: zodResolver(customerSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      phoneNumber: "",
      companyName: "",
      taxNumber: "",
      countryCode: "",
      registrationNumber: "",
      website: "",
      customerType: "",
      currency: "",
    },
  })

  const createMutation = useMutation({
    mutationFn: async (data: CustomerForm) => {
      const res = await api.post<CustomerDto>("/api/v1/crm/customers", {
        tenantId: user?.tenantId ?? "",
        customerType: data.customerType,
        companyName: data.companyName || null,
        displayName: `${data.firstName} ${data.lastName}`.trim(),
        taxNumber: data.taxNumber || null,
        registrationNumber: data.registrationNumber || null,
        email: data.email,
        phoneNumber: data.phoneNumber || null,
        countryCode: data.countryCode || null,
        website: data.website || null,
        currency: data.currency,
      })
      return res.data
    },
    onSuccess: (data) => {
      toast({
        title: "Customer created",
        description: `${data.displayName} has been created successfully.`,
      })
      router.push(`/customers/${data.id}`)
    },
    onError: (error) => {
      const apiError = error as { response?: { data?: { errors?: Record<string, string[]> } } }
      const fieldErrors = apiError?.response?.data?.errors
      if (fieldErrors) {
        Object.entries(fieldErrors).forEach(([field, messages]) => {
          setError(field as keyof CustomerForm, {
            message: (messages as string[])[0],
          })
        })
      } else {
        toast({
          title: "Error",
          description: "Failed to create customer. Please try again.",
          variant: "destructive",
        })
      }
    },
  })

  const onSubmit = (data: CustomerForm) => {
    createMutation.mutate(data)
  }

  return (
    <FormPageLayout title="New Customer" backHref="/customers" onSubmit={handleSubmit(onSubmit)}>
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
            label="Email"
            required
            error={errors.email}
            registration={register("email")}
            type="email"
            placeholder="john@example.com"
          />
          <FormField
            label="Phone"
            error={errors.phoneNumber}
            registration={register("phoneNumber")}
            placeholder="+1 555-0123"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Company"
            error={errors.companyName}
            registration={register("companyName")}
            placeholder="ACME Corp"
          />
          <FormField
            label="Tax ID"
            error={errors.taxNumber}
            registration={register("taxNumber")}
            placeholder="TX-12345"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Registration No."
            error={errors.registrationNumber}
            registration={register("registrationNumber")}
            placeholder="REG-001"
          />
          <FormField
            label="Country Code"
            error={errors.countryCode}
            registration={register("countryCode")}
            placeholder="+967"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Website"
            error={errors.website}
            registration={register("website")}
            placeholder="https://example.com"
          />
          <FormSelectField
            label="Customer Type"
            required
            error={errors.customerType}
            value={watch("customerType")}
            onValueChange={(v) => setValue("customerType", v)}
            placeholder="Select customer type..."
            options={customerTypes ?? []}
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Currency"
            required
            error={errors.currency}
            value={watch("currency")}
            onValueChange={(v) => setValue("currency", v)}
            placeholder="Select currency..."
            options={currencies ?? []}
          />
        </div>
      </FormSection>
      <FormActions backHref="/customers" loading={createMutation.isPending} submitLabel="Create Customer" />
    </FormPageLayout>
  )
}
