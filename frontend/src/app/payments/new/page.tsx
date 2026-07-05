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
import { RecordPaymentCommand } from "@/api/generated"
import { useCustomers } from "@/api/hooks/useCustomers"
import { useInvoices } from "@/api/hooks/useInvoices"
import { useCreatePayment } from "@/api/hooks/useCreatePayment"

const schema = z.object({
  customerId: z.string().min(1, "Customer is required"),
  invoiceId: z.string().min(1, "Invoice is required"),
  amount: z.string().min(1, "Amount is required"),
  method: z.string().min(1, "Payment method is required"),
  reference: z.string().optional(),
  notes: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const methodOptions = [
  { label: "Cash", value: "CASH" },
  { label: "Check", value: "CHECK" },
  { label: "Credit Card", value: "CREDIT_CARD" },
  { label: "Debit Card", value: "DEBIT_CARD" },
  { label: "Bank Transfer", value: "BANK_TRANSFER" },
]

export default function NewPaymentPage() {
  const router = useRouter()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const { data: customersData } = useCustomers({ pageSize: "1000" })

  const { data: invoicesData } = useInvoices({ pageSize: "1000", status: "SENT" })

  const mutation = useCreatePayment()

  const customerOptions = (customersData?.items ?? []).map((c) => ({
    label: `${c.displayName} (${c.email})`,
    value: c.id,
  }))

  const invoiceOptions = (invoicesData?.items ?? []).map((inv) => ({
    label: `${inv.invoiceNumber} - ${inv.customerName} (${inv.currency ?? ""} ${(inv.totalAmount ?? 0).toLocaleString()})`,
    value: inv.id,
  }))

  return (
    <FormPageLayout title="Record Payment" backHref="/payments" onSubmit={handleSubmit((data) => {
      const payload: RecordPaymentCommand = {
        customerId: data.customerId,
        amount: parseFloat(data.amount),
        currency: "USD",
        paymentMethod: data.method,
        paymentReference: data.reference ?? null,
        invoiceId: data.invoiceId ?? null,
        notes: data.notes ?? null,
      }
      mutation.mutate(payload, {
        onSuccess: () => {
          toast({ title: "Payment recorded" })
          router.push("/payments")
        },
        onError: () => {
          toast({ title: "Failed to record payment", variant: "destructive" })
        },
      })
    })}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Payment Details">
        <FormSelectField label="Customer" error={errors.customerId} options={customerOptions} value={watch("customerId")} onValueChange={(v) => setValue("customerId", v)} required placeholder="Select customer" />
        <FormSelectField label="Invoice" error={errors.invoiceId} options={invoiceOptions} value={watch("invoiceId")} onValueChange={(v) => setValue("invoiceId", v)} required placeholder="Select invoice" />
        <FormField label="Amount" error={errors.amount} registration={register("amount")} type="text" placeholder="0.00" required />
        <FormSelectField label="Payment Method" error={errors.method} options={methodOptions} value={watch("method")} onValueChange={(v) => setValue("method", v)} required placeholder="Select method" />
        <FormField label="Reference" error={errors.reference} registration={register("reference")} placeholder="Check # or transaction ref" />
        <FormField label="Notes" error={errors.notes} registration={register("notes")} placeholder="Optional notes" />
      </FormSection>
      <FormActions backHref="/payments" loading={mutation.isPending} submitLabel="Record Payment" />
    </FormPageLayout>
  )
}
