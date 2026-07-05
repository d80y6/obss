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
import { useCreateTelephoneNumber } from "@/api/hooks/useCreateTelephoneNumber"

const numberSchema = z.object({
  number: z.string().min(1, "Number is required"),
  numberType: z.string().min(1, "Number type is required"),
  cost: z.string().optional(),
  currency: z.string().min(1, "Currency is required"),
  notes: z.string().optional(),
})

type NumberForm = z.input<typeof numberSchema>

const numberTypes = [
  { label: "Geographic", value: "Geographic" },
  { label: "Mobile", value: "Mobile" },
  { label: "Toll Free", value: "TollFree" },
  { label: "National", value: "National" },
  { label: "Premium", value: "Premium" },
  { label: "Shared Cost", value: "SharedCost" },
]

export default function NewNumberPage() {
  const router = useRouter()
  const createNumber = useCreateTelephoneNumber()

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<NumberForm>({
    resolver: zodResolver(numberSchema),
    defaultValues: {
      currency: "USD",
    },
  })

  const onSubmit = (data: NumberForm) => {
    createNumber.mutate({
      ...data,
      cost: data.cost ? Number(data.cost) : undefined,
    } as Parameters<typeof createNumber.mutate>[0], {
      onSuccess: (tel) => {
        toast({ title: "Number created", description: `${tel.number} has been created successfully.` })
        router.push(`/number-inventory/${tel.id}`)
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to create number.", variant: "destructive" })
      },
    })
  }

  return (
    <FormPageLayout title="Add Telephone Number" backHref="/number-inventory" onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Number Information">
        <FormField
          label="Number"
          required
          error={errors.number}
          registration={register("number")}
          placeholder="e.g. +1-555-123-4567"
        />
        <div className="grid gap-4 md:grid-cols-2">
          <FormSelectField
            label="Number Type"
            required
            error={errors.numberType}
            options={numberTypes}
            value=""
            onValueChange={(v) => setValue("numberType", v)}
            placeholder="Select type"
          />
          <FormField
            label="Cost"
            error={errors.cost}
            registration={register("cost")}
            type="number"
            placeholder="0.00"
          />
          <FormField
            label="Currency"
            required
            error={errors.currency}
            registration={register("currency")}
            placeholder="USD"
          />
        </div>
        <FormField
          label="Notes"
          error={errors.notes}
          registration={register("notes")}
          placeholder="Optional notes"
        />
      </FormSection>
      <FormActions backHref="/number-inventory" loading={createNumber.isPending} submitLabel="Create Number" />
    </FormPageLayout>
  )
}
