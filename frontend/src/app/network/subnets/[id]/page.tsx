"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useSubnet } from "@/api/hooks/useSubnets"

export default function SubnetDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: subnet, isLoading } = useSubnet(id)

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={subnet?.name ?? "Subnet"}
        subtitle={subnet?.network}
        status={subnet?.status}
        backHref="/network/subnets"
        editHref={`/network/subnets/${id}/edit`}
        loading={isLoading}
      />
      <Card>
        <CardContent className="pt-6">
          <EntityMetadata
            title="Subnet Details"
            loading={isLoading}
            fields={[
              { label: "Name", value: subnet?.name ?? "-" },
              { label: "Network", value: subnet?.network ?? "-" },
              { label: "Status", value: subnet ? <StatusBadge status={subnet.status} /> : "-" },
              { label: "VLAN ID", value: subnet ? String(subnet.vlanId) : "-" },
              { label: "Gateway", value: subnet?.gateway ?? "-" },
              { label: "Location", value: subnet?.location ?? "-" },
              { label: "Description", value: subnet?.description ?? "-" },
              { label: "Created", value: subnet?.createdAt ? new Date(subnet.createdAt).toLocaleDateString() : "-" },
            ]}
          />
        </CardContent>
      </Card>
    </div>
  )
}
