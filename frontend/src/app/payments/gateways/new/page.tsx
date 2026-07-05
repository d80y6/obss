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
import { useCreatePaymentGateway } from "@/api/hooks/useCreatePaymentGateway"

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  provider: z.string().min(1, "Provider is required"),
  configuration: z.string().min(1, "Configuration is required"),
  supportedCurrencies: z.string().min(1, "At least one currency required"),
  transactionFee: z.string().min(1, "Fee is required"),
  feeType: z.string().min(1, "Fee type is required"),
  minAmount: z.string().optional(),
  maxAmount: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const providerOptions = [
  { label: "Stripe", value: "Stripe" },
  { label: "PayPal", value: "PayPal" },
  { label: "Local Bank", value: "LocalBank" },
  { label: "Mobile Money", value: "MobileMoney" },
  { label: "Cash", value: "Cash" },
]

const feeTypeOptions = [
  { label: "Fixed", value: "Fixed" },
  { label: "Percentage", value: "Percentage" },
]

export default function RegisterGatewayPage() {
  const router = useRouter()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })
  const mutation = useCreatePaymentGateway()

  return (
    <FormPageLayout title="Register Payment Gateway" backHref="/payments/gateways" onSubmit={handleSubmit((data) => {
      mutation.mutate({
        name: data.name,
        provider: data.provider,
        configuration: data.configuration,
        supportedCurrencies: data.supportedCurrencies.split(",").map((c) => c.trim()),
        transactionFee: parseFloat(data.transactionFee),
        feeType: data.feeType,
        minAmount: data.minAmount ? parseFloat(data.minAmount) : null,
        maxAmount: data.maxAmount ? parseFloat(data.maxAmount) : null,
      }, {
        onSuccess: () => {
          toast({ title: "Gateway registered" })
          router.push("/payments/gateways")
        },
        onError: () => {
          toast({ title: "Failed to register gateway", variant: "destructive" })
        },
      })
    })}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Gateway Details">
        <FormField label="Name" error={errors.name} registration={register("name")} placeholder="e.g. Main Stripe" required />
        <FormSelectField label="Provider" error={errors.provider} options={providerOptions} value={watch("provider")} onValueChange={(v) => setValue("provider", v)} required placeholder="Select provider" />
        <FormField label="Configuration (JSON)" error={errors.configuration} registration={register("configuration")} placeholder='{"apiKey":"..."}' required />
        <FormField label="Supported Currencies (comma-sep)" error={errors.supportedCurrencies} registration={register("supportedCurrencies")} placeholder="USD,EUR,GBP" required />
        <FormField label="Transaction Fee" error={errors.transactionFee} registration={register("transactionFee")} type="text" placeholder="0.00" required />
        <FormSelectField label="Fee Type" error={errors.feeType} options={feeTypeOptions} value={watch("feeType")} onValueChange={(v) => setValue("feeType", v)} required placeholder="Select type" />
        <FormField label="Min Amount (optional)" error={errors.minAmount} registration={register("minAmount")} type="text" placeholder="0.00" />
        <FormField label="Max Amount (optional)" error={errors.maxAmount} registration={register("maxAmount")} type="text" placeholder="0.00" />
      </FormSection>
      <FormActions backHref="/payments/gateways" loading={mutation.isPending} submitLabel="Register Gateway" />
    </FormPageLayout>
  )
}
