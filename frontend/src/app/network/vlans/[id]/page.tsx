"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import { NetworkElementDto } from "@/types/api"

export default function VlanDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: item, isLoading } = useQuery({
    queryKey: ["network", "vlans", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/vlans/${id}`)
      return res.data as NetworkElementDto
    },
    enabled: !!id,
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader title={item?.name ?? "VLAN"} subtitle={item?.elementType}
        status={item?.status} backHref="/network/vlans" loading={isLoading} />
      <EntityMetadata loading={isLoading} fields={[
        { label: "Name", value: item?.name ?? "-" },
        { label: "Status", value: item ? <StatusBadge status={item.status} /> : "-" },
        { label: "Location", value: item?.location ?? "-" },
        { label: "IP Address", value: item?.ipAddress ?? "-" },
        { label: "Model", value: item?.model ?? "-" },
        { label: "Vendor", value: item?.vendor ?? "-" },
        { label: "Serial Number", value: item?.serialNumber ?? "-" },
      ]} />
    </div>
  )
}
