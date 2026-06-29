"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent } from "@/components/ui/card"
import { useRoles } from "@/api/hooks/useRoles"
import { RoleDto } from "@/types/api"
import { Shield } from "lucide-react"
import { useRouter } from "next/navigation"

export default function RolesPage() {
  const router = useRouter()
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data, isLoading } = useRoles()

  const columns: Column<RoleDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "description", header: "Description", accessorKey: "description" },
    {
      id: "permissions",
      header: "Permissions",
      cell: (row) => (row.permissions ?? []).map((p) => p.name).join(", "),
    },
    {
      id: "createdAt",
      header: "Created",
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Roles"
        backHref="/admin"
        createHref="/admin/roles/new"
        createLabel="New Role"
      />

      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No roles found"
            emptyIcon={Shield}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/admin/roles/${row.id}`)}
          />
        </CardContent>
      </Card>
    </div>
  )
}
