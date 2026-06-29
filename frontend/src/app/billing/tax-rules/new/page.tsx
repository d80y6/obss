"use client"
import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  rate: z.string().min(1, "Rate is required"),
  region: z.string().min(1, "Region is required"),
  productCategory: z.string().min(1, "Product category is required"),
  effectiveFrom: z.string().min(1, "Effective from date is required"),
  effectiveTo: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const regionOptions = [
  { label: "North America", value: "NORTH_AMERICA" },
  { label: "Europe", value: "EUROPE" },
  { label: "Asia Pacific", value: "ASIA_PACIFIC" },
  { label: "Latin America", value: "LATIN_AMERICA" },
  { label: "Middle East", value: "MIDDLE_EAST" },
  { label: "Africa", value: "AFRICA" },
]

export default function NewTaxRulePage() {
  const router = useRouter()
  const queryClient = useQueryClient()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post("/api/v1/billing/tax-rules", {
        name: data.name,
        description: data.name,
        taxRate: parseFloat(data.rate),
        taxType: "SALES",
        taxCategory: data.productCategory,
        country: "US",
        region: data.region,
        isCompound: false,
        priority: 1,
        effectiveFrom: data.effectiveFrom,
        effectiveTo: data.effectiveTo || undefined,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.taxRules.all })
      toast({ title: "Tax rule created" })
      router.push("/billing/tax-rules")
    },
    onError: () => {
      toast({ title: "Failed to create tax rule", variant: "destructive" })
    },
  })

  return (
    <FormPageLayout title="New Tax Rule" backHref="/billing/tax-rules" onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Tax Rule Details">
        <FormField label="Name" error={errors.name} registration={register("name")} placeholder="e.g. VAT Standard" required />
        <FormField label="Rate (%)" error={errors.rate} registration={register("rate")} type="text" placeholder="e.g. 20" required />
        <FormSelectField label="Region" error={errors.region} options={regionOptions} value={watch("region")} onValueChange={(v) => setValue("region", v)} required placeholder="Select region" />
        <FormField label="Product Category" error={errors.productCategory} registration={register("productCategory")} placeholder="e.g. TELECOM" required />
        <FormField label="Effective From" error={errors.effectiveFrom} registration={register("effectiveFrom")} type="date" required />
        <FormField label="Effective To" error={errors.effectiveTo} registration={register("effectiveTo")} type="date" />
      </FormSection>
      <FormActions backHref="/billing/tax-rules" loading={mutation.isPending} />
    </FormPageLayout>
  )
}
