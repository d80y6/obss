"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent } from "@/components/ui/card"
import { useApiRoutes } from "@/api/hooks/use-api-gateway"
import { ApiRouteDto } from "@/types/api"
import { Waypoints } from "lucide-react"
import { useRouter } from "next/navigation"

const methodOptions = [
  { label: "GET", value: "GET" },
  { label: "POST", value: "POST" },
  { label: "PUT", value: "PUT" },
  { label: "DELETE", value: "DELETE" },
]

const statusOptions = [
  { label: "Active", value: "ACTIVE" },
  { label: "Inactive", value: "INACTIVE" },
]

export default function ApiRoutesPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [methodFilter, setMethodFilter] = useState("")
  const [statusFilter, setStatusFilter] = useState("")

  const { data, isLoading } = useApiRoutes()

  const filteredData = (data ?? []).filter((route) => {
    if (search && !route.path.toLowerCase().includes(search.toLowerCase())) return false
    if (methodFilter && route.method !== methodFilter) return false
    if (statusFilter && (route.isActive ? "ACTIVE" : "INACTIVE") !== statusFilter) return false
    return true
  })

  const columns: Column<ApiRouteDto>[] = [
    { id: "path", header: "Path", accessorKey: "path", sortable: true },
    { id: "method", header: "Method", accessorKey: "method" },
    { id: "targetModule", header: "Target Module", accessorKey: "targetModule" },
    { id: "targetPath", header: "Target Path", accessorKey: "targetPath" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.isActive ? "ACTIVE" : "INACTIVE"} /> },
    { id: "authRequired", header: "Auth", cell: (row) => row.requireAuthentication ? "Yes" : "No" },
  ]

  const filterConfig = [
    { id: "method", label: "Method", type: "select" as const, options: methodOptions, value: methodFilter, onChange: setMethodFilter, placeholder: "All Methods" },
    { id: "status", label: "Status", type: "select" as const, options: statusOptions, value: statusFilter, onChange: setStatusFilter, placeholder: "All Statuses" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="API Routes" backHref="/api-gateway" createHref="/api-gateway/routes/new" createLabel="New Route" />
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <SearchBar placeholder="Search routes..." value={search} onChange={setSearch} />
            <FilterBar filters={filterConfig} onClear={() => { setMethodFilter(""); setStatusFilter("") }} />
          </div>
          <div className="mt-4">
            <DataTable
              columns={columns}
              data={filteredData}
              loading={isLoading}
              emptyTitle="No routes"
              emptyIcon={Waypoints}
              rowKey={(row) => row.id}
              onRowClick={(row) => router.push(`/api-gateway/routes/${row.id}`)}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
