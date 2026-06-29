"use client"
import { useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { RecordPaymentCommand, CustomerDto, InvoiceDto, PaymentDto } from "@/api/generated"

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
  const queryClient = useQueryClient()
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const { data: customers } = useQuery({
    queryKey: queryKeys.customers.list({ pageSize: "1000" }),
    queryFn: async () => {
      const res = await api.get("/api/v1/crm/customers?pageSize=1000")
      return res.data as CustomerDto[]
    },
  })

  const { data: invoices } = useQuery({
    queryKey: queryKeys.invoices.list({ pageSize: "1000", status: "SENT" }),
    queryFn: async () => {
      const res = await api.get("/api/v1/invoices/invoices?pageSize=1000&status=SENT")
      return res.data as InvoiceDto[]
    },
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const payload: RecordPaymentCommand = {
        customerId: data.customerId,
        amount: parseFloat(data.amount),
        currency: "USD",
        paymentMethod: data.method,
        paymentReference: data.reference ?? null,
        invoiceId: data.invoiceId ?? null,
        notes: data.notes ?? null,
      }
      const res = await api.post("/api/v1/payments/payments", payload)
      return res.data as PaymentDto
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
      toast({ title: "Payment recorded" })
      router.push("/payments")
    },
    onError: () => {
      toast({ title: "Failed to record payment", variant: "destructive" })
    },
  })

  const customerOptions = (customers ?? []).map((c) => ({
    label: `${c.displayName} (${c.email})`,
    value: c.id,
  }))

  const invoiceOptions = (invoices ?? []).map((inv) => ({
    label: `${inv.invoiceNumber} - ${inv.customerName} (${inv.currency ?? ""} ${(inv.totalAmount ?? 0).toLocaleString()})`,
    value: inv.id,
  }))

  return (
    <FormPageLayout title="Record Payment" backHref="/payments" onSubmit={handleSubmit((data) => mutation.mutate(data))}>
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
