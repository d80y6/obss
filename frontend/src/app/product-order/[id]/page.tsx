"use client"

import { useParams, useRouter } from "next/navigation"
import { useState } from "react"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingState } from "@/components/shared/LoadingState"
import { ErrorBoundary } from "@/components/shared/ErrorBoundary"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useProductOrder } from "@/api/hooks/useProductOrders"
import {
  useAcknowledgeProductOrderItem,
  useStartProductOrderItem,
  useHoldProductOrderItem,
  useResumeProductOrderItem,
  useAssessProductOrderItem,
  useRejectProductOrderItem,
  useCompleteProductOrderItem,
  useFailProductOrderItem,
  useCancelProductOrderItem,
  useAddProductOrderRelationship,
  useRemoveProductOrderRelationship,
  useCreateProductOrderMilestone,
  useUpdateProductOrderMilestone,
  useRemoveProductOrderMilestone,
} from "@/api/hooks/useProductOrders"
import { ItemStateTimeline } from "../_components/ItemStateTimeline"
import { RelationshipsPanel } from "../_components/RelationshipsPanel"
import { MilestonesPanel } from "../_components/MilestonesPanel"
import { useValidateOrder } from "@/api/hooks/useValidateOrder"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { formatCurrency } from "@/lib/formatters"
import { toast } from "@/components/ui/toast"
import { CheckCircle, Send, XCircle, Trash2, FileCheck } from "lucide-react"

export default function OrderDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()

  const [showCancelConfirm, setShowCancelConfirm] = useState(false)
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)
  const [cancelReason, setCancelReason] = useState("")

  const { data: order, isLoading } = useProductOrder(id)

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

  const submitMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/orders/orders/${id}/submit`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(id) })
      toast({ title: "Order submitted", description: "Order has been submitted." })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to submit order.", variant: "destructive" })
    },
  })

  const approveMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/orders/orders/${id}/approve`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.detail(id) })
      toast({ title: "Order approved", description: "Order has been approved." })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to approve order.", variant: "destructive" })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async () => {
      await api.delete(`/api/v1/orders/orders/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.orders.lists() })
      toast({ title: "Order deleted", description: "Order has been deleted." })
      router.push("/orders")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to delete order.", variant: "destructive" })
    },
  })

  const validateMutation = useValidateOrder()

  const acknowledgeItem = useAcknowledgeProductOrderItem()
  const startItem = useStartProductOrderItem()
  const holdItem = useHoldProductOrderItem()
  const resumeItem = useResumeProductOrderItem()
  const assessItem = useAssessProductOrderItem()
  const rejectItem = useRejectProductOrderItem()
  const completeItem = useCompleteProductOrderItem()
  const failItem = useFailProductOrderItem()
  const cancelItem = useCancelProductOrderItem()
  const addRelationship = useAddProductOrderRelationship()
  const removeRelationship = useRemoveProductOrderRelationship()
  const createMilestone = useCreateProductOrderMilestone()
  const updateMilestone = useUpdateProductOrderMilestone()
  const removeMilestone = useRemoveProductOrderMilestone()

  const handleItemTransition = (itemId: string, action: string, data?: Record<string, string>) => {
    const args = { orderId: id, itemId }
    switch (action) {
      case "acknowledge": acknowledgeItem.mutate(args); break
      case "start": startItem.mutate(args); break
      case "hold": holdItem.mutate(args); break
      case "resume": resumeItem.mutate(args); break
      case "assess": assessItem.mutate(args); break
      case "reject": rejectItem.mutate({ ...args, reason: data?.reason || "" }); break
      case "complete": completeItem.mutate(args); break
      case "fail": failItem.mutate({ ...args, error: data?.error || "" }); break
      case "cancel": cancelItem.mutate(args); break
    }
  }

  const { data: auditEntries } = useAuditLog("Order", id)

  const canSubmit = order?.status === "Draft"
  const canApprove = order?.status === "Submitted" || order?.status === "PendingApproval"
  const canCancel = order?.status !== "Cancelled" && order?.status !== "Completed"

  if (isLoading && !order) return <div className="flex-1 p-6"><LoadingState rows={8} /></div>

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          <EntityMetadata
            title="Order Details"
            loading={isLoading}
            fields={[
              { label: "Order #", value: order?.orderNumber ?? "-" },
              { label: "Customer", value: order?.customerName ?? "-" },
              { label: "Order Type", value: order?.orderType ?? "-" },
              { label: "Items", value: order ? String(order.items?.length ?? 0) : "-" },
              { label: "Subtotal", value: order ? formatCurrency(order.subTotal, order.currency) : "-" },
              { label: "Discount", value: order ? formatCurrency(order.discountTotal, order.currency) : "-" },
              { label: "Tax", value: order ? formatCurrency(order.taxTotal, order.currency) : "-" },
              { label: "Total", value: order ? formatCurrency(order.grandTotal, order.currency) : "-" },
              { label: "Status", value: order ? <StatusBadge status={order.status} /> : "-" },
              { label: "Order Date", value: order?.orderDate ? new Date(order.orderDate).toLocaleDateString() : "-" },
              { label: "Description", value: order?.description || "-" },
              { label: "Channel", value: order?.channel || "-" },
              { label: "Priority", value: order?.priority || "-" },
              { label: "Notes", value: order?.notes || "-" },
              { label: "Requested Start", value: order?.requestedStartDate ? new Date(order.requestedStartDate).toLocaleDateString() : "-" },
	      { label: "Requested Completion", value: order?.requestedCompletionDate ? new Date(order.requestedCompletionDate).toLocaleDateString() : "-" },
	      { label: "Expected Completion", value: order?.expectedCompletionDate ? new Date(order.expectedCompletionDate).toLocaleDateString() : "-" },
              { label: "Notification Contact", value: order?.notificationContact || "-" },
              { label: "External ID", value: order?.externalId || "-" },
              { label: "Created", value: order?.createdAt ? new Date(order.createdAt).toLocaleDateString() : "-" },
            ]}
          />

          {(order?.items?.length ?? 0) > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Order Items</CardTitle>
              </CardHeader>
              <CardContent className="p-0">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Product</TableHead>
                      <TableHead>Offer</TableHead>
                      <TableHead className="text-right">Qty</TableHead>
                      <TableHead className="text-right">Unit Price</TableHead>
                      <TableHead className="text-right">Recurring</TableHead>
                      <TableHead className="text-right">Discount</TableHead>
                      <TableHead className="text-right">Tax</TableHead>
                      <TableHead className="text-right">Total</TableHead>
                      <TableHead>Billing</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {order?.items?.map((item) => (
                      <TableRow key={item.id}>
                        <TableCell className="font-medium">{item.productName}</TableCell>
                        <TableCell>{item.offerName}</TableCell>
                        <TableCell className="text-right">{item.quantity}</TableCell>
                        <TableCell className="text-right">{formatCurrency(item.unitPrice, order.currency)}</TableCell>
                        <TableCell className="text-right">{item.recurringPrice > 0 ? formatCurrency(item.recurringPrice, order.currency) : "-"}</TableCell>
                        <TableCell className="text-right">{item.discountAmount > 0 ? formatCurrency(item.discountAmount, order.currency) : "-"}</TableCell>
                        <TableCell className="text-right">{item.taxAmount > 0 ? formatCurrency(item.taxAmount, order.currency) : "-"}</TableCell>
                        <TableCell className="text-right font-medium">{formatCurrency(item.totalPrice, order.currency)}</TableCell>
                        <TableCell>{item.billingPeriod}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          )}

          {(order?.payments?.length ?? 0) > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Payments</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Method</TableHead>
                      <TableHead className="text-right">Amount</TableHead>
                      <TableHead>Reference</TableHead>
                      <TableHead>Date</TableHead>
                      <TableHead>Status</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {order?.payments?.map((p) => (
                      <TableRow key={p.id}>
                        <TableCell>{p.paymentMethod}</TableCell>
                        <TableCell className="text-right">{formatCurrency(p.amount, order.currency)}</TableCell>
                        <TableCell>{p.paymentReference}</TableCell>
                        <TableCell>{new Date(p.paidAt).toLocaleDateString()}</TableCell>
                        <TableCell><StatusBadge status={p.status} /></TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          )}
        </div>
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
    <ErrorBoundary>
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={`Order ${order?.orderNumber ?? ""}`}
        subtitle={order?.customerName}
        status={order?.status}
        backHref="/orders"
        editHref={`/orders/${id}/edit`}
        loading={isLoading}
      />

      <div className="flex gap-2 flex-wrap">
        {canSubmit && (
          <Button
            size="sm"
            onClick={() => submitMutation.mutate()}
            disabled={submitMutation.isPending}
          >
            <Send className="h-4 w-4 mr-1" />
            {submitMutation.isPending ? "Submitting..." : "Submit"}
          </Button>
        )}
        {canApprove && (
          <Button
            size="sm"
            variant="default"
            onClick={() => approveMutation.mutate()}
            disabled={approveMutation.isPending}
          >
            <CheckCircle className="h-4 w-4 mr-1" />
            {approveMutation.isPending ? "Approving..." : "Approve"}
          </Button>
        )}
        <Button variant="outline" size="sm" onClick={() => router.push(`/orders/${id}/tracking`)}>
          View Tracking
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={() => validateMutation.mutate(id)}
          disabled={validateMutation.isPending}
        >
          <FileCheck className="h-4 w-4 mr-1" />
          {validateMutation.isPending ? "Validating..." : "Validate"}
        </Button>
        {canCancel && (
          <Button
            variant="destructive"
            size="sm"
            onClick={() => setShowCancelConfirm(true)}
            disabled={cancelMutation.isPending}
          >
            <XCircle className="h-4 w-4 mr-1" />
            {cancelMutation.isPending ? "Cancelling..." : "Cancel Order"}
          </Button>
        )}
        {order?.status === "Draft" && (
          <Button
            variant="destructive"
            size="sm"
            onClick={() => setShowDeleteConfirm(true)}
            disabled={deleteMutation.isPending}
          >
            <Trash2 className="h-4 w-4 mr-1" />
            {deleteMutation.isPending ? "Deleting..." : "Delete Order"}
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

      {showDeleteConfirm && (
        <Card>
          <CardContent className="pt-6 space-y-3">
            <p className="text-sm">Are you sure you want to delete this order? This action cannot be undone.</p>
            <div className="flex gap-2">
              <Button variant="destructive" size="sm" onClick={() => deleteMutation.mutate()} disabled={deleteMutation.isPending}>
                {deleteMutation.isPending ? "Deleting..." : "Confirm Delete"}
              </Button>
              <Button variant="outline" size="sm" onClick={() => setShowDeleteConfirm(false)}>Keep Order</Button>
            </div>
          </CardContent>
        </Card>
      )}

      <EntityTabs tabs={tabs} defaultTab="overview" />

      {(order?.items || order?.milestones || order?.itemRelationships) && (
        <div className="space-y-6">
          <ItemStateTimeline
            items={order?.items || []}
            onTransition={handleItemTransition}
          />
          <RelationshipsPanel
            relationships={order?.itemRelationships || []}
            onAdd={(itemId, targetItemId, type) => addRelationship.mutate({ orderId: id, itemId, targetItemId, type })}
            onRemove={(relId) => removeRelationship.mutate({ orderId: id, relationshipId: relId })}
          />
          <MilestonesPanel
            milestones={order?.milestones || []}
            onAdd={(name, desc, date) => createMilestone.mutate({ orderId: id, name, description: desc, milestoneDate: date })}
            onUpdate={(milestoneId, status) => updateMilestone.mutate({ orderId: id, milestoneId, status, milestoneDate: new Date().toISOString() })}
            onRemove={(milestoneId) => removeMilestone.mutate({ orderId: id, milestoneId })}
          />
        </div>
      )}
    </div>
    </ErrorBoundary>
  )
}
