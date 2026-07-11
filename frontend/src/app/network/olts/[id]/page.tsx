"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useOlt } from "@/api/hooks/useOlt"

export default function OltDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: olt, isLoading } = useOlt(id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="OLT Details"
          loading={isLoading}
          columns={2}
          fields={[
            { label: "Name", value: olt?.name ?? "-" },
            { label: "Model", value: olt?.model ?? "-" },
            { label: "Vendor", value: olt?.vendor ?? "-" },
            { label: "IP Address", value: olt?.ipAddress ?? "-" },
            { label: "Location", value: olt?.location ?? "-" },
            { label: "Status", value: olt ? <StatusBadge status={olt.status} /> : "-" },
            { label: "PON Port Count", value: olt?.ponPortCount?.toString() ?? "-" },
            { label: "Created", value: olt?.createdAt ? new Date(olt.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={olt?.name ?? "OLT"}
        subtitle={olt?.model ? `${olt.vendor} ${olt.model}` : undefined}
        status={olt?.status}
        backHref="/network/olts"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
