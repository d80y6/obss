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
import { useCreatePromotion } from "@/api/hooks/use-rating"
import type { CreatePromotionCommand } from "@/api/generated"

const promotionSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  code: z.string().min(1, "Code is required"),
  promotionType: z.string().min(1, "Promotion type is required"),
  discountType: z.string().min(1, "Discount type is required"),
  discountValue: z.coerce.number().min(0, "Value must be positive"),
  validFrom: z.string().min(1, "Valid from is required"),
  validTo: z.string().optional(),
  priority: z.coerce.number().int().min(0, "Priority must be positive"),
})

type PromotionForm = z.infer<typeof promotionSchema>

const promotionTypes = [
  { label: "Percentage", value: "PERCENTAGE" },
  { label: "Fixed Amount", value: "FIXED_AMOUNT" },
  { label: "Buy X Get Y", value: "BUY_X_GET_Y" },
  { label: "Free Shipping", value: "FREE_SHIPPING" },
]

const discountTypes = [
  { label: "Percentage", value: "PERCENTAGE" },
  { label: "Fixed Amount", value: "FIXED_AMOUNT" },
]

export default function NewPromotionPage() {
  const router = useRouter()
  const createMutation = useCreatePromotion()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<PromotionForm>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(promotionSchema) as any,
  })

  const onSubmit = (data: PromotionForm) => {
    const command: CreatePromotionCommand = {
      name: data.name,
      description: data.description ?? null,
      promotionType: data.promotionType,
      discountValue: data.discountValue,
      currency: "USD",
      minQuantity: null,
      maxQuantity: null,
      validFrom: data.validFrom,
      validTo: data.validTo ?? null,
      isStackable: false,
      priority: data.priority,
      code: data.code,
      maxRedemptions: null,
      rules: [],
    }
    createMutation.mutate(command, {
      onSuccess: () => {
        toast({ title: "Promotion created", description: "Promotion has been created successfully." })
        router.push("/rating/promotions")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create promotion.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="New Promotion" backHref="/rating/promotions" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Promotion Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Name"
            required
            error={errors.name}
            registration={register("name")}
            placeholder="Promotion name"
          />
          <FormField
            label="Code"
            required
            error={errors.code}
            registration={register("code")}
            placeholder="PROMO2024"
          />
        </div>
        <FormField
          label="Description"
          error={errors.description}
          registration={register("description")}
          placeholder="Promotion description"
        />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Promotion Type"
            required
            error={errors.promotionType}
            options={promotionTypes}
            value=""
            onValueChange={(v) => setValue("promotionType", v)}
            placeholder="Select type"
          />
          <FormSelectField
            label="Discount Type"
            required
            error={errors.discountType}
            options={discountTypes}
            value=""
            onValueChange={(v) => setValue("discountType", v)}
            placeholder="Select discount type"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Discount Value"
            required
            type="number"
            error={errors.discountValue}
            registration={register("discountValue")}
            placeholder="0"
          />
          <FormField
            label="Priority"
            required
            type="number"
            error={errors.priority}
            registration={register("priority")}
            placeholder="0"
          />
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Valid From"
            required
            type="datetime-local"
            error={errors.validFrom}
            registration={register("validFrom")}
          />
          <FormField
            label="Valid To"
            type="datetime-local"
            error={errors.validTo}
            registration={register("validTo")}
          />
        </div>
      </FormSection>
      <FormActions backHref="/rating/promotions" loading={createMutation.isPending} submitLabel="Create Promotion" />
    </FormPageLayout>
  )
}
