"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent } from "@/components/ui/card"
import { api } from "@/api/client"
import { NetworkElementDto } from "@/types/api"
import { useQuery } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Server } from "lucide-react"

export default function VlansPage() {
  const router = useRouter()
  const { data, isLoading, error } = useQuery({
    queryKey: ["network", "vlans"],
    queryFn: async () => {
      const res = await api.get("/api/v1/network/vlans")
      return res.data as NetworkElementDto[]
    },
  })

  const columns: Column<NetworkElementDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "location", header: "Location", accessorKey: "location" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="VLANs" backHref="/network" />
      <Card>
        <CardContent className="pt-6">
          <DataTable columns={columns} data={data ?? []} loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No VLANs" emptyIcon={Server}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/network/vlans/${row.id}`)} />
        </CardContent>
      </Card>
    </div>
  )
}
