"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useBillingCycles } from "@/api/hooks/useBillingCycles"
import { Loader2 } from "lucide-react"

export default function BillingCycleDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data, isLoading } = useBillingCycles()

  const cycle = data?.items?.find((c) => c.id === id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Cycle Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: cycle?.name ?? "-" },
            { label: "Billing Date", value: cycle?.billingDate ? new Date(cycle.billingDate).toLocaleDateString() : "-" },
            { label: "Period", value: cycle?.period ?? "-" },
            { label: "Status", value: cycle ? <StatusBadge status={cycle.status} /> : "-" },
            { label: "Customer Count", value: cycle ? String(cycle.customerCount) : "-" },
            { label: "Total Amount", value: cycle ? `${(cycle.totalAmount ?? 0).toLocaleString()}` : "-" },
            { label: "Executed At", value: cycle?.executedAt ? new Date(cycle.executedAt).toLocaleString() : "-" },
            { label: "Created", value: cycle?.createdAt ? new Date(cycle.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
  ]

  if (isLoading) {
    return (
      <div className="flex-1 space-y-6 p-6 flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    )
  }

  if (!cycle) {
    return (
      <div className="flex-1 space-y-6 p-6">
        <EntityHeader title="Cycle Not Found" backHref="/billing/cycles" />
        <p className="text-muted-foreground">The requested billing cycle was not found.</p>
      </div>
    )
  }

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={cycle.name}
        subtitle={`Period: ${cycle.period}`}
        status={cycle.status}
        backHref="/billing/cycles"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
