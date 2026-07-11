"use client"

import { useState } from "react"
import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Input } from "@/components/ui/input"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { useRecordArrangementPayment, useDunningPolicies } from "@/api/hooks/use-collections"
import type { CollectionCaseDto, CollectionActionDto, PaymentArrangementDto } from "@/api/generated"
import { toast } from "@/components/ui/toast"
import Link from "next/link"
import { CheckCircle, Send, Phone, CreditCard, Landmark } from "lucide-react"

export default function CollectionCaseDetailPage() {
  const params = useParams()
  const id = params.id as string
  const queryClient = useQueryClient()

  const { data: caseData, isLoading } = useQuery({
    queryKey: queryKeys.collections.cases.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/collections/cases/${id}`)
      return res.data as CollectionCaseDto
    },
    enabled: !!id,
  })

  const { data: actions } = useQuery({
    queryKey: queryKeys.collections.cases.actions(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/collections/cases/${id}/actions`)
      return res.data as CollectionActionDto[]
    },
    enabled: !!id,
  })

  const { data: arrangements } = useQuery({
    queryKey: queryKeys.collections.cases.arrangements(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/collections/cases/${id}/arrangements`)
      return res.data as PaymentArrangementDto[]
    },
    enabled: !!id,
  })

  const { data: auditEntries } = useAuditLog("CollectionCase", id)

  const resolveMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/collections/cases/${id}/resolve`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.all })
      toast({ title: "Case resolved" })
    },
    onError: () => {
      toast({ title: "Failed to resolve case", variant: "destructive" })
    },
  })

  const dunningMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/collections/cases/${id}/dunning`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.actions(id) })
      toast({ title: "Dunning notice sent" })
    },
    onError: () => {
      toast({ title: "Failed to send dunning notice", variant: "destructive" })
    },
  })

  const [payingArrangement, setPayingArrangement] = useState<string | null>(null)
  const [paymentAmount, setPaymentAmount] = useState<number>(0)
  const recordPaymentMutation = useRecordArrangementPayment()

  const { data: policies } = useDunningPolicies()
  const maxDunningLevel = policies?.[0]?.maxDunningLevel ?? 5

  const handleRecordPayment = (arrangementId: string) => {
    recordPaymentMutation.mutate(
      { paymentArrangementId: arrangementId, amount: paymentAmount },
      {
        onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.arrangements(id) })
          queryClient.invalidateQueries({ queryKey: queryKeys.collections.cases.detail(id) })
          toast({ title: "Payment recorded" })
          setPayingArrangement(null)
          setPaymentAmount(0)
        },
        onError: () => {
          toast({ title: "Failed to record payment", variant: "destructive" })
        },
      }
    )
  }

  function generateInstallments(arrangement: PaymentArrangementDto) {
    const count = arrangement.installmentCount
    if (!count || count <= 0) return []
    const startDate = arrangement.firstPaymentDate ? new Date(arrangement.firstPaymentDate) : new Date()
    const endDate = arrangement.lastPaymentDate ? new Date(arrangement.lastPaymentDate) : startDate
    const diffMs = endDate.getTime() - startDate.getTime()
    const intervalMs = count > 1 ? diffMs / (count - 1) : 0
    const amountPerInstallment = arrangement.installmentAmount ?? 0
    return Array.from({ length: count }, (_, i) => {
      const dueDate = new Date(startDate.getTime() + i * intervalMs)
      return {
        number: i + 1,
        dueDate: dueDate.toLocaleDateString(),
        amount: amountPerInstallment,
        paidAmount: arrangement.status === "COMPLETED" ? amountPerInstallment : 0,
        status: arrangement.status === "COMPLETED" ? "PAID" : "PENDING",
      }
    })
  }

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          <EntityMetadata
            title="Case Details"
            loading={isLoading}
            fields={[
              { label: "Customer", value: caseData?.customerName ?? "-" },
              { label: "Total Overdue", value: caseData ? `${caseData.currency ?? ""} ${(caseData.totalOverdueAmount ?? 0).toLocaleString()}` : "-" },
              { label: "Status", value: caseData ? <StatusBadge status={caseData.status} /> : "-" },
              { label: "Assigned To", value: caseData?.assignedTo || "-" },
              { label: "Opened", value: caseData?.openedAt ? new Date(caseData.openedAt).toLocaleDateString() : "-" },
            ]}
          />
          <Card>
            <CardHeader>
              <CardTitle className="text-base flex items-center gap-2">
                <Landmark className="h-4 w-4" />
                Dunning Progress
              </CardTitle>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <p className="text-sm text-muted-foreground">Loading...</p>
              ) : (
                <div className="flex items-center gap-3">
                  <span className="text-lg font-bold">
                    Level {caseData?.currentDunningLevel ?? 0} of {maxDunningLevel}
                  </span>
                  <Badge
                    variant={
                      (caseData?.currentDunningLevel ?? 0) <= 1
                        ? "success"
                        : (caseData?.currentDunningLevel ?? 0) <= 3
                          ? "warning"
                          : "destructive"
                    }
                  >
                    {(caseData?.currentDunningLevel ?? 0) <= 1
                      ? "Low"
                      : (caseData?.currentDunningLevel ?? 0) <= 3
                        ? "Medium"
                        : "High"}
                  </Badge>
                </div>
              )}
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Payment Arrangements</CardTitle>
            </CardHeader>
            <CardContent>
              {(!arrangements || arrangements.length === 0) ? (
                <p className="text-sm text-muted-foreground">No payment arrangements.</p>
              ) : (
                arrangements.map((a) => {
                  const installments = generateInstallments(a)
                  return (
                    <div key={a.id} className="border-b py-3 space-y-2">
                      <div className="flex justify-between items-center">
                        <div>
                          <span className="font-medium">{a.installmentCount} installments of {caseData?.currency ?? ""} {(a.installmentAmount ?? 0).toLocaleString()}</span>
                          <span className="text-xs text-muted-foreground ml-2">
                            ({a.frequency})
                          </span>
                        </div>
                        <div className="flex items-center gap-2">
                          <StatusBadge status={a.status} />
                          {a.status === "ACTIVE" && (
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => setPayingArrangement(payingArrangement === a.id ? null : a.id)}
                            >
                              <CreditCard className="mr-1 h-3 w-3" />
                              Record Payment
                            </Button>
                          )}
                        </div>
                      </div>
                      <p className="text-sm text-muted-foreground">
                        {a.firstPaymentDate ? new Date(a.firstPaymentDate).toLocaleDateString() : "-"} to {a.lastPaymentDate ? new Date(a.lastPaymentDate).toLocaleDateString() : "-"}
                      </p>
                      {installments.length > 0 && (
                        <div className="overflow-x-auto mt-2">
                          <table className="w-full text-xs">
                            <thead>
                              <tr className="border-b text-muted-foreground">
                                <th className="text-left py-1 pr-2">#</th>
                                <th className="text-left py-1 pr-2">Due Date</th>
                                <th className="text-right py-1 pr-2">Amount</th>
                                <th className="text-right py-1 pr-2">Paid</th>
                                <th className="text-right py-1">Status</th>
                              </tr>
                            </thead>
                            <tbody>
                              {installments.map((inst) => (
                                <tr key={inst.number} className="border-b last:border-0">
                                  <td className="py-1 pr-2">{inst.number}</td>
                                  <td className="py-1 pr-2">{inst.dueDate}</td>
                                  <td className="py-1 pr-2 text-right">{(inst.amount ?? 0).toLocaleString()}</td>
                                  <td className="py-1 pr-2 text-right">{(inst.paidAmount ?? 0).toLocaleString()}</td>
                                  <td className="py-1 text-right"><StatusBadge status={inst.status} /></td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      )}
                      {payingArrangement === a.id && (
                        <div className="flex items-center gap-2 mt-2 bg-muted/50 p-3 rounded-md">
                          <Input
                            type="number"
                            placeholder="Payment amount"
                            value={paymentAmount || ""}
                            onChange={(e) => setPaymentAmount(Number(e.target.value))}
                            className="w-40"
                          />
                          <Button
                            size="sm"
                            onClick={() => handleRecordPayment(a.id)}
                            disabled={recordPaymentMutation.isPending || !paymentAmount}
                          >
                            {recordPaymentMutation.isPending ? "Submitting..." : "Submit"}
                          </Button>
                        </div>
                      )}
                    </div>
                  )
                })
              )}
            </CardContent>
          </Card>
        </div>
      ),
    },
    {
      id: "actions",
      label: `Actions (${(actions ?? []).length})`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Collection Actions</CardTitle></CardHeader>
          <CardContent>
            {(!actions || actions.length === 0) ? (
              <p className="text-sm text-muted-foreground">No actions recorded.</p>
            ) : (
              actions.map((a) => (
                <div key={a.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{a.actionType}</span>
                    <span className="text-sm text-muted-foreground">{a.performedAt ? new Date(a.performedAt).toLocaleString() : "-"}</span>
                  </div>
                  <p className="text-sm text-muted-foreground">{a.description} — {a.performedBy}</p>
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
          <CardHeader><CardTitle className="text-base">Audit Trail</CardTitle></CardHeader>
          <CardContent>
            {(!auditEntries || auditEntries.length === 0) ? (
              <p className="text-sm text-muted-foreground">No audit entries.</p>
            ) : (
              auditEntries.map((e) => (
                <div key={e.id} className="border-b py-3">
                  <span className="font-medium">{e.action}</span>
                  <span className="text-sm text-muted-foreground ml-2">{new Date(e.performedAt).toLocaleString()}</span>
                  <p className="text-sm text-muted-foreground">By: {e.performedByName}</p>
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
      <EntityHeader title={`Case ${id.substring(0, 8)}`} subtitle={caseData?.customerName} status={caseData?.status} backHref="/collections" loading={isLoading} />
      {caseData && caseData.status !== "RESOLVED" && caseData.status !== "CLOSED" && (
        <div className="flex flex-wrap gap-2">
          <Button variant="default" size="sm" onClick={() => resolveMutation.mutate()}>
            <CheckCircle className="mr-1 h-4 w-4" /> Resolve
          </Button>
          <Button variant="outline" size="sm" onClick={() => dunningMutation.mutate()}>
            <Send className="mr-1 h-4 w-4" /> Send Dunning
          </Button>
          <Button variant="outline" size="sm" asChild>
            <Link href={`/collections/${id}/actions/new`}>
              <Phone className="mr-1 h-4 w-4" /> Add Action
            </Link>
          </Button>
          <Button variant="outline" size="sm" asChild>
            <Link href={`/collections/${id}/arrangements/new`}>
              <CreditCard className="mr-1 h-4 w-4" /> Add Arrangement
            </Link>
          </Button>
        </div>
      )}
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
