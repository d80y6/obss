"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useUsageRecords, useUnratedRecords, useRateUsage } from "@/api/hooks/use-rating"
import type { UsageRecordDto } from "@/api/generated"
import { BarChart3, Play } from "lucide-react"
import { useRouter } from "next/navigation"
import { toast } from "@/components/ui/toast"

export default function UsageRecordsPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [showUnrated, setShowUnrated] = useState(false)

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useUsageRecords(filters)
  const { data: unratedRecords, isLoading: unratedLoading } = useUnratedRecords()
  const rateMutation = useRateUsage()

  const displayData = showUnrated ? (unratedRecords ?? []) : (data ?? [])
  const displayLoading = showUnrated ? unratedLoading : isLoading

  const handleBatchRate = () => {
    rateMutation.mutate(undefined, {
      onSuccess: () => {
        toast({ title: "Rating complete", description: "Unrated records have been processed." })
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to rate records.", variant: "destructive" })
      },
    })
  }

  const columns: Column<UsageRecordDto>[] = [
    { id: "subscriptionId", header: "Subscription", accessorKey: "subscriptionId" },
    { id: "usageType", header: "Usage Type", accessorKey: "usageType" },
    {
      id: "quantity",
      header: "Quantity",
      cell: (row) => `${row.quantity} ${row.unit}`,
      sortable: true,
    },
    { id: "unit", header: "Unit", accessorKey: "unit" },
    {
      id: "status",
      header: "Status",
      cell: (row) => <StatusBadge status={row.status} />,
      sortable: true,
    },
    {
      id: "recordedAt",
      header: "Recorded",
      cell: (row) => new Date(row.recordedAt).toLocaleDateString(),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Usage Records"
        createHref="/rating/usage/new"
        createLabel="Submit Usage"
        actions={
          <>
            <Button
              variant={showUnrated ? "default" : "outline"}
              size="sm"
              onClick={() => setShowUnrated(!showUnrated)}
            >
              Unrated ({unratedRecords?.length ?? 0})
            </Button>
            {showUnrated && (
              <Button
                variant="default"
                size="sm"
                onClick={handleBatchRate}
                disabled={rateMutation.isPending || !unratedRecords?.length}
              >
                <Play className="mr-1 h-4 w-4" />
                {rateMutation.isPending ? "Rating..." : "Rate All"}
              </Button>
            )}
          </>
        }
      />

      <Card>
        <CardHeader className="pb-3">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search usage records..." />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={displayData}
            loading={displayLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No usage records"
            emptyDescription="Submit usage records to start tracking."
            emptyIcon={BarChart3}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/rating/usage/${row.id}`)}
            pagination={
              !showUnrated
                ? {
                    page,
                    pageSize,
                    total: data?.length ?? 0,
                    onPageChange: setPage,
                    onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
                  }
                : undefined
            }
          />
        </CardContent>
      </Card>
    </div>
  )
}
