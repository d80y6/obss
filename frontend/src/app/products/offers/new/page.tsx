"use client"

import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import type { OfferDto } from "@/api/generated"
import { useProducts } from "@/api/hooks/useProducts"
import { queryKeys } from "@/lib/query-keys"
import { useQueryClient } from "@tanstack/react-query"

const offerSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  productId: z.string().min(1, "Product is required"),
  offerType: z.string().min(1, "Offer type is required"),
  billingPeriod: z.string().optional(),
  isContract: z.boolean().default(false),
  contractDurationMonths: z.coerce.number().optional(),
  taxInclusive: z.boolean().default(false),
  sortOrder: z.coerce.number().default(0),
  recurringPrice: z.coerce.number().default(0),
  oneTimePrice: z.coerce.number().default(0),
  currency: z.string().default("USD"),
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

export default function NewOfferPage() {
  const router = useRouter()
  const queryClient = useQueryClient()

  const { data: products } = useProducts()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<OfferForm>({
    resolver: zodResolver(offerSchema),
    defaultValues: { currency: "USD", isContract: false, taxInclusive: false, sortOrder: 0 },
  })

  const createMutation = useMutation({
    mutationFn: async (data: OfferForm) => {
      const res = await api.post<OfferDto>("/api/v1/catalog/offers", {
        name: data.name,
        description: data.description || null,
        productId: data.productId,
        offerType: data.offerType,
        isContract: data.isContract,
        contractDurationMonths: data.isContract ? data.contractDurationMonths || null : null,
        billingPeriod: data.billingPeriod || null,
        taxInclusive: data.taxInclusive,
        sortOrder: data.sortOrder,
        validFrom: data.validFrom ? new Date(data.validFrom).toISOString() : null,
        validTo: data.validTo ? new Date(data.validTo).toISOString() : null,
        pricings: [
          {
            pricingType: "Flat",
            currency: data.currency,
            recurringPrice: data.recurringPrice,
            oneTimePrice: data.oneTimePrice,
            usagePrice: 0,
            unitOfMeasure: null,
            minQuantity: null,
            maxQuantity: null,
            isActive: true,
          },
        ],
      })
      return res.data
    },
    onSuccess: (data) => {
      toast({ title: "Offer created", description: `${data.name} has been created successfully.` })
      queryClient.invalidateQueries({ queryKey: queryKeys.products.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.offers.all })
      router.push(`/products/offers/${data.id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to create offer.", variant: "destructive" })
    },
  })

  const onSubmit = (data: OfferForm) => {
    createMutation.mutate(data)
  }

  const productOptions = (products?.items ?? []).map((p) => ({ label: p.name, value: p.id }))

  return (
    <FormPageLayout title="New Offer" backHref="/products/offers" onSubmit={handleSubmit(onSubmit)}>
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
            label="Product"
            required
            error={errors.productId}
            options={productOptions}
            value=""
            onValueChange={(v) => setValue("productId", v)}
            placeholder="Select a product"
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
            label="Offer Type"
            required
            error={errors.offerType}
            options={offerTypeOptions}
            value=""
            onValueChange={(v) => setValue("offerType", v)}
            placeholder="Select offer type"
          />
          <FormSelectField
            label="Billing Period"
            error={errors.billingPeriod}
            options={billingPeriodOptions}
            value=""
            onValueChange={(v) => setValue("billingPeriod", v)}
            placeholder="Select billing period"
          />
        </div>
      </FormSection>
      <FormSection title="Pricing">
        <div className="grid gap-4 md:grid-cols-3">
          <FormField
            label="Recurring Price"
            error={errors.recurringPrice}
            registration={register("recurringPrice")}
            type="number"
            step="0.01"
            placeholder="0.00"
          />
          <FormField
            label="One-Time Price"
            error={errors.oneTimePrice}
            registration={register("oneTimePrice")}
            type="number"
            step="0.01"
            placeholder="0.00"
          />
          <FormField
            label="Currency"
            error={errors.currency}
            registration={register("currency")}
            placeholder="USD"
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
            label="Sort Order"
            error={errors.sortOrder}
            registration={register("sortOrder")}
            type="number"
            placeholder="0"
          />
          <FormField
            label="Contract Duration (months)"
            error={errors.contractDurationMonths}
            registration={register("contractDurationMonths")}
            type="number"
            placeholder="12"
          />
        </div>
      </FormSection>
      <FormActions backHref="/products/offers" loading={createMutation.isPending} submitLabel="Create Offer" />
    </FormPageLayout>
  )
}
