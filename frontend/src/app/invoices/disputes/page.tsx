"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { DisputeDto } from "@/types/api"
import { FileCheck } from "lucide-react"

export default function DisputesPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["invoice-disputes"],
    queryFn: async () => {
      const res = await api.get("/api/v1/invoices/invoices/disputes")
      return res.data as DisputeDto[]
    },
  })

  const columns: Column<DisputeDto>[] = [
    { id: "invoiceNumber", header: "Invoice", accessorKey: "invoiceNumber" },
    { id: "reason", header: "Reason", accessorKey: "reason" },
    { id: "amount", header: "Amount", cell: (row) => `$${(row.amount ?? 0).toLocaleString()}` },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "createdAt", header: "Created", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Disputes" backHref="/invoices" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No disputes"
            emptyIcon={FileCheck}
            rowKey={(row) => row.id}
          />
        </CardContent>
      </Card>
    </div>
  )
}
