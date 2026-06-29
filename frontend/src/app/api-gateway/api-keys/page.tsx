"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQueryClient } from "@tanstack/react-query"
import { useApiKeys, useRevokeApiKey } from "@/api/hooks/use-api-gateway"
import { ApiKeyDto } from "@/types/api"
import { Key } from "lucide-react"
import { useRouter } from "next/navigation"
import { toast } from "@/components/ui/toast"

const statusOptions = [
  { label: "Active", value: "ACTIVE" },
  { label: "Revoked", value: "REVOKED" },
  { label: "Expired", value: "EXPIRED" },
]

export default function ApiKeysPage() {
  const router = useRouter()
  const queryClient = useQueryClient()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")

  const { data, isLoading } = useApiKeys()
  const revokeMutation = useRevokeApiKey()

  const filteredData = (data ?? []).filter((key) => {
    if (search && !key.name.toLowerCase().includes(search.toLowerCase())) return false
    if (statusFilter && key.status !== statusFilter) return false
    return true
  })

  const handleRevoke = (id: string, e: React.MouseEvent) => {
    e.stopPropagation()
    revokeMutation.mutate(id, {
      onSuccess: () => {
        toast({ title: "API key revoked" })
        queryClient.invalidateQueries({ queryKey: ["gateway", "api-keys"] })
      },
      onError: () => toast({ title: "Error", description: "Failed to revoke.", variant: "destructive" }),
    })
  }

  const columns: Column<ApiKeyDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "key", header: "Key", accessorKey: "key" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.status} /> },
    { id: "expiresAt", header: "Expires", cell: (row) => row.expiresAt ? new Date(row.expiresAt).toLocaleDateString() : "Never" },
    {
      id: "actions",
      header: "Actions",
      cell: (row) => row.status !== "REVOKED" ? (
        <Button variant="destructive" size="sm" onClick={(e) => handleRevoke(row.id, e)}>Revoke</Button>
      ) : null,
    },
  ]

  const filterConfig = [
    { id: "status", label: "Status", type: "select" as const, options: statusOptions, value: statusFilter, onChange: setStatusFilter, placeholder: "All Statuses" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="API Keys" backHref="/api-gateway" createHref="/api-gateway/api-keys/new" createLabel="New API Key" />
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <SearchBar placeholder="Search API keys..." value={search} onChange={setSearch} />
            <FilterBar filters={filterConfig} onClear={() => setStatusFilter("")} />
          </div>
          <div className="mt-4">
            <DataTable
              columns={columns}
              data={filteredData}
              loading={isLoading}
              emptyTitle="No API keys"
              emptyIcon={Key}
              rowKey={(row) => row.id}
              onRowClick={(row) => router.push(`/api-gateway/api-keys/${row.id}`)}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
