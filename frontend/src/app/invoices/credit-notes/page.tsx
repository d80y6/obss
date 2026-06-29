"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { CreditNoteDto } from "@/types/api"
import { FileX } from "lucide-react"

export default function CreditNotesPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["credit-notes"],
    queryFn: async () => {
      const res = await api.get("/api/v1/invoices/invoices/credit-notes")
      return res.data as CreditNoteDto[]
    },
  })

  const columns: Column<CreditNoteDto>[] = [
    { id: "creditNoteNumber", header: "Credit Note #", accessorKey: "creditNoteNumber" },
    { id: "invoiceNumber", header: "Invoice", accessorKey: "invoiceNumber" },
    { id: "amount", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.amount ?? 0).toLocaleString()}` },
    { id: "reason", header: "Reason", accessorKey: "reason" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Credit Notes" backHref="/invoices" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No credit notes"
            emptyIcon={FileX}
            rowKey={(row) => row.id}
          />
        </CardContent>
      </Card>
    </div>
  )
}
