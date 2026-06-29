"use client"

import { useParams, useRouter } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { z } from "zod"
import { FormPageLayout } from "@/forms/FormPageLayout"
import { FormSection } from "@/forms/FormSection"
import { FormField } from "@/forms/FormField"
import { FormActions } from "@/forms/FormActions"
import { FormErrorSummary } from "@/forms/FormErrorSummary"
import { toast } from "@/components/ui/toast"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorFallback } from "@/components/shared/ErrorFallback"
import { useOrder } from "@/api/hooks/useOrder"
import api from "@/services/api"
import type { OrderDto } from "@/api/generated"
import { useEffect } from "react"

const orderSchema = z.object({
  quantity: z.string().min(1, "Quantity is required"),
  notes: z.string().optional(),
})

type OrderForm = z.infer<typeof orderSchema>

export default function EditOrderPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: order, isLoading, error } = useOrder(id)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<OrderForm>({
    resolver: zodResolver(orderSchema),
  })

  useEffect(() => {
    if (order) {
      const firstItem = order.items?.[0]
      reset({ quantity: String(firstItem?.quantity ?? 1), notes: order.notes ?? "" })
    }
  }, [order, reset])

  const updateMutation = useMutation({
    mutationFn: async (data: OrderForm) => {
      const firstItem = order?.items?.[0]
      if (!firstItem) throw new Error("No items to update")
      const res = await api.post<OrderDto>(`/api/v1/orders/orders/${id}/items`, {
        productId: firstItem.productId,
        offerId: firstItem.offerId,
        productName: firstItem.productName,
        offerName: firstItem.offerName,
        quantity: parseInt(data.quantity),
        unitPrice: firstItem.unitPrice,
        recurringPrice: firstItem.recurringPrice,
        discountAmount: firstItem.discountAmount,
        taxAmount: firstItem.taxAmount,
        billingPeriod: firstItem.billingPeriod,
      })
      return res.data
    },
    onSuccess: () => {
      toast({ title: "Order updated", description: "Order has been updated." })
      router.push(`/orders/${id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to update order.", variant: "destructive" })
    },
  })

  const onSubmit = (data: OrderForm) => {
    updateMutation.mutate(data)
  }

  if (isLoading) return <div className="flex-1 p-6"><LoadingState rows={3} /></div>
  if (error || !order) return <div className="flex-1 p-6"><ErrorFallback message="Failed to load order" /></div>

  return (
    <FormPageLayout title="Edit Order" backHref={`/orders/${id}`} onSubmit={handleSubmit(onSubmit)}>
      <FormErrorSummary errors={errors} />
      <FormSection title="Order Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Quantity"
            required
            error={errors.quantity}
            registration={register("quantity")}
            type="number"
            placeholder="1"
          />
        </div>
      </FormSection>
      <FormActions backHref={`/orders/${id}`} loading={updateMutation.isPending} submitLabel="Update Quantity" />
    </FormPageLayout>
  )
}
