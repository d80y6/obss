"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
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
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useReconciliation } from "@/api/hooks/useReconciliation"
import type { ReconciliationDto, PaymentDto } from "@/api/generated"
import { ArrowLeftRight, Upload, Wand2, CheckCircle } from "lucide-react"
import { toast } from "@/components/ui/toast"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { useRef, ChangeEvent } from "react"

export default function ReconciliationPage() {
  const queryClient = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [matchSelections, setMatchSelections] = useState<Record<string, string>>({})

  const { data: reconciliations, isLoading } = useQuery({
    queryKey: queryKeys.payments.reconciliation.list({}),
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/reconciliation")
      return res.data as ReconciliationDto[]
    },
  })

  const { data: unmatched } = useQuery({
    queryKey: ["payment-reconciliation-unmatched"],
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments/reconciliation/unmatched")
      return res.data as unknown[]
    },
  })

  const { data: payments } = useQuery({
    queryKey: queryKeys.payments.list({ status: "COMPLETED", pageSize: "1000" }),
    queryFn: async () => {
      const res = await api.get("/api/v1/payments/payments?status=COMPLETED&pageSize=1000")
      return res.data as PaymentDto[]
    },
  })

  const importMutation = useMutation({
    mutationFn: async (file: File) => {
      const formData = new FormData()
      formData.append("file", file)
      const res = await api.post("/api/v1/payments/payments/reconciliation/import", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      queryClient.invalidateQueries({ queryKey: ["payment-reconciliation-unmatched"] })
      toast({ title: "Statement imported" })
    },
    onError: () => {
      toast({ title: "Failed to import statement", variant: "destructive" })
    },
  })

  const autoReconcileMutation = useMutation({
    mutationFn: async () => {
      const res = await api.post("/api/v1/payments/payments/reconciliation/auto")
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      queryClient.invalidateQueries({ queryKey: ["payment-reconciliation-unmatched"] })
      toast({ title: "Auto-reconciliation completed" })
    },
    onError: () => {
      toast({ title: "Failed to auto-reconcile", variant: "destructive" })
    },
  })

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      importMutation.mutate(file)
    }
  }

  const matchTransactionMutation = useMutation({
    mutationFn: async ({ transactionId, paymentId }: { transactionId: string; paymentId: string }) => {
      await api.post(`/api/v1/payments/payments/reconciliation/${transactionId}/match`, { paymentId })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["payment-reconciliation-unmatched"] })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      toast({ title: "Transaction matched" })
    },
    onError: () => {
      toast({ title: "Failed to match transaction", variant: "destructive" })
    },
  })

  const reconciliationColumns: Column<ReconciliationDto>[] = [
    { id: "id", header: "ID", cell: (row) => row.id.substring(0, 8) + "..." },
    { id: "statementDate", header: "Statement Date", cell: (row) => new Date(row.statementDate).toLocaleDateString() },
    { id: "totalAmount", header: "Total", cell: (row) => `$${(row.totalAmount ?? 0).toLocaleString()}` },
    { id: "matchedCount", header: "Matched", accessorKey: "matchedCount" },
    { id: "unmatchedCount", header: "Unmatched", accessorKey: "unmatchedCount" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Reconciliation" backHref="/payments" />

      <div className="flex flex-wrap gap-2">
        <Button variant="outline" size="sm" onClick={() => fileInputRef.current?.click()}>
          <Upload className="mr-1 h-4 w-4" /> Import Statement
        </Button>
        <input ref={fileInputRef} type="file" className="hidden" accept=".csv,.xlsx,.xls" onChange={handleFileChange} />
        <Button variant="outline" size="sm" onClick={() => autoReconcileMutation.mutate()} disabled={autoReconcileMutation.isPending}>
          <Wand2 className="mr-1 h-4 w-4" /> Auto-Reconcile
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Unmatched Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          {(!unmatched || (unmatched as unknown[]).length === 0) ? (
            <p className="text-sm text-muted-foreground">No unmatched transactions.</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Transaction</TableHead>
                  <TableHead>Amount</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Match Payment</TableHead>
                  <TableHead></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {(unmatched as Array<{ id: string; amount?: number; date?: string; description?: string }>).map((tx) => (
                  <TableRow key={tx.id}>
                    <TableCell>{tx.description || tx.id.substring(0, 8)}</TableCell>
                    <TableCell>${(tx.amount ?? 0).toLocaleString()}</TableCell>
                    <TableCell>{tx.date ? new Date(tx.date).toLocaleDateString() : "-"}</TableCell>
                    <TableCell>
                      <Select
                        value={matchSelections[tx.id] || ""}
                        onValueChange={(v) => setMatchSelections((prev) => ({ ...prev, [tx.id]: v }))}
                      >
                        <SelectTrigger className="w-64">
                          <SelectValue placeholder="Select payment..." />
                        </SelectTrigger>
                        <SelectContent>
                          {(payments ?? []).map((p) => (
                            <SelectItem key={p.id} value={p.id}>
                              {p.paymentNumber} - {p.customerName} ({p.currency ?? ""} {(p.amount ?? 0).toLocaleString()})
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </TableCell>
                    <TableCell>
                      <Button
                        variant="ghost"
                        size="sm"
                        disabled={!matchSelections[tx.id]}
                        onClick={() =>
                          matchTransactionMutation.mutate({
                            transactionId: tx.id,
                            paymentId: matchSelections[tx.id],
                          })
                        }
                      >
                        <CheckCircle className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Reconciliation History</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={reconciliationColumns}
            data={reconciliations ?? []}
            loading={isLoading}
            emptyTitle="No reconciliation records"
            emptyIcon={ArrowLeftRight}
            rowKey={(row) => row.id}
          />
        </CardContent>
      </Card>
    </div>
  )
}
