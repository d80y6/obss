"use client"

import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { SearchBar } from "@/components/shared/SearchBar"
import { Card, CardContent } from "@/components/ui/card"
import { useNotificationTemplates } from "@/api/hooks/use-notification-templates"
import type { NotificationTemplateDto } from "@/api/generated"
import { FileText } from "lucide-react"
import { useRouter } from "next/navigation"

export default function NotificationTemplatesPage() {
  const router = useRouter()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const { data, isLoading, error } = useNotificationTemplates(search)

  const columns: Column<NotificationTemplateDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "subject", header: "Subject", accessorKey: "subject" },
    { id: "channel", header: "Channel", accessorKey: "channel" },
    { id: "variables", header: "Variables", cell: (row) => (row.variables ?? []).join(", ") },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Notification Templates" backHref="/notifications" createHref="/notifications/templates/new" createLabel="New Template" />
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-4">
            <SearchBar placeholder="Search templates..." value={search} onChange={(v) => { setSearch(v); setPage(1) }} />
          </div>
          <div className="mt-4">
            <DataTable
              columns={columns}
              data={data ?? []}
              loading={isLoading}
              error={error ? "Failed to load data." : undefined}
              emptyTitle="No templates"
              emptyIcon={FileText}
              rowKey={(row) => row.id}
              onRowClick={(row) => router.push(`/notifications/templates/${row.id}`)}
              pagination={{ page, pageSize, total: data?.length ?? 0, onPageChange: setPage, onPageSizeChange: (s) => { setPageSize(s); setPage(1) } }}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
