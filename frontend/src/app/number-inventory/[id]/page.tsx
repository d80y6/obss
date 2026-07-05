"use client"

import { useParams, useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { toast } from "@/components/ui/toast"
import { useTelephoneNumber } from "@/api/hooks/useTelephoneNumber"
import { useAssignTelephoneNumber } from "@/api/hooks/useAssignTelephoneNumber"
import { useReleaseTelephoneNumber } from "@/api/hooks/useReleaseTelephoneNumber"
import { useState } from "react"

export default function NumberDetailPage() {
  const params = useParams()
  const router = useRouter()
  const id = params.id as string

  const { data: tel, isLoading } = useTelephoneNumber(id)
  const assignMutation = useAssignTelephoneNumber()
  const releaseMutation = useReleaseTelephoneNumber()
  const [customerId, setCustomerId] = useState("")

  const handleAssign = () => {
    if (!customerId.trim()) {
      toast({ title: "Validation Error", description: "Customer ID is required.", variant: "destructive" })
      return
    }
    assignMutation.mutate({ id, customerId }, {
      onSuccess: () => {
        toast({ title: "Number assigned", description: `Number ${tel?.number} has been assigned.` })
        setCustomerId("")
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to assign number.", variant: "destructive" })
      },
    })
  }

  const handleRelease = () => {
    releaseMutation.mutate(id, {
      onSuccess: () => {
        toast({ title: "Number released", description: `Number ${tel?.number} has been released.` })
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to release number.", variant: "destructive" })
      },
    })
  }

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Number Details"
          loading={isLoading}
          columns={2}
          fields={[
            { label: "Number", value: tel?.number ?? "-" },
            { label: "Type", value: tel?.numberType ?? "-" },
            { label: "Status", value: tel ? <StatusBadge status={tel.status} /> : "-" },
            { label: "Customer ID", value: tel?.customerId ?? "-" },
            { label: "Subscription ID", value: tel?.subscriptionId ?? "-" },
            { label: "Cost", value: tel ? `${tel.currency} ${tel.cost.toFixed(2)}` : "-" },
            { label: "Currency", value: tel?.currency ?? "-" },
            { label: "Notes", value: tel?.notes ?? "-" },
            { label: "Assigned At", value: tel?.assignedAt ? new Date(tel.assignedAt).toLocaleString() : "-" },
            { label: "Reserved At", value: tel?.reservedAt ? new Date(tel.reservedAt).toLocaleString() : "-" },
            { label: "Created", value: tel?.createdAt ? new Date(tel.createdAt).toLocaleString() : "-" },
            { label: "Updated", value: tel?.updatedAt ? new Date(tel.updatedAt).toLocaleString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "actions",
      label: "Actions",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {tel?.status === "Available" && (
              <div className="flex items-end gap-3">
                <div className="flex-1 space-y-2">
                  <label className="text-sm font-medium">Customer ID</label>
                  <Input
                    placeholder="Enter customer ID"
                    value={customerId}
                    onChange={(e) => setCustomerId(e.target.value)}
                  />
                </div>
                <Button onClick={handleAssign} disabled={assignMutation.isPending}>
                  {assignMutation.isPending ? "Assigning..." : "Assign"}
                </Button>
              </div>
            )}
            {tel?.status === "Assigned" && (
              <Button variant="destructive" onClick={handleRelease} disabled={releaseMutation.isPending}>
                {releaseMutation.isPending ? "Releasing..." : "Release Number"}
              </Button>
            )}
            {tel && !["Available", "Assigned"].includes(tel.status) && (
              <p className="text-sm text-muted-foreground">No actions available for numbers with status "{tel.status}".</p>
            )}
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={tel?.number ?? "Telephone Number"}
        subtitle={tel?.numberType}
        status={tel?.status}
        backHref="/number-inventory"
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
