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
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { useRenewSubscription } from "@/api/hooks/useRenewSubscription"
import { useSetSubscriptionEntitlements } from "@/api/hooks/useSetSubscriptionEntitlements"
import { useOverrideEntitlementLimit } from "@/api/hooks/useOverrideEntitlementLimit"
import { useUpdateEntitlementUsage } from "@/api/hooks/useUpdateEntitlementUsage"
import { useChangeSubscriptionQuantity } from "@/api/hooks/useChangeSubscriptionQuantity"
import { useExtendSubscriptionEndDate } from "@/api/hooks/useExtendSubscriptionEndDate"
import type { EntitlementDto, EntitlementDefinition } from "@/api/generated"
import { formatCurrency } from "@/lib/formatters"
import { toast } from "@/components/ui/toast"
import { RefreshCw, Plus, Pencil, Hash, Calendar } from "lucide-react"

export default function SubscriptionDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: sub, isLoading } = useSubscription(id)
  const queryClient = useQueryClient()
  const [confirmAction, setConfirmAction] = useState<string | null>(null)
  const [cancelReason, setCancelReason] = useState("")
  const [showEntitlementForm, setShowEntitlementForm] = useState(false)
  const [entitlementForm, setEntitlementForm] = useState({ entitlementType: "", name: "", limit: "100", unit: "GB", isUnlimited: false })
  const [overrideForm, setOverrideForm] = useState<{ entitlementType: string; newLimit: string } | null>(null)
  const [usageForm, setUsageForm] = useState<{ entitlementType: string; amount: string } | null>(null)

  const [showQuantityForm, setShowQuantityForm] = useState(false)
  const [newQuantity, setNewQuantity] = useState("")
  const [showEndDateForm, setShowEndDateForm] = useState(false)
  const [newEndDate, setNewEndDate] = useState("")

  const renewSubscription = useRenewSubscription()
  const setEntitlements = useSetSubscriptionEntitlements(id)
  const overrideLimit = useOverrideEntitlementLimit(id)
  const recordUsage = useUpdateEntitlementUsage(id)
  const changeQuantity = useChangeSubscriptionQuantity(id)
  const extendEndDate = useExtendSubscriptionEndDate(id)

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

  const { data: auditEntries } = useAuditLog("Subscription", id)

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
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">Entitlements</CardTitle>
            <Button variant="outline" size="sm" onClick={() => setShowEntitlementForm(true)}>
              <Plus className="mr-1 h-4 w-4" /> Add
            </Button>
          </CardHeader>
          <CardContent>
            {!entitlements || entitlements.length === 0 ? (
              <p className="text-sm text-muted-foreground">No entitlements defined.</p>
            ) : (
              <div className="space-y-4">
                {entitlements.map((ent) => (
                  <div key={ent.id} className="border rounded-lg p-4">
                    <div className="flex justify-between items-center mb-2">
                      <div>
                        <span className="font-medium">{ent.name}</span>
                        <span className="text-sm text-muted-foreground ml-2">{ent.entitlementType}</span>
                        {ent.isUnlimited && <span className="text-xs text-muted-foreground ml-2">(Unlimited)</span>}
                        {ent.validTo && (
                          <span className="text-xs text-muted-foreground ml-2">
                            expires {new Date(ent.validTo).toLocaleDateString()}
                          </span>
                        )}
                      </div>
                      <div className="flex gap-1">
                        {ent.isOverridable && (
                          <Button variant="ghost" size="sm" onClick={() => setOverrideForm({ entitlementType: ent.entitlementType, newLimit: String(ent.limit) })}>
                            <Pencil className="h-3 w-3" />
                          </Button>
                        )}
                        <Button variant="ghost" size="sm" onClick={() => setUsageForm({ entitlementType: ent.entitlementType, amount: "" })}>
                          +
                        </Button>
                      </div>
                    </div>
                    {!ent.isUnlimited && (
                      <div className="flex items-center gap-4">
                        <div className="flex-1 bg-muted rounded-full h-2">
                          <div
                            className="bg-primary h-2 rounded-full"
                            style={{ width: `${Math.min(100, ent.limit > 0 ? (ent.used / ent.limit) * 100 : 0)}%` }}
                          />
                        </div>
                        <span className="text-sm font-medium">
                          {ent.used} / {ent.limit} {ent.unit}
                        </span>
                      </div>
                    )}
                    {overrideForm?.entitlementType === ent.entitlementType && (
                      <div className="mt-3 flex gap-2 items-end">
                        <div className="flex-1">
                          <label className="text-xs text-muted-foreground">New Limit ({ent.unit})</label>
                          <input
                            className="flex h-8 w-full rounded-md border border-input bg-background px-2 text-sm"
                            value={overrideForm.newLimit}
                            onChange={(e) => setOverrideForm({ ...overrideForm, newLimit: e.target.value })}
                          />
                        </div>
                        <Button size="sm" onClick={() => {
                          overrideLimit.mutate({ entitlementType: overrideForm.entitlementType, newLimit: parseFloat(overrideForm.newLimit) })
                          setOverrideForm(null)
                        }} disabled={overrideLimit.isPending}>Save</Button>
                        <Button variant="ghost" size="sm" onClick={() => setOverrideForm(null)}>Cancel</Button>
                      </div>
                    )}
                    {usageForm?.entitlementType === ent.entitlementType && (
                      <div className="mt-3 flex gap-2 items-end">
                        <div className="flex-1">
                          <label className="text-xs text-muted-foreground">Amount to record ({ent.unit})</label>
                          <input
                            className="flex h-8 w-full rounded-md border border-input bg-background px-2 text-sm"
                            type="number"
                            value={usageForm.amount}
                            onChange={(e) => setUsageForm({ ...usageForm, amount: e.target.value })}
                          />
                        </div>
                        <Button size="sm" onClick={() => {
                          recordUsage.mutate({ entitlementType: usageForm.entitlementType, amount: parseFloat(usageForm.amount) || 0 })
                          setUsageForm(null)
                        }} disabled={recordUsage.isPending}>Record</Button>
                        <Button variant="ghost" size="sm" onClick={() => setUsageForm(null)}>Cancel</Button>
                      </div>
                    )}
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
          {(sub.status === "ACTIVE" || sub.status === "SUSPENDED") && (
            <Button variant="outline" size="sm" onClick={() => { setShowQuantityForm(!showQuantityForm); setShowEndDateForm(false) }}>
              <Hash className="mr-1 h-4 w-4" /> Change Qty
            </Button>
          )}
          {(sub.status === "ACTIVE" || sub.status === "SUSPENDED") && (
            <Button variant="outline" size="sm" onClick={() => { setShowEndDateForm(!showEndDateForm); setShowQuantityForm(false) }}>
              <Calendar className="mr-1 h-4 w-4" /> Extend Date
            </Button>
          )}
          {sub.status === "ACTIVE" && (
            <Button variant="outline" size="sm" onClick={() => renewSubscription.mutate(id)} disabled={renewSubscription.isPending}>
              <RefreshCw className="mr-1 h-4 w-4" /> Renew
            </Button>
          )}
          {sub.status === "ACTIVE" && (
            <Button variant="outline" size="sm" onClick={() => setConfirmAction("suspend")}>Suspend</Button>
          )}
          {sub.status === "ACTIVE" && (
            <Button variant="outline" size="sm" onClick={() => setConfirmAction("cancel")}>Cancel</Button>
          )}
          {sub.status === "SUSPENDED" && (
            <Button variant="outline" size="sm" onClick={() => setConfirmAction("resume")}>Resume</Button>
          )}
          {sub.status === "PENDING" && (
            <Button variant="outline" size="sm" onClick={() => setConfirmAction("activate")}>Activate</Button>
          )}
        </div>
      )}

      {showQuantityForm && (
        <Card>
          <CardContent className="pt-6 space-y-3">
            <p className="text-sm font-medium">Change Quantity</p>
            <p className="text-xs text-muted-foreground">Current quantity: {sub?.quantity}</p>
            <input
              className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
              type="number" min="1" placeholder="New quantity"
              value={newQuantity}
              onChange={(e) => setNewQuantity(e.target.value)}
            />
            <div className="flex gap-2">
              <Button size="sm" onClick={() => {
                changeQuantity.mutate({ subscriptionId: id, newQuantity: parseInt(newQuantity, 10) })
                setShowQuantityForm(false)
                setNewQuantity("")
              }} disabled={changeQuantity.isPending || !newQuantity}>Save</Button>
              <Button variant="outline" size="sm" onClick={() => { setShowQuantityForm(false); setNewQuantity("") }}>Cancel</Button>
            </div>
          </CardContent>
        </Card>
      )}

      {showEndDateForm && (
        <Card>
          <CardContent className="pt-6 space-y-3">
            <p className="text-sm font-medium">Extend End Date</p>
            <p className="text-xs text-muted-foreground">Current end date: {sub?.endDate ? new Date(sub.endDate).toLocaleDateString() : "No end date"}</p>
            <input
              className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
              type="date"
              value={newEndDate}
              onChange={(e) => setNewEndDate(e.target.value)}
            />
            <div className="flex gap-2">
              <Button size="sm" onClick={() => {
                extendEndDate.mutate({ subscriptionId: id, newEndDate: new Date(newEndDate).toISOString() })
                setShowEndDateForm(false)
                setNewEndDate("")
              }} disabled={extendEndDate.isPending || !newEndDate}>Save</Button>
              <Button variant="outline" size="sm" onClick={() => { setShowEndDateForm(false); setNewEndDate("") }}>Cancel</Button>
            </div>
          </CardContent>
        </Card>
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

      {showEntitlementForm && (
        <Card>
          <CardHeader><CardTitle className="text-base">Add Entitlements</CardTitle></CardHeader>
          <CardContent className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-muted-foreground">Type</label>
                <input
                  className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                  placeholder="e.g. Bandwidth"
                  value={entitlementForm.entitlementType}
                  onChange={(e) => setEntitlementForm({ ...entitlementForm, entitlementType: e.target.value })}
                />
              </div>
              <div>
                <label className="text-xs text-muted-foreground">Name</label>
                <input
                  className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                  placeholder="e.g. Data Allowance"
                  value={entitlementForm.name}
                  onChange={(e) => setEntitlementForm({ ...entitlementForm, name: e.target.value })}
                />
              </div>
              <div>
                <label className="text-xs text-muted-foreground">Limit</label>
                <input
                  className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                  type="number"
                  value={entitlementForm.limit}
                  onChange={(e) => setEntitlementForm({ ...entitlementForm, limit: e.target.value })}
                />
              </div>
              <div>
                <label className="text-xs text-muted-foreground">Unit</label>
                <input
                  className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                  placeholder="e.g. GB"
                  value={entitlementForm.unit}
                  onChange={(e) => setEntitlementForm({ ...entitlementForm, unit: e.target.value })}
                />
              </div>
            </div>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={entitlementForm.isUnlimited}
                onChange={(e) => setEntitlementForm({ ...entitlementForm, isUnlimited: e.target.checked })}
              />
              Unlimited
            </label>
            <div className="flex gap-2">
              <Button size="sm" onClick={() => {
                const definition: EntitlementDefinition = {
                  entitlementType: entitlementForm.entitlementType,
                  name: entitlementForm.name,
                  limit: parseFloat(entitlementForm.limit) || 0,
                  used: 0,
                  unit: entitlementForm.unit,
                  isUnlimited: entitlementForm.isUnlimited,
                  isOverridable: true,
                  validFrom: new Date().toISOString(),
                  validTo: null,
                }
                setEntitlements.mutate({ entitlements: [definition] })
                setShowEntitlementForm(false)
                setEntitlementForm({ entitlementType: "", name: "", limit: "100", unit: "GB", isUnlimited: false })
              }} disabled={setEntitlements.isPending}>Save</Button>
              <Button variant="outline" size="sm" onClick={() => setShowEntitlementForm(false)}>Cancel</Button>
            </div>
          </CardContent>
        </Card>
      )}

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
