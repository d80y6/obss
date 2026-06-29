"use client"

import { useParams, useRouter } from "next/navigation"
import { useState } from "react"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useOrder } from "@/api/hooks/useOrder"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { AuditEntryDto } from "@/types/api"
import { formatCurrency } from "@/lib/formatters"
import { toast } from "@/components/ui/toast"

export default function OrderDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()

  const [showCancelConfirm, setShowCancelConfirm] = useState(false)
  const [cancelReason, setCancelReason] = useState("")

  const { data: order, isLoading } = useOrder(id)

  const cancelMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/orders/orders/${id}/cancel`, { reason: cancelReason || "Customer request" })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(id) })
      toast({ title: "Order cancelled", description: "Order has been cancelled." })
      setShowCancelConfirm(false)
      setCancelReason("")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to cancel order.", variant: "destructive" })
    },
  })

  const { data: auditEntries } = useQuery({
    queryKey: queryKeys.audit.entity("Order", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/Order/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Order Details"
          loading={isLoading}
          fields={[
            { label: "Order #", value: order?.orderNumber ?? "-" },
            { label: "Customer", value: order?.customerName ?? "-" },
            { label: "Offer", value: "-" },
            { label: "Items", value: order ? String(order.items?.length ?? 0) : "-" },
            { label: "Total", value: order ? formatCurrency(order.grandTotal, order.currency) : "-" },
            { label: "Status", value: order ? <StatusBadge status={order.status} /> : "-" },
            { label: "Order Date", value: order?.orderDate ? new Date(order.orderDate).toLocaleDateString() : "-" },
            { label: "Notes", value: order?.notes || "-" },
            { label: "Created", value: order?.createdAt ? new Date(order.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Audit Trail</CardTitle>
          </CardHeader>
          <CardContent>
            {(auditEntries ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No audit entries found.</p>
            ) : (
              auditEntries?.map((entry) => (
                <div key={entry.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{entry.action}</span>
                    <span className="text-sm text-muted-foreground">
                      {new Date(entry.performedAt).toLocaleString()}
                    </span>
                  </div>
                  <p className="text-sm text-muted-foreground">By: {entry.performedByName}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={`Order ${order?.orderNumber ?? ""}`}
        subtitle={order?.customerName}
        status={order?.status}
        backHref="/orders"
        editHref={`/orders/${id}/edit`}
        loading={isLoading}
      />

      <div className="flex gap-2">
        <Button variant="outline" size="sm" onClick={() => router.push(`/orders/${id}/tracking`)}>
          View Tracking
        </Button>
        {order?.status !== "CANCELLED" && order?.status !== "COMPLETED" && (
          <Button
            variant="destructive"
            size="sm"
            onClick={() => setShowCancelConfirm(true)}
            disabled={cancelMutation.isPending}
          >
            {cancelMutation.isPending ? "Cancelling..." : "Cancel Order"}
          </Button>
        )}
      </div>

      {showCancelConfirm && (
        <Card>
          <CardContent className="pt-6 space-y-3">
            <p className="text-sm">Are you sure you want to cancel this order? This action cannot be undone.</p>
            <Input
              placeholder="Reason for cancellation"
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
            />
            <div className="flex gap-2">
              <Button variant="destructive" size="sm" onClick={() => cancelMutation.mutate()} disabled={cancelMutation.isPending}>
                {cancelMutation.isPending ? "Cancelling..." : "Confirm Cancel"}
              </Button>
              <Button variant="outline" size="sm" onClick={() => { setShowCancelConfirm(false); setCancelReason("") }}>Keep Order</Button>
            </div>
          </CardContent>
        </Card>
      )}

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
