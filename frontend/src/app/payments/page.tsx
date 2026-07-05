"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { usePayments } from "@/api/hooks/usePayments"
import type { PaymentDto } from "@/api/generated/dto"
import { CreditCard, ArrowLeftRight, RotateCcw, Plus } from "lucide-react"
import { useRouter } from "next/navigation"
import Link from "next/link"

const statusOptions = [
  { label: "Completed", value: "COMPLETED" },
  { label: "Pending", value: "PENDING" },
  { label: "Failed", value: "FAILED" },
  { label: "Refunded", value: "REFUNDED" },
]

const methodOptions = [
  { label: "Credit Card", value: "CREDIT_CARD" },
  { label: "Debit Card", value: "DEBIT_CARD" },
  { label: "Bank Transfer", value: "BANK_TRANSFER" },
  { label: "Cash", value: "CASH" },
  { label: "Check", value: "CHECK" },
  { label: "Other", value: "OTHER" },
]

export default function PaymentsPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")
  const [methodFilter, setMethodFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(statusFilter ? { status: statusFilter } : {}),
    ...(methodFilter ? { method: methodFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = usePayments(filters)

  const columns: Column<PaymentDto>[] = [
    { id: "paymentNumber", header: "Payment #", accessorKey: "paymentNumber" },
    { id: "customerName", header: "Customer", accessorKey: "customerName" },
    { id: "amount", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.amount ?? 0).toLocaleString()}` },
    { id: "method", header: "Method", cell: (row) => (row.method ?? "").replace(/_/g, " ") },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "paidAt", header: "Date", cell: (row) => new Date(row.paidAt || row.createdAt).toLocaleDateString() },
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
    {
      id: "method",
      label: "Method",
      type: "select" as const,
      options: methodOptions,
      value: methodFilter,
      onChange: (v: string) => { setMethodFilter(v === "all" ? "" : v); setPage(1) },
      placeholder: "All Methods",
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Payments"
        createHref="/payments/new"
        createLabel="Record Payment"
        actions={
          <>
            <Button variant="outline" size="sm" asChild>
              <Link href="/payments/reconciliation"><ArrowLeftRight className="mr-1 h-4 w-4" /> Reconciliation</Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link href="/payments/refunds"><RotateCcw className="mr-1 h-4 w-4" /> Refunds</Link>
            </Button>
          </>
        }
      />

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search payments..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setMethodFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No payments found"
            emptyIcon={CreditCard}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/payments/${row.id}`)}
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
