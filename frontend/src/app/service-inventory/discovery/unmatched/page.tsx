"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useUnmatchedResources } from "@/api/hooks/use-service-inventory"
import type { DiscoveryJobDto } from "@/api/generated"
import { Search } from "lucide-react"

export default function UnmatchedResourcesPage() {
  const { data, isLoading } = useUnmatchedResources()

  const columns: Column<DiscoveryJobDto>[] = [
    { id: "discoveryType", header: "Discovery Type", accessorKey: "discoveryType", sortable: true },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "resourcesFound", header: "Resources Found", accessorKey: "resourcesFound" },
    { id: "resourcesMatched", header: "Resources Matched", accessorKey: "resourcesMatched" },
    {
      id: "unmatched",
      header: "Unmatched",
      cell: (row) => {
        const unmatched = row.resourcesFound - row.resourcesMatched
        return (
          <span className={unmatched > 0 ? "font-medium text-destructive" : ""}>
            {unmatched}
          </span>
        )
      },
    },
    { id: "createdBy", header: "Created By", accessorKey: "createdBy" },
    { id: "startedAt", header: "Started At", cell: (row) => row.startedAt ? new Date(row.startedAt).toLocaleString() : "-" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Unmatched Resources" backHref="/service-inventory/discovery" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No unmatched resources"
            emptyIcon={Search}
            rowKey={(row) => row.id}
          />
        </CardContent>
      </Card>
    </div>
  )
}
