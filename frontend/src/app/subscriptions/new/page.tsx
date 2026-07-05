"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { useCustomers } from "@/api/hooks/useCustomers"
import { useOffers } from "@/api/hooks/useOffers"
import type { SubscriptionDto } from "@/api/generated"

const subscriptionSchema = z.object({
  customerId: z.string().min(1, "Customer is required"),
  offerId: z.string().min(1, "Offer is required"),
  quantity: z.string().min(1, "Quantity is required"),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().optional(),
})

type SubscriptionForm = z.infer<typeof subscriptionSchema>

export default function NewSubscriptionPage() {
  const router = useRouter()

  const { data: customers } = useCustomers()
  const { data: offers } = useOffers()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<SubscriptionForm>({
    resolver: zodResolver(subscriptionSchema),
  })

  const createMutation = useMutation({
    mutationFn: async (data: SubscriptionForm) => {
      const customer = customers?.items.find((c) => c.id === data.customerId)
      const offer = offers?.items.find((o) => o.id === data.offerId)
      const res = await api.post<SubscriptionDto>("/api/v1/subscriptions/subscriptions", {
        customerId: data.customerId,
        customerName: customer ? customer.displayName : "",
        orderId: "00000000-0000-0000-0000-000000000000",
        orderItemId: "00000000-0000-0000-0000-000000000000",
        productId: data.offerId,
        offerId: data.offerId,
        offerName: offer?.name ?? "",
        billingPeriod: offer?.billingPeriod ?? "",
        currency: offer?.pricings?.[0]?.currency ?? "USD",
        price: offer?.pricings?.[0]?.recurringPrice ?? offer?.pricings?.[0]?.oneTimePrice ?? 0,
        quantity: parseInt(data.quantity),
        startDate: data.startDate,
        endDate: data.endDate || null,
      })
      return res.data
    },
    onSuccess: (data) => {
      toast({ title: "Subscription created", description: `Subscription ${data.offerName} has been created.` })
      router.push(`/subscriptions/${data.id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create subscription.", variant: "destructive" })
    },
  })

  const onSubmit = (data: SubscriptionForm) => {
    createMutation.mutate(data)
  }

  const customerOptions = (customers?.items ?? []).map((c) => ({
    label: `${c.displayName} (${c.email})`,
    value: c.id,
  }))

  const offerOptions = (offers?.items ?? []).map((o) => ({
    label: `${o.name} (${o.pricings?.[0]?.currency ?? "USD"} ${o.pricings?.[0]?.recurringPrice ?? o.pricings?.[0]?.oneTimePrice ?? 0})`,
    value: o.id,
  }))

  return (
    <FormPageLayout title="New Subscription" backHref="/subscriptions" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Subscription Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Customer"
            required
            error={errors.customerId}
            options={customerOptions}
            value=""
            onValueChange={(v) => setValue("customerId", v)}
            placeholder="Select a customer"
          />
          <FormSelectField
            label="Offer"
            required
            error={errors.offerId}
            options={offerOptions}
            value=""
            onValueChange={(v) => setValue("offerId", v)}
            placeholder="Select an offer"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Quantity"
            required
            error={errors.quantity}
            registration={register("quantity")}
            type="number"
            placeholder="1"
          />
          <FormField
            label="Start Date"
            required
            error={errors.startDate}
            registration={register("startDate")}
            type="date"
          />
        </div>
      </FormSection>
      <FormActions backHref="/subscriptions" loading={createMutation.isPending} submitLabel="Create Subscription" />
    </FormPageLayout>
  )
}
