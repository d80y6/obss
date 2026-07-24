"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { useNasDevices } from "@/api/hooks/useNasDevices"
import { useDeleteNas } from "@/api/hooks/useDeleteNas"
import type { NasDto } from "@/api/generated/dto"
import { toast } from "@/components/ui/toast"
import { Trash2 } from "lucide-react"

export default function NasListPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const deleteNas = useDeleteNas()

  const filters: Record<string, string> = { page: String(page), pageSize: "20" }

  const { data, isLoading, error } = useNasDevices(filters)

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete NAS device "${name}"?`)) return
    await deleteNas.mutateAsync(id)
    toast({ title: "NAS deleted", description: `"${name}" has been removed.` })
  }

  const columns = [
    { id: "name", header: "Name", accessorKey: "name" as const, sortable: true },
    { id: "nasIpAddress", header: "IP Address", accessorKey: "nasIpAddress" as const },
    { id: "nasType", header: "Type", accessorKey: "nasType" as const },
    { id: "status", header: "Status", cell: (row: NasDto) => <StatusBadge status={row.status} /> },
    { id: "location", header: "Location", accessorKey: "location" as const },
    { id: "createdAt", header: "Created", accessorKey: "createdAt" as const },
  ]

  return (
    <div className="space-y-4">
      <PageHeader title="NAS Devices" createHref="/aaa/nas/new" createLabel="Register NAS" />
      <div className="flex gap-2">
        <SearchBar value={search} onChange={setSearch} placeholder="Search NAS devices..." />
      </div>
      <DataTable<NasDto>
        columns={columns}
        data={data?.items ?? []}
        loading={isLoading}
        error={error?.message}
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/aaa/nas/${row.id}`)}
        emptyTitle="No NAS devices registered"
        pagination={{
          page,
          pageSize: 20,
          total: data?.totalCount ?? 0,
          onPageChange: setPage,
          onPageSizeChange: () => {},
        }}
      />
    </div>
  )
}
