"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { AuditEntryDto } from "@/types/api"

export default function AuditEntryDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: entry, isLoading } = useQuery({
    queryKey: ["audit", "entries", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entries/${id}`)
      return res.data as AuditEntryDto
    },
    enabled: !!id,
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={`Audit Entry`}
        subtitle={entry?.action}
        backHref="/audit"
        loading={isLoading}
      />
      <EntityMetadata
        title="Entry Details"
        loading={isLoading}
        fields={[
          { label: "Action", value: entry?.action ?? "-" },
          { label: "Entity Type", value: entry?.entityType ?? "-" },
          { label: "Entity ID", value: entry?.entityId ?? "-" },
          { label: "Actor", value: entry?.performedByName ?? "-" },
          { label: "Actor ID", value: entry?.performedById ?? "-" },
          { label: "Timestamp", value: entry?.performedAt ? new Date(entry.performedAt).toLocaleString() : "-" },
        ]}
      />
      {entry?.changes && (
        <Card>
          <CardHeader><CardTitle className="text-base">Changes</CardTitle></CardHeader>
          <CardContent>
            <pre className="text-sm whitespace-pre-wrap bg-muted p-4 rounded-md">
              {entry.changes}
            </pre>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
