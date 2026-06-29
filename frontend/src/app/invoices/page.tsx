"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { MetricCard } from "@/components/shared/MetricCard"
import { FilterBar } from "@/components/shared/FilterBar"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useInvoices } from "@/api/hooks/useInvoices"
import { InvoiceDto } from "@/types/api"
import { FileText, FileX, FileCheck } from "lucide-react"
import { useRouter } from "next/navigation"
import Link from "next/link"
import { cn } from "@/lib/utils"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { toast } from "@/components/ui/toast"
import { queryKeys } from "@/lib/query-keys"

const statusOptions = [
  { label: "Draft", value: "DRAFT" },
  { label: "Finalized", value: "FINALIZED" },
  { label: "Sent", value: "SENT" },
  { label: "Paid", value: "PAID" },
  { label: "Overdue", value: "OVERDUE" },
  { label: "Cancelled", value: "CANCELLED" },
]

export default function InvoicesPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(statusFilter ? { status: statusFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useInvoices(filters)
  const queryClient = useQueryClient()

  const bulkMutation = useMutation({
    mutationFn: async ({ ids, action }: { ids: string[]; action: string }) => {
      const promises = ids.map((id) => api.post(`/api/v1/invoices/invoices/${id}/${action}`))
      await Promise.all(promises)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.lists() })
      toast({ title: "Success", description: `${variables.ids.length} items updated.` })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to perform action.", variant: "destructive" })
    },
  })
  const allInvoices = useInvoices({ pageSize: "1000" })

  const totalOutstanding = (allInvoices.data?.items ?? [])
    .filter((i) => i.status === "SENT" || i.status === "OVERDUE")
    .reduce((sum, i) => sum + i.totalAmount, 0)

  const totalOverdue = (allInvoices.data?.items ?? [])
    .filter((i) => i.status === "OVERDUE")
    .reduce((sum, i) => sum + i.totalAmount, 0)

  const totalPaid = (allInvoices.data?.items ?? [])
    .filter((i) => i.status === "PAID")
    .reduce((sum, i) => sum + i.totalAmount, 0)

  const columns: Column<InvoiceDto>[] = [
    { id: "invoiceNumber", header: "Invoice #", accessorKey: "invoiceNumber" },
    { id: "customerName", header: "Customer", accessorKey: "customerName" },
    { id: "totalAmount", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.totalAmount ?? 0).toLocaleString()}` },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "issueDate", header: "Date", cell: (row) => new Date(row.issueDate).toLocaleDateString() },
    {
      id: "dueDate",
      header: "Due Date",
      cell: (row) => (
        <span className={cn(row.status === "OVERDUE" && "font-semibold text-destructive")}>
          {new Date(row.dueDate).toLocaleDateString()}
        </span>
      ),
    },
  ]

  const filterConfig = [
    {
      id: "status",
      label: "Status",
      type: "select" as const,
      options: statusOptions,
      value: statusFilter,
      onChange: (v: string) => { setStatusFilter(v === "all" ? "" : v); setPage(1) },
      placeholder: "All Statuses",
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Invoices"
        createHref="/invoices/new"
        createLabel="New Invoice"
        actions={
          <>
            <Button variant="outline" size="sm" asChild>
              <Link href="/invoices/credit-notes"><FileX className="mr-1 h-4 w-4" /> Credit Notes</Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/invoices/disputes"><FileCheck className="mr-1 h-4 w-4" /> Disputes</Link>
            </Button>
          </>
        }
      />

      <div className="grid gap-4 md:grid-cols-3">
        <MetricCard title="Total Outstanding" value={`$${totalOutstanding.toLocaleString()}`} icon={FileText} loading={allInvoices.isLoading} />
        <MetricCard title="Total Overdue" value={`$${totalOverdue.toLocaleString()}`} icon={FileText} loading={allInvoices.isLoading} />
        <MetricCard title="Total Paid" value={`$${totalPaid.toLocaleString()}`} icon={FileText} loading={allInvoices.isLoading} />
      </div>

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search by invoice # or customer..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No invoices found"
            emptyIcon={FileText}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/invoices/${row.id}`)}
            pagination={{
              page,
              pageSize,
              total: data?.total ?? data?.items?.length ?? 0,
              onPageChange: setPage,
              onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
            }}
            bulkActions={[
              { label: "Send", onClick: (ids) => bulkMutation.mutate({ ids, action: "send" }) },
              { label: "Cancel", onClick: (ids) => bulkMutation.mutate({ ids, action: "cancel" }), variant: "destructive" },
            ]}
          />
        </CardContent>
      </Card>
    </div>
  )
}
