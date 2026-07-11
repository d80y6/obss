"use client"

import { useParams } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { DataTable, Column } from "@/components/shared/DataTable"
import { useReconciliationDetail } from "@/api/hooks/useReconciliationDetail"
import type { ReconciliationItemDto } from "@/api/generated/dto"
import { ArrowLeftRight } from "lucide-react"

export default function ReconciliationDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: reconciliation, isLoading } = useReconciliationDetail(id)

  const itemColumns: Column<ReconciliationItemDto>[] = [
    { id: "externalReference", header: "Reference", accessorKey: "externalReference" },
    { id: "amount", header: "Amount", cell: (row) => `${row.currency ?? ""} ${(row.amount ?? 0).toLocaleString()}` },
    { id: "transactionDate", header: "Date", cell: (row) => new Date(row.transactionDate).toLocaleDateString() },
    { id: "description", header: "Description", cell: (row) => row.description ?? "-" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Reconciliation Detail" backHref="/payments/reconciliation" />
      <EntityMetadata
        title="Import Details"
        loading={isLoading}
        fields={[
          { label: "Import Source", value: reconciliation?.importSource ?? "-" },
          { label: "Import Date", value: reconciliation?.importDate ? new Date(reconciliation.importDate).toLocaleString() : "-" },
          { label: "File", value: reconciliation?.importFileName ?? "-" },
          { label: "Status", value: reconciliation ? <StatusBadge status={reconciliation.status} /> : "-" },
          { label: "Total Amount", value: reconciliation ? `${reconciliation.currency ?? ""} ${(reconciliation.totalImportAmount ?? 0).toLocaleString()}` : "-" },
          { label: "Reconciled", value: reconciliation ? `${reconciliation.currency ?? ""} ${(reconciliation.totalReconciledAmount ?? 0).toLocaleString()}` : "-" },
          { label: "Imported By", value: reconciliation?.importedBy ?? "-" },
        ]}
      />
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Transaction Items ({reconciliation?.items?.length ?? 0})</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={itemColumns}
            data={reconciliation?.items ?? []}
            loading={isLoading}
            emptyTitle="No items"
            emptyIcon={ArrowLeftRight}
            rowKey={(row) => row.id}
          />
        </CardContent>
      </Card>
    </div>
  )
}
