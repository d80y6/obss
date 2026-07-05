"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { useTenants } from "@/api/hooks/useTenants"
import type { TenantDto } from "@/api/generated"
import { Building2 } from "lucide-react"
import { useRouter } from "next/navigation"

export default function TenantsPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading } = useTenants()

  const columns: Column<TenantDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "slug", header: "Slug", accessorKey: "slug" },
    { id: "isActive", header: "Status", cell: (row) => <StatusBadge status={row.isActive ? "ACTIVE" : "INACTIVE"} /> },
    { id: "createdAt", header: "Created", cell: (row) => new Date(row.createdAt).toLocaleDateString() },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Tenants" backHref="/admin" createHref="/admin/tenants/new" createLabel="New Tenant" />

      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No tenants found"
            emptyIcon={Building2}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/admin/tenants/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
