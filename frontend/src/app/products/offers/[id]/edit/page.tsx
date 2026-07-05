"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { useOffer } from "@/api/hooks/useOffer"
import { useUpdateOffer } from "@/api/hooks/useUpdateOffer"
import { useEffect } from "react"

const offerSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  offerType: z.string().min(1, "Offer type is required"),
  billingPeriod: z.string().optional(),
  isContract: z.boolean().optional(),
  contractDurationMonths: z.number().optional(),
  taxInclusive: z.boolean().optional(),
  sortOrder: z.number().optional(),
  validFrom: z.string().optional(),
  validTo: z.string().optional(),
})

type OfferForm = z.infer<typeof offerSchema>

const offerTypeOptions = [
  { label: "One Time", value: "OneTime" },
  { label: "Recurring", value: "Recurring" },
  { label: "Usage Based", value: "UsageBased" },
  { label: "Bundled", value: "Bundled" },
]

const billingPeriodOptions = [
  { label: "Monthly", value: "Monthly" },
  { label: "Quarterly", value: "Quarterly" },
  { label: "Semi-Annual", value: "SemiAnnual" },
  { label: "Annual", value: "Annual" },
]

export default function EditOfferPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: offer, isLoading, error } = useOffer(id)
  const updateOffer = useUpdateOffer(id)

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm<OfferForm>({
    resolver: zodResolver(offerSchema),
  })

  useEffect(() => {
    if (offer) {
      reset({
        name: offer.name,
        description: offer.description ?? "",
        offerType: offer.offerType,
        billingPeriod: offer.billingPeriod ?? undefined,
        isContract: offer.isContract,
        contractDurationMonths: offer.contractDurationMonths ?? undefined,
        taxInclusive: offer.taxInclusive,
        sortOrder: offer.sortOrder,
        validFrom: offer.validFrom ? offer.validFrom.split("T")[0] : undefined,
        validTo: offer.validTo ? offer.validTo.split("T")[0] : undefined,
      })
    }
  }, [offer, reset])

  const onSubmit = (data: OfferForm) => {
    updateOffer.mutate({
      offerId: id,
      name: data.name,
      description: data.description || null,
      offerType: data.offerType,
      isContract: data.isContract ?? false,
      contractDurationMonths: data.isContract ? (data.contractDurationMonths ?? null) : null,
      billingPeriod: data.billingPeriod || null,
      taxInclusive: data.taxInclusive ?? false,
      sortOrder: data.sortOrder ?? 0,
      validFrom: data.validFrom ? new Date(data.validFrom).toISOString() : null,
      validTo: data.validTo ? new Date(data.validTo).toISOString() : null,
    }, {
      onSuccess: () => {
        toast({ title: "Offer updated", description: "Offer has been updated successfully." })
        router.push(`/products/offers/${id}`)
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to update offer.", variant: "destructive" })
      },
    })
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !offer) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load offer" /></div>

  return (
    <FormPageLayout title="Edit Offer" backHref={`/products/offers/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Offer Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Name"
            required
            error={errors.name}
            registration={register("name")}
            placeholder="Offer name"
          />
          <FormSelectField
            label="Offer Type"
            required
            error={errors.offerType}
            options={offerTypeOptions}
            value={offer.offerType}
            onValueChange={(v) => setValue("offerType", v)}
            placeholder="Select offer type"
          />
        </div>
        <FormField
          label="Description"
          error={errors.description}
          registration={register("description")}
          placeholder="Offer description"
        />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Billing Period"
            error={errors.billingPeriod}
            options={billingPeriodOptions}
            value={offer.billingPeriod ?? ""}
            onValueChange={(v) => setValue("billingPeriod", v)}
            placeholder="Select billing period"
          />
          <FormField
            label="Sort Order"
            error={errors.sortOrder}
            registration={register("sortOrder")}
            type="number"
            placeholder="0"
          />
        </div>
      </FormSection>
      <FormSection title="Validity & Terms">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Valid From"
            error={errors.validFrom}
            registration={register("validFrom")}
            type="date"
          />
          <FormField
            label="Valid To"
            error={errors.validTo}
            registration={register("validTo")}
            type="date"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-3">
          <FormField
            label="Contract Duration (months)"
            error={errors.contractDurationMonths}
            registration={register("contractDurationMonths")}
            type="number"
            placeholder="12"
          />
        </div>
      </FormSection>
      <FormActions backHref={`/products/offers/${id}`} loading={updateOffer.isPending} submitLabel="Update Offer" />
    </FormPageLayout>
  )
}
