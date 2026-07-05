"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent } from "@/components/ui/card"
import { usePartners } from "@/api/hooks/use-api-gateway"
import type { PartnerDto } from "@/api/generated"
import { Handshake } from "lucide-react"
import { useRouter } from "next/navigation"

const statusOptions = [
  { label: "Active", value: "ACTIVE" },
  { label: "Inactive", value: "INACTIVE" },
  { label: "Suspended", value: "SUSPENDED" },
]

export default function PartnersPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")

  const { data, isLoading } = usePartners()

  const filteredData = (data ?? []).filter((partner) => {
    if (search && !partner.name.toLowerCase().includes(search.toLowerCase())) return false
    if (statusFilter && (partner.isActive ? "ACTIVE" : "INACTIVE") !== statusFilter) return false
    return true
  })

  const columns: Column<PartnerDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "contactEmail", header: "Contact", accessorKey: "contactEmail" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.isActive ? "ACTIVE" : "INACTIVE"} /> },
    { id: "apiKeys", header: "API Keys", cell: (row) => String(row.apiKeys?.length ?? 0) },
  ]

  const filterConfig = [
    { id: "status", label: "Status", type: "select" as const, options: statusOptions, value: statusFilter, onChange: setStatusFilter, placeholder: "All Statuses" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Partners" backHref="/api-gateway" createHref="/api-gateway/partners/new" createLabel="New Partner" />
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <SearchBar placeholder="Search partners..." value={search} onChange={setSearch} />
            <FilterBar filters={filterConfig} onClear={() => setStatusFilter("")} />
          </div>
          <div className="mt-4">
            <DataTable
              columns={columns}
              data={filteredData}
              loading={isLoading}
              emptyTitle="No partners"
              emptyIcon={Handshake}
              rowKey={(row) => row.id}
              onRowClick={(row) => router.push(`/api-gateway/partners/${row.id}`)}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
