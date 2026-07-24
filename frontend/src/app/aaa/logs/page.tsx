"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { useAaaLogs } from "@/api/hooks/useAaaLogs"
import type { AaaAuditLogDto } from "@/api/generated/dto"

export default function AuditLogsPage() {
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)

  const filters: Record<string, string> = { page: String(page), pageSize: "50" }
  if (search) filters.username = search

  const { data, isLoading, error } = useAaaLogs(filters)

  const columns = [
    { id: "timestamp", header: "Timestamp", accessorKey: "timestamp" as const, sortable: true },
    { id: "eventType", header: "Event Type", cell: (row: AaaAuditLogDto) => <StatusBadge status={row.eventType} /> },
    { id: "username", header: "Username", accessorKey: "username" as const },
    { id: "nasIpAddress", header: "NAS IP", accessorKey: "nasIpAddress" as const },
    { id: "detail", header: "Detail", accessorKey: "detail" as const },
  ]

  return (
    <div className="space-y-4">
      <PageHeader title="Audit Logs" />
      <div className="flex gap-2">
        <SearchBar value={search} onChange={setSearch} placeholder="Search by username..." />
      </div>
      <DataTable<AaaAuditLogDto>
        columns={columns}
        data={data?.items ?? []}
        loading={isLoading}
        error={error?.message}
        rowKey={(row) => row.id}
        emptyTitle="No audit log entries found"
        pagination={{
          page,
          pageSize: 50,
          total: data?.totalCount ?? 0,
          onPageChange: setPage,
          onPageSizeChange: () => {},
        }}
      />
    </div>
  )
}
