"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import { AuditEntryDto, AuditAlertDto } from "@/types/api"
import { ScrollText, AlertTriangle, Download } from "lucide-react"
import { useRouter } from "next/navigation"
import Link from "next/link"

const entityTypeOptions = [
  { label: "Ticket", value: "Ticket" },
  { label: "Customer", value: "Customer" },
  { label: "Order", value: "Order" },
  { label: "Subscription", value: "Subscription" },
  { label: "Invoice", value: "Invoice" },
  { label: "Payment", value: "Payment" },
  { label: "User", value: "User" },
  { label: "Notification", value: "Notification" },
  { label: "ReportDefinition", value: "ReportDefinition" },
  { label: "ScheduledReport", value: "ScheduledReport" },
  { label: "ApiRoute", value: "ApiRoute" },
  { label: "ApiKey", value: "ApiKey" },
  { label: "Partner", value: "Partner" },
]

const actionOptions = [
  { label: "CREATE", value: "CREATE" },
  { label: "UPDATE", value: "UPDATE" },
  { label: "DELETE", value: "DELETE" },
  { label: "ASSIGN", value: "ASSIGN" },
  { label: "RESOLVE", value: "RESOLVE" },
  { label: "CLOSE", value: "CLOSE" },
  { label: "ESCALATE", value: "ESCALATE" },
]

export default function AuditPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [search, setSearch] = useState("")
  const [entityTypeFilter, setEntityTypeFilter] = useState("")
  const [actorFilter, setActorFilter] = useState("")
  const [actionFilter, setActionFilter] = useState("")
  const [dateFrom, setDateFrom] = useState("")
  const [dateTo, setDateTo] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(entityTypeFilter ? { entityType: entityTypeFilter } : {}),
    ...(actorFilter ? { actor: actorFilter } : {}),
    ...(actionFilter ? { action: actionFilter } : {}),
    ...(dateFrom ? { dateFrom } : {}),
    ...(dateTo ? { dateTo } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data: entries, isLoading, error } = useQuery({
    queryKey: queryKeys.audit.entries.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/audit/entries?${params.toString()}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as AuditEntryDto[], total }
    },
  })

  const { data: alerts } = useQuery({
    queryKey: ["audit-alerts"],
    queryFn: async () => {
      const res = await api.get("/api/v1/audit/alerts")
      return res.data as AuditAlertDto[]
    },
  })

  const handleExportCsv = () => {
    const params = new URLSearchParams()
    Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
    window.open(`/api/v1/audit/entries/export?${params.toString()}`, "_blank")
  }

  const columns: Column<AuditEntryDto>[] = [
    { id: "action", header: "Action", accessorKey: "action", sortable: true },
    { id: "entityType", header: "Entity Type", accessorKey: "entityType" },
    { id: "entityId", header: "Entity ID", cell: (row) => row.entityId.substring(0, 8) + "..." },
    { id: "performedByName", header: "Actor", accessorKey: "performedByName" },
    { id: "performedAt", header: "Timestamp", cell: (row) => row.performedAt ? new Date(row.performedAt).toLocaleString() : "-", sortable: true },
  ]

  const filterConfig = [
    { id: "entityType", label: "Entity Type", type: "select" as const, options: entityTypeOptions, value: entityTypeFilter, onChange: (v: string) => { setEntityTypeFilter(v === "all" ? "" : v); setPage(1) }, placeholder: "All Types" },
    { id: "action", label: "Action", type: "select" as const, options: actionOptions, value: actionFilter, onChange: (v: string) => { setActionFilter(v === "all" ? "" : v); setPage(1) }, placeholder: "All Actions" },
    { id: "actor", label: "Actor", type: "text" as const, value: actorFilter, onChange: (v: string) => { setActorFilter(v); setPage(1) }, placeholder: "Filter by actor..." },
    { id: "dateFrom", label: "From", type: "date-range" as const, value: dateFrom, onChange: (v: string) => { setDateFrom(v); setPage(1) } },
    { id: "dateTo", label: "To", type: "date-range" as const, value: dateTo, onChange: (v: string) => { setDateTo(v); setPage(1) } },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Audit"
        actions={
          <>
            <Button variant="outline" size="sm" onClick={handleExportCsv}>
              <Download className="mr-1 h-4 w-4" /> Export CSV
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/audit/alert-rules"><AlertTriangle className="mr-1 h-4 w-4" /> Alert Rules</Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/audit/alerts"><AlertTriangle className="mr-1 h-4 w-4" /> Alerts ({(alerts ?? []).length})</Link>
            </Button>
          </>
        }
      />

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardContent className="pt-6">
            <p className="text-2xl font-bold">{entries?.items?.length ?? 0}</p>
            <p className="text-sm text-muted-foreground">Total Entries</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-2xl font-bold">{new Set(entries?.items?.map((e) => e.performedByName) ?? []).size}</p>
            <p className="text-sm text-muted-foreground">Unique Actors</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-2xl font-bold">{new Set(entries?.items?.map((e) => e.entityType) ?? []).size}</p>
            <p className="text-sm text-muted-foreground">Entity Types</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardContent className="pt-6 space-y-4">
          <div className="flex gap-4">
            <SearchBar placeholder="Search entity ID or description..." value={search} onChange={(v) => { setSearch(v); setPage(1) }} />
          </div>
          <FilterBar
            filters={filterConfig}
            onClear={() => { setEntityTypeFilter(""); setActorFilter(""); setActionFilter(""); setDateFrom(""); setDateTo(""); setPage(1) }}
          />
          <DataTable
            columns={columns}
            data={entries?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No audit entries"
            emptyIcon={ScrollText}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/audit/${row.id}`)}
            pagination={{ page, pageSize, total: entries?.total ?? entries?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
