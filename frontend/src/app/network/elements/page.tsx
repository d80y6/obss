"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { NetworkElementDto } from "@/types/api"
import { Cable } from "lucide-react"
import { useRouter } from "next/navigation"

export default function NetworkElementsPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading, error } = useQuery({
    queryKey: ["network-elements"],
    queryFn: async () => {
      const res = await api.get("/api/v1/network/elements")
      return res.data as NetworkElementDto[]
    },
  })

  const columns: Column<NetworkElementDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "elementType", header: "Type", accessorKey: "elementType" },
    { id: "model", header: "Model", accessorKey: "model" },
    { id: "vendor", header: "Vendor", accessorKey: "vendor" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "location", header: "Location", accessorKey: "location" },
    { id: "ipAddress", header: "IP", accessorKey: "ipAddress" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Network Elements" backHref="/network" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No network elements"
            emptyIcon={Cable}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/network/elements/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
