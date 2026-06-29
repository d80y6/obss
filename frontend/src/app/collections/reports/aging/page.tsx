"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { Calendar } from "lucide-react"

interface AgingBucket {
  bucketName: string
  customerCount: number
  totalOverdueAmount: number
  currency: string
}

export default function AgingReportPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["collections-aging"],
    queryFn: async () => {
      const res = await api.get("/api/v1/collections/reports/aging")
      const body = res.data as { buckets?: AgingBucket[] }
      return body.buckets ?? []
    },
  })

  const bucketColumns: Column<AgingBucket>[] = [
    { id: "bucket", header: "Aging Bucket", accessorKey: "bucketName" },
    { id: "customerCount", header: "Customers", cell: (row) => String(row.customerCount) },
    { id: "totalAmount", header: "Total Amount", cell: (row) => `${row.currency ?? ""} ${(row.totalOverdueAmount ?? 0).toLocaleString()}` },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Aging Report" backHref="/collections" />
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Aging Summary</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={bucketColumns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No aging data"
            emptyIcon={Calendar}
            rowKey={(row) => row.bucketName}
          />
        </CardContent>
      </Card>
    </div>
  )
}
