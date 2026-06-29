"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { useProvisioningTemplates } from "@/api/hooks/use-provisioning-templates"
import { ProvisioningTemplateDto } from "@/types/api"
import { FileJson } from "lucide-react"
import { useRouter } from "next/navigation"

export default function ProvisioningTemplatesPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const filters = {
    ...(search ? { search } : {}),
    page: String(page),
    pageSize: String(pageSize),
  }
  const { data, isLoading, error } = useProvisioningTemplates(filters)

  const columns: Column<ProvisioningTemplateDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "description", header: "Description", accessorKey: "description" },
    { id: "serviceType", header: "Service Type", accessorKey: "serviceType" },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Provisioning Templates" backHref="/provisioning" createHref="/provisioning/templates/new" createLabel="New Template" />
      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-wrap items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Search templates..." />
          </div>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            loading={isLoading}
            error={error ? "Failed to load data." : undefined}
            emptyTitle="No templates"
            emptyIcon={FileJson}
            rowKey={(row) => row.id}
            onRowClick={(row) => router.push(`/provisioning/templates/${row.id}`)}
            pagination={{ page, pageSize, total: data?.total ?? data?.items?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
          />
        </CardContent>
      </Card>
    </div>
  )
}
