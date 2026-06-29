"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { NotificationTemplateDto, AuditEntryDto } from "@/types/api"
import { FileText } from "lucide-react"

export default function NotificationTemplateDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: template, isLoading, error: templateError } = useQuery({
    queryKey: ["notification-templates", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/notifications/templates/${id}`)
      return res.data as NotificationTemplateDto
    },
    enabled: !!id,
  })

  const { data: auditEntries, error: auditError } = useQuery({
    queryKey: ["audit", "entity", "NotificationTemplate", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/NotificationTemplate/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Template Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: template?.name ?? "-" },
            { label: "Channel", value: template?.channel ?? "-" },
            { label: "Subject", value: template?.subject ?? "-" },
            { label: "Variables", value: (template?.variables ?? []).join(", ") || "None" },
          ]}
        />
      ),
    },
    {
      id: "body",
      label: "Body",
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Template Body</CardTitle></CardHeader>
          <CardContent>
            <pre className="text-sm whitespace-pre-wrap bg-muted p-4 rounded-md">{template?.body ?? "-"}</pre>
          </CardContent>
        </Card>
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Audit Trail</CardTitle></CardHeader>
          <CardContent>
            {(!auditEntries || auditEntries.length === 0) ? (
              <p className="text-sm text-muted-foreground">No audit entries.</p>
            ) : (
              auditEntries.map((e) => (
                <div key={e.id} className="border-b py-3">
                  <span className="font-medium">{e.action}</span>
                  <span className="text-sm text-muted-foreground ml-2">{new Date(e.performedAt).toLocaleString()}</span>
                  <p className="text-sm text-muted-foreground">By: {e.performedByName}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={template?.name ?? "Template"}
        subtitle={template?.channel}
        backHref="/notifications/templates"
        editHref={`/notifications/templates/${id}/edit`}
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
