"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useVlan } from "@/api/hooks/useVlan"

export default function VlanDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: vlan, isLoading } = useVlan(id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="VLAN Details"
          loading={isLoading}
          columns={2}
          fields={[
            { label: "VLAN ID", value: vlan?.vlanId?.toString() ?? "-" },
            { label: "Name", value: vlan?.name ?? "-" },
            { label: "Description", value: vlan?.description ?? "-" },
            { label: "Subnet", value: vlan?.subnet ?? "-" },
            { label: "Status", value: vlan ? <StatusBadge status={vlan.status} /> : "-" },
            { label: "Created", value: vlan?.createdAt ? new Date(vlan.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={vlan?.name ?? "VLAN"}
        subtitle={`VLAN ${vlan?.vlanId ?? ""}`}
        status={vlan?.status}
        backHref="/network/vlans"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
