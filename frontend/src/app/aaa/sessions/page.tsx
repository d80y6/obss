"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { useSessions } from "@/api/hooks/useSessions"
import type { RadiusSessionDto } from "@/api/generated/dto"

function formatDuration(seconds: number): string {
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  return `${h}h ${m}m ${s}s`
}

function formatBytes(bytes: number): string {
  if (bytes === 0) return "0 B"
  const k = 1024
  const sizes = ["B", "KB", "MB", "GB", "TB"]
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`
}

export default function SessionsListPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)

  const filters: Record<string, string> = { page: String(page), pageSize: "20" }
  if (search) filters.username = search

  const { data, isLoading, error } = useSessions(filters)

  const columns = [
    { id: "username", header: "Username", accessorKey: "username" as const, sortable: true },
    { id: "framedIpAddress", header: "Framed IP", accessorKey: "framedIpAddress" as const },
    { id: "sessionStatus", header: "Status", cell: (row: RadiusSessionDto) => <StatusBadge status={row.sessionStatus} /> },
    { id: "acctSessionTime", header: "Duration", cell: (row: RadiusSessionDto) => formatDuration(row.acctSessionTime) },
    { id: "inputOctets", header: "RX", cell: (row: RadiusSessionDto) => formatBytes(row.inputOctets) },
    { id: "outputOctets", header: "TX", cell: (row: RadiusSessionDto) => formatBytes(row.outputOctets) },
    { id: "startedAt", header: "Started", accessorKey: "startedAt" as const, sortable: true },
  ]

  return (
    <div className="space-y-4">
      <PageHeader title="RADIUS Sessions" />
      <div className="flex gap-2">
        <SearchBar value={search} onChange={setSearch} placeholder="Search by username..." />
      </div>
      <DataTable<RadiusSessionDto>
        columns={columns}
        data={data?.items ?? []}
        loading={isLoading}
        error={error?.message}
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/aaa/sessions/${row.id}`)}
        emptyTitle="No sessions found"
        pagination={{
          page,
          pageSize: 20,
          total: data?.totalCount ?? 0,
          onPageChange: setPage,
          onPageSizeChange: () => {},
        }}
      />
    </div>
  )
}
