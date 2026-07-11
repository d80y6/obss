"use client"

import { useParams, useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useDunningPolicy, useDeleteDunningPolicy } from "@/api/hooks/use-collections"
import { toast } from "@/components/ui/toast"
import { useCallback } from "react"

export default function DunningPolicyDetailPage() {
  const params = useParams()
  const id = params.id as string
  const router = useRouter()

  const { data: policy, isLoading } = useDunningPolicy(id)
  const deleteMutation = useDeleteDunningPolicy()

  const handleDelete = useCallback(() => {
    if (!window.confirm(`Delete policy "${policy?.name}"? This action cannot be undone.`)) return
    deleteMutation.mutate(id, {
      onSuccess: () => {
        toast({ title: "Policy deleted" })
        router.push("/collections/dunning-policies")
      },
      onError: () => {
        toast({ title: "Failed to delete policy", variant: "destructive" })
      },
    })
  }, [deleteMutation, id, policy, router])

  const dunningFeeEntries = policy?.dunningFees
    ? Object.entries(policy.dunningFees).sort(([a], [b]) => Number(a) - Number(b))
    : []

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={policy?.name ?? "Dunning Policy"}
        subtitle={`Level ${policy?.maxDunningLevel ?? "?"} max`}
        status={policy?.isActive ? "ACTIVE" : "INACTIVE"}
        backHref="/collections/dunning-policies"
        editHref={`/collections/dunning-policies/${id}/edit`}
        onDelete={policy && !policy.isActive ? handleDelete : undefined}
        loading={isLoading}
      />
      <EntityMetadata
        title="Policy Details"
        loading={isLoading}
        fields={[
          { label: "Name", value: policy?.name ?? "-" },
          { label: "Description", value: policy?.description || "-" },
          { label: "Status", value: policy ? <StatusBadge status={policy.isActive ? "ACTIVE" : "INACTIVE"} /> : "-" },
          { label: "Max Dunning Level", value: String(policy?.maxDunningLevel ?? "-") },
          { label: "Days Between Actions", value: String(policy?.daysBetweenActions ?? "-") },
          { label: "Escalation After Days", value: String(policy?.escalationAfterDays ?? "-") },
        ]}
      />
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Dunning Fees</CardTitle>
        </CardHeader>
        <CardContent>
          {dunningFeeEntries.length === 0 ? (
            <p className="text-sm text-muted-foreground">No fees configured.</p>
          ) : (
            <div className="rounded-md border">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="px-4 py-2 text-left font-medium">Level</th>
                    <th className="px-4 py-2 text-right font-medium">Fee Amount</th>
                  </tr>
                </thead>
                <tbody>
                  {dunningFeeEntries.map(([level, fee]) => (
                    <tr key={level} className="border-b last:border-0">
                      <td className="px-4 py-2">{level}</td>
                      <td className="px-4 py-2 text-right">{(fee as number).toLocaleString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
