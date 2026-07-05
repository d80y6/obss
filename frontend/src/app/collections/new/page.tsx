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
import type { CustomerDto } from "@/api/generated"

const schema = z.object({
  customerId: z.string().min(1, "Customer is required"),
  assignedAgent: z.string().optional(),
  priority: z.string().min(1, "Priority is required"),
  notes: z.string().optional(),
})

type FormData = z.infer<typeof schema>

const priorityOptions = [
  { label: "Low", value: "LOW" },
  { label: "Medium", value: "MEDIUM" },
  { label: "High", value: "HIGH" },
  { label: "Urgent", value: "URGENT" },
]

export default function NewCollectionCasePage() {
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

  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      const res = await api.post("/api/v1/collections/cases", {
        customerId: data.customerId,
        customerName: "",
        totalOverdueAmount: 0,
        currency: "USD",
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.all })
      toast({ title: "Collection case created" })
      router.push("/collections")
    },
    onError: () => {
      toast({ title: "Failed to create case", variant: "destructive" })
    },
  })

  const customerOptions = (customers ?? []).map((c) => ({
    label: `${c.displayName} (${c.email})`,
    value: c.id,
  }))

  return (
    <FormPageLayout title="New Collection Case" backHref="/collections" onSubmit={handleSubmit((data) => mutation.mutate(data))}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Case Details">
        <FormSelectField label="Customer" error={errors.customerId} options={customerOptions} value={watch("customerId")} onValueChange={(v) => setValue("customerId", v)} required placeholder="Select customer" />
        <FormField label="Assigned Agent" error={errors.assignedAgent} registration={register("assignedAgent")} placeholder="Agent name or ID" />
        <FormSelectField label="Priority" error={errors.priority} options={priorityOptions} value={watch("priority")} onValueChange={(v) => setValue("priority", v)} required placeholder="Select priority" />
        <FormField label="Notes" error={errors.notes} registration={register("notes")} placeholder="Initial notes" />
      </FormSection>
      <FormActions backHref="/collections" loading={mutation.isPending} submitLabel="Create Case" />
    </FormPageLayout>
  )
}
