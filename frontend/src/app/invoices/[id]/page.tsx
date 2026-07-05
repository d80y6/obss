"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useInvoice } from "@/api/hooks/useInvoice"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import type { DisputeDto, CreditNoteDto } from "@/api/generated"
import { formatCurrency } from "@/lib/formatters"
import { toast } from "@/components/ui/toast"
import Link from "next/link"
import { FileText, Send, XCircle, FileDown, FileX } from "lucide-react"

export default function InvoiceDetailPage() {
  const params = useParams()
  const id = params.id as string
  const queryClient = useQueryClient()

  const { data: invoice, isLoading } = useInvoice(id)

  const { data: disputes } = useQuery({
    queryKey: queryKeys.invoices.disputes.list({ invoiceId: id }),
    queryFn: async () => {
      const res = await api.get(`/api/v1/invoices/invoices/${id}/disputes`)
      return res.data as DisputeDto[]
    },
    enabled: !!id,
  })

  const { data: creditNotes } = useQuery({
    queryKey: queryKeys.invoices.creditNotes.list({ invoiceId: id }),
    queryFn: async () => {
      const res = await api.get(`/api/v1/invoices/invoices/${id}/credit-notes`)
      return res.data as CreditNoteDto[]
    },
    enabled: !!id,
  })

  const { data: auditEntries } = useAuditLog("Invoice", id)

  const finalizeMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/invoices/invoices/${id}/finalize`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.detail(id) })
      toast({ title: "Invoice finalized" })
    },
    onError: () => {
      toast({ title: "Failed to finalize invoice", variant: "destructive" })
    },
  })

  const sendMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/invoices/invoices/${id}/send`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.detail(id) })
      toast({ title: "Invoice sent" })
    },
    onError: () => {
      toast({ title: "Failed to send invoice", variant: "destructive" })
    },
  })

  const cancelMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/invoices/invoices/${id}/cancel`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.detail(id) })
      toast({ title: "Invoice cancelled" })
    },
    onError: () => {
      toast({ title: "Failed to cancel invoice", variant: "destructive" })
    },
  })

  const canFinalize = invoice && (invoice.status === "DRAFT")
  const canSend = invoice && (invoice.status === "FINALIZED")
  const canCancel = invoice && (invoice.status !== "CANCELLED" && invoice.status !== "PAID")

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          <EntityMetadata
            title="Invoice Details"
            loading={isLoading}
            fields={[
              { label: "Invoice #", value: invoice?.invoiceNumber ?? "-" },
              { label: "Customer", value: invoice?.customerName ?? "-" },
              { label: "Status", value: invoice ? <StatusBadge status={invoice.status} /> : "-" },
              { label: "Total", value: invoice ? formatCurrency(invoice.totalAmount, invoice.currency) : "-" },
              { label: "Issue Date", value: invoice?.issueDate ? new Date(invoice.issueDate).toLocaleDateString() : "-" },
              { label: "Due Date", value: invoice?.dueDate ? new Date(invoice.dueDate).toLocaleDateString() : "-" },
            ]}
          />
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Line Items</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Description</TableHead>
                    <TableHead className="text-right">Qty</TableHead>
                    <TableHead className="text-right">Unit Price</TableHead>
                    <TableHead className="text-right">Total</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {(invoice?.lineItems ?? []).length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={4} className="text-center text-muted-foreground">No line items.</TableCell>
                    </TableRow>
                  ) : (
                    invoice?.lineItems.map((item, i) => (
                      <TableRow key={i}>
                        <TableCell>{item.description}</TableCell>
                        <TableCell className="text-right">{item.quantity}</TableCell>
                        <TableCell className="text-right">{formatCurrency(item.unitPrice)}</TableCell>
                        <TableCell className="text-right">{formatCurrency(item.totalPrice)}</TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>
      ),
    },
    {
      id: "disputes",
      label: `Disputes (${(disputes ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Disputes</CardTitle>
          </CardHeader>
          <CardContent>
            {!disputes || disputes.length === 0 ? (
              <p className="text-sm text-muted-foreground">No disputes.</p>
            ) : (
              disputes.map((d) => (
                <div key={d.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{d.reason}</span>
                    <StatusBadge status={d.status} />
                  </div>
                  <p className="text-sm text-muted-foreground">Amount: {formatCurrency(d.amount)}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
    {
      id: "creditNotes",
      label: `Credit Notes (${(creditNotes ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Credit Notes</CardTitle>
          </CardHeader>
          <CardContent>
            {!creditNotes || creditNotes.length === 0 ? (
              <p className="text-sm text-muted-foreground">No credit notes.</p>
            ) : (
              creditNotes.map((cn) => (
                <div key={cn.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{cn.creditNoteNumber}</span>
                    <StatusBadge status={cn.status} />
                  </div>
                  <p className="text-sm text-muted-foreground">Amount: {formatCurrency(cn.amount)}</p>
                  <p className="text-sm text-muted-foreground">Reason: {cn.reason}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
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
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={`Invoice ${invoice?.invoiceNumber ?? ""}`}
        subtitle={invoice?.customerName}
        status={invoice?.status}
        backHref="/invoices"
        loading={isLoading}
      />
      {invoice && (
        <div className="flex flex-wrap gap-2">
          {canFinalize && (
            <Button variant="default" size="sm" onClick={() => finalizeMutation.mutate()}>
              <FileText className="mr-1 h-4 w-4" /> Finalize
            </Button>
          )}
          {canSend && (
            <Button variant="default" size="sm" onClick={() => sendMutation.mutate()}>
              <Send className="mr-1 h-4 w-4" /> Send
            </Button>
          )}
          {canCancel && (
            <Button variant="destructive" size="sm" onClick={() => cancelMutation.mutate()}>
              <XCircle className="mr-1 h-4 w-4" /> Cancel
            </Button>
          )}
          {invoice.status !== "DRAFT" && (
            <Button variant="outline" size="sm" asChild>
              <Link href={`/api/v1/invoices/invoices/${id}/pdf`} target="_blank">
                <FileDown className="mr-1 h-4 w-4" /> Download PDF
              </Link>
            </Button>
          )}
          <Button variant="outline" size="sm" asChild>
            <Link href={`/invoices/${id}/credit-notes/new`}>
              <FileX className="mr-1 h-4 w-4" /> Issue Credit Note
            </Link>
          </Button>
        </div>
      )}
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
