"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { FilterBar } from "@/components/shared/FilterBar"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useUsers } from "@/api/hooks/useUsers"
import { IamUserDto } from "@/types/api"
import { Shield, ShieldCheck } from "lucide-react"
import { useRouter } from "next/navigation"
import Link from "next/link"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { toast } from "@/components/ui/toast"
import { queryKeys } from "@/lib/query-keys"

const statusOptions = [
  { label: "Active", value: "true" },
  { label: "Inactive", value: "false" },
]

export default function AdminPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [statusFilter, setStatusFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(statusFilter ? { isActive: statusFilter } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useUsers(filters)
  const queryClient = useQueryClient()

  const bulkMutation = useMutation({
    mutationFn: async ({ ids, action }: { ids: string[]; action: string }) => {
      const promises = ids.map((id) =>
        action === "activate"
          ? api.post(`/api/v1/iam/users/${id}/deactivate`, {})
          : api.put(`/api/v1/iam/users/${id}`, { isActive: action === "activate" })
      )
      await Promise.all(promises)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.lists() })
      toast({ title: "Success", description: `${variables.ids.length} items updated.` })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to perform action.", variant: "destructive" })
    },
  })

  const columns: Column<IamUserDto>[] = [
    { id: "username", header: "Username", accessorKey: "username", sortable: true },
    { id: "email", header: "Email", accessorKey: "email" },
    { id: "firstName", header: "First Name", accessorKey: "firstName" },
    { id: "lastName", header: "Last Name", accessorKey: "lastName" },
    {
      id: "isActive",
      header: "Status",
      cell: (row) => <StatusBadge status={row.isActive ? "ACTIVE" : "INACTIVE"} />,
      sortable: true,
    },
    {
      id: "role",
      header: "Role",
      cell: (row) => row.role ?? "-",
    },
  ]

  const filterConfig = [
    {
      id: "status",
      label: "Status",
      type: "select" as const,
      options: statusOptions,
      value: statusFilter,
      onChange: (v: string) => { setStatusFilter(v === "all" ? "" : v); setPage(1) },
      placeholder: "All Statuses",
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Admin"
        description="User and role management"
        createHref="/admin/users/new"
        createLabel="New User"
      />

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search users..." />
            <FilterBar filters={filterConfig} onClear={() => { setStatusFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No users found"
            emptyDescription="No users match the current filters."
            emptyIcon={Shield}
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            onRowClick={(row) => router.push(`/admin/users/${row.id}`)}
            pagination={{
              page,
              pageSize,
              total: data?.total ?? data?.items?.length ?? 0,
              onPageChange: setPage,
              onPageSizeChange: (size) => { setPageSize(size); setPage(1) },
            }}
            bulkActions={[
              { label: "Activate", onClick: (ids) => bulkMutation.mutate({ ids, action: "activate" }) },
              { label: "Deactivate", onClick: (ids) => bulkMutation.mutate({ ids, action: "deactivate" }), variant: "destructive" },
            ]}
          />
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-base">Role Management</CardTitle>
          <Button variant="outline" size="sm" asChild>
            <Link href="/admin/roles">
              <ShieldCheck className="mr-1 h-4 w-4" /> Manage Roles
            </Link>
          </Button>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Create and manage roles, assign permissions, and control access to system resources.
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
