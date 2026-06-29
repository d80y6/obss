"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { TaxRuleDto } from "@/types/api"
import { Percent } from "lucide-react"

export default function TaxRulesPage() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const { data, isLoading, error } = useQuery({
    queryKey: ["billing-tax-rules"],
    queryFn: async () => {
      const res = await api.get("/api/v1/billing/tax-rules")
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as TaxRuleDto[], total }
    },
  })

  const columns: Column<TaxRuleDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "rate", header: "Rate (%)", cell: (row) => `${row.rate}%` },
    { id: "region", header: "Region", accessorKey: "region" },
    { id: "productCategory", header: "Product Category", accessorKey: "productCategory" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Tax Rules" backHref="/billing" createHref="/billing/tax-rules/new" createLabel="New Tax Rule" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No tax rules"
            emptyIcon={Percent}
            rowKey={(row) => row.id}
            pagination={{ page, pageSize, total: data?.total ?? data?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
