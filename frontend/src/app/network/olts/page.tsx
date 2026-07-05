"use client"

import { DataTable, Column } from "@/components/shared/DataTable"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useOlts } from "@/api/hooks/useOlts"
import type { OltDto } from "@/api/generated/dto"
import { Radio } from "lucide-react"
import { useRouter } from "next/navigation"

export default function OltsPage() {
  const router = useRouter()
  const { data, isLoading, error } = useOlts()

  const columns: Column<OltDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "model", header: "Model", accessorKey: "model" },
    { id: "vendor", header: "Vendor", accessorKey: "vendor" },
    { id: "location", header: "Location", accessorKey: "location" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "ponPortCount", header: "Port Count", accessorKey: "ponPortCount" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="OLTs" backHref="/network" createHref="/network/olts/new" createLabel="New OLT" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No OLTs"
            emptyIcon={Radio}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/network/olts/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
