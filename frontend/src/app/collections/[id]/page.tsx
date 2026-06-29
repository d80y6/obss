"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { CollectionCaseDto, CollectionActionDto, PaymentArrangementDto, AuditEntryDto } from "@/types/api"
import { toast } from "@/components/ui/toast"
import Link from "next/link"
import { CheckCircle, Send, Plus, Phone, CreditCard } from "lucide-react"

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

  const { data: auditEntries } = useQuery({
    queryKey: queryKeys.audit.entity("CollectionCase", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/CollectionCase/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

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
              <CardTitle className="text-base">Payment Arrangements</CardTitle>
            </CardHeader>
            <CardContent>
              {(!arrangements || arrangements.length === 0) ? (
                <p className="text-sm text-muted-foreground">No payment arrangements.</p>
              ) : (
                arrangements.map((a) => (
                  <div key={a.id} className="border-b py-3">
                    <div className="flex justify-between">
                      <span className="font-medium">{a.installmentCount} installments of {caseData?.currency ?? ""} {(a.installmentAmount ?? 0).toLocaleString()}</span>
                      <StatusBadge status={a.status} />
                    </div>
                    <p className="text-sm text-muted-foreground">
                      {a.frequency} — {a.firstPaymentDate ? new Date(a.firstPaymentDate).toLocaleDateString() : "-"} to {a.lastPaymentDate ? new Date(a.lastPaymentDate).toLocaleDateString() : "-"}
                    </p>
                  </div>
                ))
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
