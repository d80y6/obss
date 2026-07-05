"use client"

import { useState, useCallback } from "react"
import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchBar } from "@/components/shared/SearchBar"
import { FilterBar } from "@/components/shared/FilterBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useDunningPolicies, useDeleteDunningPolicy } from "@/api/hooks/use-collections"
import type { DunningPolicyDto } from "@/api/generated"
import { Landmark } from "lucide-react"
import { toast } from "@/components/ui/toast"
import Link from "next/link"

export default function DunningPoliciesPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [activeFilter, setActiveFilter] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters: Record<string, string> = {
    ...(search ? { search } : {}),
    ...(activeFilter ? { activeOnly: activeFilter === "true" ? "true" : "false" } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }

  const { data, isLoading, error } = useDunningPolicies(filters)
  const deleteMutation = useDeleteDunningPolicy()

  const handleDelete = useCallback((id: string, name: string) => {
    if (!window.confirm(`Delete policy "${name}"? This action cannot be undone.`)) return
    deleteMutation.mutate(id, {
      onSuccess: () => {
        toast({ title: "Policy deleted successfully" })
      },
      onError: () => {
        toast({ title: "Failed to delete policy", variant: "destructive" })
      },
    })
  }, [deleteMutation])

  const columns: Column<DunningPolicyDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    {
      id: "isActive",
      header: "Status",
      cell: (row) => <StatusBadge status={row.isActive ? "ACTIVE" : "INACTIVE"} />,
    },
    { id: "maxDunningLevel", header: "Max Level", accessorKey: "maxDunningLevel" },
    { id: "daysBetweenActions", header: "Days Between Actions", accessorKey: "daysBetweenActions" },
    { id: "escalationAfterDays", header: "Escalation After Days", accessorKey: "escalationAfterDays" },
    {
      id: "actions",
      header: "Actions",
      cell: (row) => (
        <div className="flex gap-2">
          <Button variant="outline" size="sm" asChild>
            <Link href={`/collections/dunning-policies/${row.id}/edit`} onClick={(e) => e.stopPropagation()}>Edit</Link>
          </Button>
          <Button variant="destructive" size="sm" onClick={(e) => { e.stopPropagation(); handleDelete(row.id, row.name) }}>
            Delete
          </Button>
        </div>
      ),
    },
  ]

  const filterConfig = [
    {
      id: "activeOnly",
      label: "Status",
      type: "select" as const,
      options: [
        { label: "All", value: "all" },
        { label: "Active", value: "true" },
        { label: "Inactive", value: "false" },
      ],
      value: activeFilter || "all",
      onChange: (v: string) => { setActiveFilter(v === "all" ? "" : v); setPage(1) },
      placeholder: "All Policies",
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader
        title="Dunning Policies"
        createHref="/collections/dunning-policies/new"
        createLabel="New Policy"
      />
      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search policies..." />
            <FilterBar filters={filterConfig} onClear={() => { setActiveFilter(""); setPage(1) }} />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            error={error ? "Failed to load dunning policies." : undefined}
            emptyTitle="No dunning policies"
            emptyIcon={Landmark}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/collections/dunning-policies/${row.id}`)}
            pagination={{ page, pageSize, total: data?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
