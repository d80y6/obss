"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { ServiceDto } from "@/types/api"
import { Network } from "lucide-react"
import { useRouter } from "next/navigation"

export default function ServicesPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const { data, isLoading, error } = useQuery({
    queryKey: ["services", page, pageSize],
    queryFn: async () => {
      const res = await api.get(`/api/v1/service-inventory/services?page=${page}&pageSize=${pageSize}`)
      const total = res.headers['x-total-count'] ? parseInt(res.headers['x-total-count'], 10) : null
      return { items: res.data as ServiceDto[], total }
    },
  })

  const columns: Column<ServiceDto>[] = [
    { id: "serviceIdentifier", header: "Name", accessorKey: "serviceIdentifier", sortable: true },
    { id: "serviceType", header: "Type", accessorKey: "serviceType" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} />, sortable: true },
    { id: "createdAt", header: "Created", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Services" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No services"
            emptyIcon={Network}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/services/${row.id}`)}
            pagination={{ page, pageSize, total: data?.total ?? data?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
