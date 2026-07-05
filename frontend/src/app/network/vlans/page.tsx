"use client"

import { DataTable, Column } from "@/components/shared/DataTable"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useVlans } from "@/api/hooks/useVlans"
import type { VlanDto } from "@/api/generated/dto"
import { Waves } from "lucide-react"
import { useRouter } from "next/navigation"

export default function VlansPage() {
  const router = useRouter()
  const { data, isLoading, error } = useVlans()

  const columns: Column<VlanDto>[] = [
    { id: "vlanId", header: "VLAN ID", accessorKey: "vlanId", sortable: true },
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "subnet", header: "Subnet", accessorKey: "subnet" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="VLANs" backHref="/network" createHref="/network/vlans/new" createLabel="New VLAN" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No VLANs"
            emptyIcon={Waves}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/network/vlans/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
