"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useNotifications } from "@/api/hooks/use-notifications"
import type { NotificationDto } from "@/api/generated/dto"
import { Bell, FileText } from "lucide-react"
import Link from "next/link"
import { useRouter } from "next/navigation"

const typeOptions = [
  { label: "Info", value: "INFO" },
  { label: "Warning", value: "WARNING" },
  { label: "Error", value: "ERROR" },
]

const channelOptions = [
  { label: "Email", value: "EMAIL" },
  { label: "SMS", value: "SMS" },
  { label: "Push", value: "PUSH" },
]

const statusOptions = [
  { label: "Sent", value: "SENT" },
  { label: "Pending", value: "PENDING" },
  { label: "Failed", value: "FAILED" },
  { label: "Read", value: "READ" },
]

export default function NotificationsPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [search, setSearch] = useState("")
  const [typeFilter, setTypeFilter] = useState("")
  const [channelFilter, setChannelFilter] = useState("")
  const [statusFilter, setStatusFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(typeFilter ? { type: typeFilter } : {}),
    ...(channelFilter ? { channel: channelFilter } : {}),
    ...(statusFilter ? { status: statusFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useNotifications(filters)

  const columns: Column<NotificationDto>[] = [
    { id: "title", header: "Title", accessorKey: "title", sortable: true },
    { id: "type", header: "Type", accessorKey: "type" },
    { id: "channel", header: "Channel", accessorKey: "channel" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "createdAt", header: "Sent", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
  ]

  const filterConfig = [
    { id: "type", label: "Type", type: "select" as const, options: typeOptions, value: typeFilter, onChange: (v: string) => { setTypeFilter(v === "all" ? "" : v); setPage(1) }, placeholder: "All Types" },
    { id: "channel", label: "Channel", type: "select" as const, options: channelOptions, value: channelFilter, onChange: (v: string) => { setChannelFilter(v === "all" ? "" : v); setPage(1) }, placeholder: "All Channels" },
    { id: "status", label: "Status", type: "select" as const, options: statusOptions, value: statusFilter, onChange: (v: string) => { setStatusFilter(v === "all" ? "" : v); setPage(1) }, placeholder: "All Statuses" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Notifications"
        actions={
          <>
            <Button variant="outline" size="sm" asChild>
              <Link href="/notifications/preferences">Preferences</Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/notifications/templates"><FileText className="mr-1 h-4 w-4" /> Templates</Link>
            </Button>
          </>
        }
      />
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <SearchBar placeholder="Search by subject or recipient..." value={search} onChange={(v) => { setSearch(v); setPage(1) }} />
            <FilterBar
              filters={filterConfig}
              onClear={() => { setTypeFilter(""); setChannelFilter(""); setStatusFilter(""); setPage(1) }}
            />
          </div>
          <div className="mt-4">
            <DataTable
              columns={columns}
              data={data ?? []}
              loading={isLoading}
              error={error ? "Failed to load data." : undefined}
              emptyTitle="No notifications"
              emptyIcon={Bell}
              rowKey={(row) => row.id}
              selectedIds={selectedIds}
              onSelectionChange={setSelectedIds}
              onRowClick={(row) => router.push(`/notifications/${row.id}`)}
              pagination={{ page, pageSize, total: (data ?? []).length, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
