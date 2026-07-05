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
import { useBills } from "@/api/hooks/useBills"
import type { BillDto } from "@/api/generated"
import { FileText, CreditCard, Clock } from "lucide-react"
import Link from "next/link"
import { useRouter } from "next/navigation"

const statusOptions = [
  { label: "Paid", value: "PAID" },
  { label: "Pending", value: "PENDING" },
  { label: "Overdue", value: "OVERDUE" },
  { label: "Cancelled", value: "CANCELLED" },
]

export default function BillingPage() {
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

  const { data, isLoading, error } = useBills(filters)

  const allBills = useBills({ pageSize: "1000" })

  const totalOutstanding = (allBills.data?.items ?? [])
    .filter((b) => b.status === "PENDING" || b.status === "OVERDUE")
    .reduce((sum, b) => sum + b.totalAmount, 0)

  const totalOverdue = (allBills.data?.items ?? [])
    .filter((b) => b.status === "OVERDUE")
    .reduce((sum, b) => sum + b.totalAmount, 0)

  const totalPaid = (allBills.data?.items ?? [])
    .filter((b) => b.status === "PAID")
    .reduce((sum, b) => sum + b.totalAmount, 0)

  const columns: Column<BillDto>[] = [
    { id: "billNumber", header: "Bill #", accessorKey: "billNumber" },
    { id: "customerName", header: "Customer", accessorKey: "customerName" },
    { id: "period", header: "Period", accessorKey: "period" },
    { id: "totalAmount", header: "Total", cell: (row) => `${row.currency ?? ""} ${(row.totalAmount ?? 0).toLocaleString()}` },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "issueDate", header: "Date", cell: (row) => new Date(row.issueDate).toLocaleDateString() },
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
        title="Billing"
        actions={
          <>
            <Button variant="outline" size="sm" asChild>
              <Link href="/billing/cycles"><Clock className="mr-1 h-4 w-4" /> Cycles</Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/billing/jobs"><CreditCard className="mr-1 h-4 w-4" /> Jobs</Link>
            </Button>
          </>
        }
      />

      <div className="grid gap-4 md:grid-cols-3">
        <MetricCard
          title="Total Outstanding"
          value={`$${totalOutstanding.toLocaleString()}`}
          icon={FileText}
          loading={allBills.isLoading}
        />
        <MetricCard
          title="Total Overdue"
          value={`$${totalOverdue.toLocaleString()}`}
          icon={FileText}
          loading={allBills.isLoading}
        />
        <MetricCard
          title="Total Paid"
          value={`$${totalPaid.toLocaleString()}`}
          icon={FileText}
          loading={allBills.isLoading}
        />
      </div>

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search bills..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No bills found"
            emptyIcon={FileText}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/billing/${row.id}`)}
            pagination={{
              page,
              pageSize,
              total: data?.total ?? data?.items?.length ?? 0,
              onPageChange: setPage,
              onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
            }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
