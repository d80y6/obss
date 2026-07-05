"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useRouter } from "next/navigation"
import { useSubnets } from "@/api/hooks/useSubnets"
import type { SubnetDto } from "@/api/generated"
import { Network } from "lucide-react"

export default function SubnetsPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const { data, isLoading } = useSubnets()

  const columns: Column<SubnetDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "network", header: "Network (CIDR)", accessorKey: "network" },
    { id: "vlanId", header: "VLAN", accessorKey: "vlanId" },
    { id: "gateway", header: "Gateway", accessorKey: "gateway" },
    { id: "location", header: "Location", accessorKey: "location" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Subnets" createHref="/network/subnets/new" createLabel="New Subnet" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No subnets found"
            emptyIcon={Network}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/network/subnets/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
