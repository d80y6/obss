"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useAuditEntry } from "@/api/hooks/use-audit-entries"
import type { AuditEntryDto } from "@/api/generated/dto"

export default function AuditEntryDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: entry, isLoading } = useAuditEntry(id)

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
