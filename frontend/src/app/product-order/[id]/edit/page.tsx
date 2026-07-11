"use client"

import { useParams, useRouter } from "next/navigation"
import { useMemo, useState } from "react"
import { useMutation, useQueryClient } from "@tanstack/react-query"
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
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useEffect } from "react"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { queryKeys } from "@/lib/query-keys"
import { formatCurrency } from "@/lib/formatters"
import { Trash2 } from "lucide-react"

const orderSchema = z.object({
  description: z.string().optional(),
  channel: z.string().optional(),
  priority: z.string().optional(),
  notes: z.string().optional(),
})

type OrderForm = z.infer<typeof orderSchema>

export default function EditOrderPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()

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
      reset({
        description: order.description ?? "",
        channel: order.channel ?? "",
        priority: order.priority ?? "",
        notes: order.notes ?? "",
      })
    }
  }, [order, reset])

  const initialQuantities = useMemo(() => {
    const q: Record<string, string> = {}
    if (order?.items) {
      order.items.forEach((item) => {
        q[item.id] = String(item.quantity)
      })
    }
    return q
  }, [order])

  const [itemQuantities, setItemQuantities] = useState<Record<string, string>>(initialQuantities)

  const updateMutation = useMutation({
    mutationFn: async (data: OrderForm) => {
      await api.patch<OrderDto>(`/api/v1/orders/orders/${id}`, {
        id,
        description: data.description || null,
        channel: data.channel || null,
        priority: data.priority || null,
        notes: data.notes || null,
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(id) })
      toast({ title: "Order updated", description: "Order has been updated." })
      router.push(`/orders/${id}`)
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to update order.", variant: "destructive" })
    },
  })

  const removeItemMutation = useMutation({
    mutationFn: async (itemId: string) => {
      await api.delete(`/api/v1/orders/orders/${id}/items/${itemId}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(id) })
      toast({ title: "Item removed", description: "Item has been removed from the order." })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to remove item.", variant: "destructive" })
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

      {order.status !== "Draft" && (
        <Card className="border-amber-200 bg-amber-50">
          <CardContent className="pt-6">
            <p className="text-sm text-amber-800">
              Only Draft orders can be edited. This order is currently <strong>{order.status}</strong>.
            </p>
          </CardContent>
        </Card>
      )}

      {(order.items?.length ?? 0) > 0 && (
        <FormSection title="Order Items">
          <Card>
            <CardContent className="p-0">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Product</TableHead>
                    <TableHead className="text-right">Qty</TableHead>
                    <TableHead className="text-right">Unit Price</TableHead>
                    <TableHead className="text-right">Total</TableHead>
                    <TableHead>Billing</TableHead>
                    <TableHead className="w-12"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {order.items?.map((item) => (
                    <TableRow key={item.id}>
                      <TableCell className="font-medium">{item.productName}</TableCell>
                      <TableCell className="text-right">
                        {item.id === order.items[0].id ? (
                          <Input
                            type="number"
                            className="w-20 h-8 text-right inline-block"
                            value={itemQuantities[item.id] ?? item.quantity}
                            onChange={(e) => setItemQuantities((prev) => ({ ...prev, [item.id]: e.target.value }))}
                            min={1}
                          />
                        ) : (
                          <span>{item.quantity}</span>
                        )}
                      </TableCell>
                      <TableCell className="text-right">{formatCurrency(item.unitPrice, order.currency)}</TableCell>
                      <TableCell className="text-right font-medium">{formatCurrency(item.totalPrice, order.currency)}</TableCell>
                      <TableCell>{item.billingPeriod}</TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8"
                          onClick={() => removeItemMutation.mutate(item.id)}
                          disabled={removeItemMutation.isPending}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </FormSection>
      )}

      <FormSection title="Order Information">
        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            label="Description"
            error={errors.description}
            registration={register("description")}
            placeholder="Order description..."
          />
          <FormField
            label="Channel"
            error={errors.channel}
            registration={register("channel")}
            placeholder="e.g. Web, Phone, Store"
          />
          <FormField
            label="Priority"
            error={errors.priority}
            registration={register("priority")}
            placeholder="e.g. High, Medium, Low"
          />
          <FormField
            label="Notes"
            error={errors.notes}
            registration={register("notes")}
            placeholder="Order notes..."
          />
        </div>
      </FormSection>

      <FormActions backHref={`/orders/${id}`} loading={updateMutation.isPending} submitLabel="Save Changes" />
    </FormPageLayout>
  )
}
