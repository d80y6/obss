"use client"

import { useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useBill } from "@/api/hooks/useBill"
import { useFinalizeBill } from "@/api/hooks/useFinalizeBill"
import { useAddBillAdjustment } from "@/api/hooks/useAddBillAdjustment"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { formatCurrency } from "@/lib/formatters"
import { toast } from "@/components/ui/toast"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { FileText, Plus } from "lucide-react"

export default function BillDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string
  const queryClient = useQueryClient()
  const [adjustmentOpen, setAdjustmentOpen] = useState(false)
  const [adjAmount, setAdjAmount] = useState("")
  const [adjReason, setAdjReason] = useState("")

  const { data: bill, isLoading } = useBill(id)

  const { data: auditEntries, isLoading: auditLoading } = useAuditLog("Bill", id)

  const finalizeMutation = useFinalizeBill()

  const generateInvoiceMutation = useMutation({
    mutationFn: async () => {
      const res = await api.post("/api/v1/invoices/invoices", { billId: id })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.bills.detail(id) })
      toast({ title: "Invoice generated" })
      router.push(`/invoices/${data.id}`)
    },
    onError: () => {
      toast({ title: "Failed to generate invoice", variant: "destructive" })
    },
  })

  const addAdjustmentMutation = useAddBillAdjustment(id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          <EntityMetadata
            title="Bill Details"
            loading={isLoading}
            fields={[
              { label: "Bill #", value: bill?.billNumber ?? "-" },
              { label: "Customer", value: bill?.customerName ?? "-" },
              { label: "Period", value: bill?.period ?? "-" },
              { label: "Status", value: bill ? <StatusBadge status={bill.status} /> : "-" },
              { label: "Total", value: bill ? formatCurrency(bill.totalAmount, bill.currency) : "-" },
              { label: "Issue Date", value: bill?.issueDate ? new Date(bill.issueDate).toLocaleDateString() : "-" },
              { label: "Due Date", value: bill?.dueDate ? new Date(bill.dueDate).toLocaleDateString() : "-" },
              { label: "Created", value: bill?.createdAt ? new Date(bill.createdAt).toLocaleDateString() : "-" },
            ]}
          />
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-base">Adjustments</CardTitle>
              <Dialog open={adjustmentOpen} onOpenChange={setAdjustmentOpen}>
                <DialogTrigger asChild>
                  <Button variant="outline" size="sm"><Plus className="mr-1 h-4 w-4" /> Add Adjustment</Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Add Adjustment</DialogTitle>
                  </DialogHeader>
                  <div className="space-y-4">
                    <div className="space-y-2">
                      <Label>Amount</Label>
                      <Input type="number" value={adjAmount} onChange={(e) => setAdjAmount(e.target.value)} placeholder="0.00" />
                    </div>
                    <div className="space-y-2">
                      <Label>Description</Label>
                      <Input value={adjReason} onChange={(e) => setAdjReason(e.target.value)} placeholder="e.g. Discount" />
                    </div>
                    <Button onClick={() => addAdjustmentMutation.mutate({
                      amount: parseFloat(adjAmount),
                      description: adjReason,
                      currency: "USD",
                    }, {
                      onSuccess: () => {
                        toast({ title: "Adjustment added" })
                        setAdjustmentOpen(false)
                        setAdjAmount("")
                        setAdjReason("")
                      },
                      onError: () => toast({ title: "Failed to add adjustment", variant: "destructive" }),
                    })} disabled={!adjAmount || !adjReason || addAdjustmentMutation.isPending}>
                      {addAdjustmentMutation.isPending ? "Adding..." : "Add"}
                    </Button>
                  </div>
                </DialogContent>
              </Dialog>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">Adjustment history is not available via API.</p>
            </CardContent>
          </Card>
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
            {auditLoading ? (
              <p className="text-sm text-muted-foreground">Loading...</p>
            ) : (auditEntries ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No audit entries.</p>
            ) : (
              auditEntries?.map((entry) => (
                <div key={entry.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{entry.action}</span>
                    <span className="text-sm text-muted-foreground">{new Date(entry.performedAt).toLocaleString()}</span>
                  </div>
                  <p className="text-sm text-muted-foreground">By: {entry.performedByName}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
    {
      id: "actions",
      label: "Actions",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Bill Actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Button variant="default" size="sm" onClick={() => finalizeMutation.mutate(id, {
              onSuccess: () => toast({ title: "Bill finalized" }),
              onError: () => toast({ title: "Failed to finalize bill", variant: "destructive" }),
            })} disabled={finalizeMutation.isPending}>
              <FileText className="mr-1 h-4 w-4" /> {finalizeMutation.isPending ? "Finalizing..." : "Finalize"}
            </Button>
            <Button variant="outline" size="sm" onClick={() => generateInvoiceMutation.mutate()} disabled={generateInvoiceMutation.isPending}>
              <Plus className="mr-1 h-4 w-4" /> {generateInvoiceMutation.isPending ? "Generating..." : "Generate Invoice"}
            </Button>
            <Button variant="outline" size="sm" onClick={() => setAdjustmentOpen(true)}>
              <Plus className="mr-1 h-4 w-4" /> Add Adjustment
            </Button>
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={`Bill ${bill?.billNumber ?? ""}`}
        subtitle={bill?.customerName}
        status={bill?.status}
        backHref="/billing"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
