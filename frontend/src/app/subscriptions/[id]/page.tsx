"use client"

import { useParams } from "next/navigation"
import { useState } from "react"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useSubscription } from "@/api/hooks/useSubscription"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { EntitlementDto, AuditEntryDto } from "@/types/api"
import { formatCurrency } from "@/lib/formatters"
import { toast } from "@/components/ui/toast"

export default function SubscriptionDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: sub, isLoading } = useSubscription(id)
  const queryClient = useQueryClient()
  const [confirmAction, setConfirmAction] = useState<string | null>(null)
  const [cancelReason, setCancelReason] = useState("")

  const lifecycleMutation = useMutation({
    mutationFn: async (action: string) => {
      const body = action === "cancel"
        ? { reason: cancelReason || "Customer request", effectiveDate: new Date().toISOString() }
        : action === "suspend"
        ? { reason: cancelReason || "Administrative" }
        : undefined
      await api.post(`/api/v1/subscriptions/subscriptions/${id}/${action}`, body)
    },
    onSuccess: (_data, action) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.subscriptions.detail(id) })
      toast({ title: "Success", description: `Subscription has been ${action}ed.` })
      setConfirmAction(null)
      setCancelReason("")
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to perform action.", variant: "destructive" })
    },
  })

  const { data: entitlements } = useQuery({
    queryKey: queryKeys.subscriptions.entitlements(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/subscriptions/subscriptions/${id}/entitlements`)
      return res.data as EntitlementDto[]
    },
    enabled: !!id,
  })

  const { data: auditEntries } = useQuery({
    queryKey: queryKeys.audit.entity("Subscription", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/Subscription/${id}`)
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
          title="Subscription Details"
          loading={isLoading}
          fields={[
            { label: "Subscription #", value: sub?.id?.slice(0, 8) ?? "-" },
            { label: "Customer", value: sub?.customerName ?? "-" },
            { label: "Offer", value: sub?.offerName ?? "-" },
            { label: "Status", value: sub ? <StatusBadge status={sub.status} /> : "-" },
            { label: "Amount", value: sub ? formatCurrency(sub.price, sub.currency) : "-" },
            { label: "Billing Period", value: sub?.billingPeriod ?? "-" },
            { label: "Quantity", value: sub ? String(sub.quantity) : "-" },
            { label: "Start Date", value: sub?.startDate ? new Date(sub.startDate).toLocaleDateString() : "-" },
            { label: "End Date", value: sub?.endDate ? new Date(sub.endDate).toLocaleDateString() : "-" },
            { label: "Created", value: sub?.createdAt ? new Date(sub.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "entitlements",
      label: `Entitlements (${(entitlements ?? []).length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Entitlements</CardTitle>
          </CardHeader>
          <CardContent>
            {!entitlements || entitlements.length === 0 ? (
              <p className="text-sm text-muted-foreground">No entitlements defined.</p>
            ) : (
              <div className="space-y-4">
                {entitlements.map((ent) => (
                  <div key={ent.id} className="border rounded-lg p-4">
                    <div className="flex justify-between items-center mb-2">
                      <span className="font-medium">{ent.name}</span>
                      <span className="text-sm text-muted-foreground">{ent.type}</span>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="flex-1 bg-muted rounded-full h-2">
                        <div
                          className="bg-primary h-2 rounded-full"
                          style={{ width: `${Math.min(100, (ent.usage / ent.limit) * 100)}%` }}
                        />
                      </div>
                      <span className="text-sm font-medium">
                        {ent.usage} / {ent.limit} {ent.unit}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      ),
    },
    {
      id: "addons",
      label: `Add-ons (${(entitlements ?? []).filter((e) => e.type === "ADDON").length})`,
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Add-ons</CardTitle>
          </CardHeader>
          <CardContent>
            {!entitlements || entitlements.filter((e) => e.type === "ADDON").length === 0 ? (
              <p className="text-sm text-muted-foreground">No add-ons found.</p>
            ) : (
              <div className="space-y-4">
                {entitlements.filter((e) => e.type === "ADDON").map((ent) => (
                  <div key={ent.id} className="border rounded-lg p-4">
                    <div className="flex justify-between items-center mb-2">
                      <span className="font-medium">{ent.name}</span>
                      <span className="text-sm text-muted-foreground">{ent.type}</span>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="flex-1 bg-muted rounded-full h-2">
                        <div
                          className="bg-primary h-2 rounded-full"
                          style={{ width: `${Math.min(100, (ent.usage / ent.limit) * 100)}%` }}
                        />
                      </div>
                      <span className="text-sm font-medium">
                        {ent.usage} / {ent.limit} {ent.unit}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
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
              <p className="text-sm text-muted-foreground">No audit entries found.</p>
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
        title={`Subscription ${sub?.offerName ?? ""}`}
        subtitle={sub?.customerName}
        status={sub?.status}
        backHref="/subscriptions"
        editHref={`/subscriptions/${id}/edit`}
        loading={isLoading}
      />

      {sub && !isLoading && (
        <div className="flex gap-2 flex-wrap">
          {sub.status === "ACTIVE" && (
            <Button variant="outline" size="sm" onClick={() => setConfirmAction("suspend")}>Suspend</Button>
          )}
          {sub.status === "ACTIVE" && (
            <Button variant="outline" size="sm" onClick={() => setConfirmAction("cancel")}>Cancel</Button>
          )}
          {sub.status === "SUSPENDED" && (
            <Button variant="outline" size="sm" onClick={() => setConfirmAction("resume")}>Resume</Button>
          )}
          {sub.status === "INACTIVE" && (
            <Button variant="outline" size="sm" onClick={() => setConfirmAction("activate")}>Activate</Button>
          )}
        </div>
      )}

      {confirmAction && (
        <Card>
          <CardContent className="pt-6 space-y-3">
            <p className="text-sm">
              Are you sure you want to <strong>{confirmAction}</strong> this subscription?
            </p>
            {(confirmAction === "cancel" || confirmAction === "suspend") && (
              <input
                className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm"
                placeholder="Reason..."
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
              />
            )}
            <div className="flex gap-2">
              <Button
                variant={confirmAction === "cancel" ? "destructive" : "default"}
                size="sm"
                onClick={() => lifecycleMutation.mutate(confirmAction)}
                disabled={lifecycleMutation.isPending}
              >
                {lifecycleMutation.isPending ? "Processing..." : `Confirm ${confirmAction.charAt(0).toUpperCase() + confirmAction.slice(1)}`}
              </Button>
              <Button variant="outline" size="sm" onClick={() => { setConfirmAction(null); setCancelReason("") }}>Cancel</Button>
            </div>
          </CardContent>
        </Card>
      )}

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
