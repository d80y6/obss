"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { useSubscription } from "@/api/hooks/useSubscription"
import { useOffers } from "@/api/hooks/useOffers"
import api from "@/services/api"

const subscriptionSchema = z.object({
  offerId: z.string().optional(),
})

type SubscriptionForm = z.infer<typeof subscriptionSchema>

export default function EditSubscriptionPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: sub, isLoading, error } = useSubscription(id)
  const { data: offers } = useOffers()

  const {
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<SubscriptionForm>({
    resolver: zodResolver(subscriptionSchema),
  })

  const offerChangeMutation = useMutation({
    mutationFn: async (offerId: string) => {
      const selectedOffer = offers?.find((o) => o.id === offerId)
      await api.put(`/api/v1/subscriptions/subscriptions/${id}/offer`, {
        newOfferId: offerId,
        newPrice: selectedOffer?.price ?? 0,
      })
    },
    onSuccess: () => {
      toast({ title: "Offer changed", description: "Subscription offer has been changed." })
      router.push(`/subscriptions/${id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to change offer.", variant: "destructive" })
    },
  })

  const onSubmit = (data: SubscriptionForm) => {
    if (data.offerId && data.offerId !== sub?.offerId) {
      offerChangeMutation.mutate(data.offerId)
    }
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !sub) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load subscription" /></div>

  const offerOptions = (offers ?? []).map((o) => ({
    label: `${o.name} (${o.currency} ${o.price})`,
    value: o.id,
  }))

  return (
    <FormPageLayout title="Change Offer" backHref={`/subscriptions/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Change Offer">
        <FormSelectField
          label="Offer"
          error={errors.offerId}
          options={offerOptions}
          value=""
          onValueChange={(v) => setValue("offerId", v)}
          placeholder={sub.offerName}
        />
      </FormSection>
      <FormActions backHref={`/subscriptions/${id}`} loading={offerChangeMutation.isPending} submitLabel="Change Offer" />
    </FormPageLayout>
  )
}
