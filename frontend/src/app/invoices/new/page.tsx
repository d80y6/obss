"use client"
import { useState } from "react"
import { useRouter } from "next/navigation"
import { useForm, useFieldArray } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField, FormSelectField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { Button } from "@/components/ui/button"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { toast } from "@/components/ui/toast"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { CustomerDto } from "@/api/generated/dto"
import { Plus, Trash2 } from "lucide-react"

const lineItemSchema = z.object({
  description: z.string().min(1, "Description is required"),
  quantity: z.string().min(1, "Quantity is required"),
  unitPrice: z.string().min(1, "Unit price is required"),
})

const schema = z.object({
  customerId: z.string().min(1, "Customer is required"),
  dueDate: z.string().min(1, "Due date is required"),
  notes: z.string().optional(),
  lineItems: z.array(lineItemSchema).min(1, "At least one line item is required"),
})

type FormData = z.infer<typeof schema>

export default function NewInvoicePage() {
  const router = useRouter()
  const queryClient = useQueryClient()
  const [billId] = useState(() => crypto.randomUUID())
  const { register, handleSubmit, control, formState: { errors }, watch, setValue } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { lineItems: [{ description: "", quantity: "1", unitPrice: "0" }] },
  })
  const { fields, append, remove } = useFieldArray({ control, name: "lineItems" })

  const { data: customers } = useQuery({
    queryKey: queryKeys.customers.list({ pageSize: "1000" }),
    queryFn: async () => {
      const res = await api.get("/api/v1/crm/customers?pageSize=1000")
      return res.data as CustomerDto[]
    },
  })

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const customer = customers?.find(c => c.id === data.customerId)
      const payload = {
        billId,
        customerId: data.customerId,
        customerName: customer?.displayName ?? "",
        customerEmail: customer?.email ?? "",
        customerAddress: "",
        currency: customer?.currency ?? "USD",
        dueDate: data.dueDate,
        notes: data.notes ?? null,
        lineItems: data.lineItems.map(item => ({
          description: item.description,
          quantity: parseInt(item.quantity, 10),
          unitPrice: parseFloat(item.unitPrice),
        })),
      }
      const res = await api.post("/api/v1/invoices/invoices", payload)
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.all })
      toast({ title: "Invoice created" })
      router.push(`/invoices/${data.id}`)
    },
    onError: () => {
      toast({ title: "Failed to create invoice", variant: "destructive" })
    },
  })

  const customerOptions = (customers ?? []).map((c) => ({
    label: `${c.displayName} (${c.email})`,
    value: c.id,
  }))

  return (
    <FormPageLayout title="New Invoice" backHref="/invoices" onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Invoice Details">
        <FormSelectField label="Customer" error={errors.customerId} options={customerOptions} value={watch("customerId")} onValueChange={(v) => setValue("customerId", v)} required placeholder="Select customer" />
        <FormField label="Due Date" error={errors.dueDate} registration={register("dueDate")} type="date" required />
        <FormField label="Notes" error={errors.notes} registration={register("notes")} placeholder="Optional notes" />
      </FormSection>
      <FormSection title="Line Items">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Description *</TableHead>
              <TableHead className="w-20">Qty *</TableHead>
              <TableHead className="w-24">Unit Price *</TableHead>
              <TableHead className="w-10"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {fields.map((field, index) => (
              <TableRow key={field.id}>
                <TableCell>
                  <input {...register(`lineItems.${index}.description`)} className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" placeholder="Description" />
                </TableCell>
                <TableCell>
                  <input {...register(`lineItems.${index}.quantity`)} type="text" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" placeholder="1" />
                </TableCell>
                <TableCell>
                  <input {...register(`lineItems.${index}.unitPrice`)} type="text" className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm" placeholder="0.00" />
                </TableCell>
                <TableCell>
                  <Button variant="ghost" size="icon" type="button" onClick={() => remove(index)}><Trash2 className="h-4 w-4" /></Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <Button variant="outline" size="sm" type="button" onClick={() => append({ description: "", quantity: "1", unitPrice: "0" })}>
          <Plus className="mr-1 h-4 w-4" /> Add Line Item
        </Button>
      </FormSection>
      <FormActions backHref="/invoices" loading={mutation.isPending} submitLabel="Create Invoice" />
    </FormPageLayout>
  )
}
