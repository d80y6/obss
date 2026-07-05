"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useDiscoveryJobs } from "@/api/hooks/use-service-inventory"
import type { DiscoveryJobDto } from "@/api/generated"
import { Search } from "lucide-react"

export default function DiscoveryPage() {
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading } = useDiscoveryJobs()

  const columns: Column<DiscoveryJobDto>[] = [
    { id: "discoveryType", header: "Type", accessorKey: "discoveryType", sortable: true },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "resourcesFound", header: "Found", accessorKey: "resourcesFound" },
    { id: "resourcesMatched", header: "Matched", accessorKey: "resourcesMatched" },
    { id: "createdBy", header: "Created By", accessorKey: "createdBy" },
    { id: "startedAt", header: "Started", cell: (row) => row.startedAt ? new Date(row.startedAt).toLocaleString() : "-" },
    { id: "completedAt", header: "Completed", cell: (row) => row.completedAt ? new Date(row.completedAt).toLocaleString() : "-" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Discovery Jobs" backHref="/service-inventory" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No discovery jobs"
            emptyIcon={Search}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
          />
        </CardContent>
      </Card>
    </div>
  )
}
