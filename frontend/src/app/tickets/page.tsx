"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useTickets } from "@/api/hooks/useTickets"
import { TicketDto } from "@/types/api"
import { Ticket } from "lucide-react"
import { useRouter } from "next/navigation"
import { cn } from "@/lib/utils"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { toast } from "@/components/ui/toast"
import { queryKeys } from "@/lib/query-keys"

const statusOptions = [
  { label: "Open", value: "OPEN" },
  { label: "In Progress", value: "IN_PROGRESS" },
  { label: "Resolved", value: "RESOLVED" },
  { label: "Closed", value: "CLOSED" },
]

const priorityOptions = [
  { label: "Low", value: "LOW" },
  { label: "Medium", value: "MEDIUM" },
  { label: "High", value: "HIGH" },
  { label: "Critical", value: "CRITICAL" },
]

const categoryOptions = [
  { label: "Billing", value: "BILLING" },
  { label: "Technical", value: "TECHNICAL" },
  { label: "Account", value: "ACCOUNT" },
  { label: "Service", value: "SERVICE" },
  { label: "Other", value: "OTHER" },
]

const priorityColors: Record<string, string> = {
  LOW: "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300",
  MEDIUM: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300",
  HIGH: "bg-amber-100 text-amber-700 dark:bg-amber-900 dark:text-amber-300",
  CRITICAL: "bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300",
}

const priorityOrder: Record<string, number> = { LOW: 1, MEDIUM: 2, HIGH: 3, CRITICAL: 4 }

export default function TicketsPage() {
  const router = useRouter()
  const [statusFilter, setStatusFilter] = useState("")
  const [priorityFilter, setPriorityFilter] = useState("")
  const [categoryFilter, setCategoryFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(statusFilter ? { status: statusFilter } : {}),
    ...(priorityFilter ? { priority: priorityFilter } : {}),
    ...(categoryFilter ? { category: categoryFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useTickets(filters)
  const queryClient = useQueryClient()

  const bulkMutation = useMutation({
    mutationFn: async ({ ids, action }: { ids: string[]; action: string }) => {
      const promises = ids.map((id) => {
        if (action === "assign") {
          return api.post(`/api/v1/ticketing/tickets/${id}/assign`, {})
        }
        return api.post(`/api/v1/ticketing/tickets/${id}/${action}`)
      })
      await Promise.all(promises)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.tickets.lists() })
      toast({ title: "Success", description: `${variables.ids.length} items updated.` })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to perform action.", variant: "destructive" })
    },
  })

  const columns: Column<TicketDto>[] = [
    { id: "ticketNumber", header: "Ticket #", cell: (row) => `#${row.ticketNumber}` },
    { id: "subject", header: "Subject", accessorKey: "subject" },
    { id: "customerName", header: "Customer", accessorKey: "customerName" },
    {
      id: "priority",
      header: "Priority",
      cell: (row) => (
        <span className={cn("inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold", priorityColors[row.priority] || "bg-secondary text-secondary-foreground")}>
          {row.priority}
        </span>
      ),
      sortable: true,
    },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "assignedTo", header: "Assigned", cell: (row) => row.assignedTo || "-" },
    { id: "createdAt", header: "Created", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
  ]

  const filterConfig = [
    { id: "status", label: "Status", type: "select" as const, options: statusOptions, value: statusFilter, onChange: (v: string) => { setStatusFilter(v === "all" ? "" : v); setPage(1) }, placeholder: "All Statuses" },
    { id: "priority", label: "Priority", type: "select" as const, options: priorityOptions, value: priorityFilter, onChange: (v: string) => { setPriorityFilter(v === "all" ? "" : v); setPage(1) }, placeholder: "All Priorities" },
    { id: "category", label: "Category", type: "select" as const, options: categoryOptions, value: categoryFilter, onChange: (v: string) => { setCategoryFilter(v === "all" ? "" : v); setPage(1) }, placeholder: "All Categories" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Tickets" createHref="/tickets/new" createLabel="New Ticket" />

      <Card>
        <CardHeader className="pb-3">
          <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setPriorityFilter(""); setCategoryFilter(""); setPage(1) }} />
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={(data?.items ?? []).sort((a, b) => (priorityOrder[b.priority] || 0) - (priorityOrder[a.priority] || 0))}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No tickets found"
            emptyIcon={Ticket}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/tickets/${row.id}`)}
            pagination={{ page, pageSize, total: data?.total ?? data?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
            bulkActions={[
              { label: "Assign", onClick: (ids) => bulkMutation.mutate({ ids, action: "assign" }) },
              { label: "Close", onClick: (ids) => bulkMutation.mutate({ ids, action: "close" }), variant: "destructive" },
            ]}
          />
        </CardContent>
      </Card>
    </div>
  )
}
