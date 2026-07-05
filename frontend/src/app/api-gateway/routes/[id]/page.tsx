"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import type { ApiRouteDto } from "@/api/generated"
import { useApiRoute } from "@/api/hooks/use-api-gateway"

export default function ApiRouteDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: route, isLoading } = useApiRoute(id)

  const { data: auditEntries, error: auditError } = useAuditLog("ApiRoute", id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Route Details"
          loading={isLoading}
          fields={[
            { label: "Path", value: route?.path ?? "-" },
            { label: "Method", value: route?.method ?? "-" },
            { label: "Target Module", value: route?.targetModule ?? "-" },
            { label: "Target Path", value: route?.targetPath ?? "-" },
            { label: "Status", value: route ? <StatusBadge status={route.isActive ? "ACTIVE" : "INACTIVE"} /> : "-" },
            { label: "Auth Required", value: route?.requireAuthentication ? "Yes" : "No" },
            { label: "Rate Limit", value: String(route?.rateLimitPerMinute ?? "-") },
          ]}
        />
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
        title={route?.path ?? "Route"}
        subtitle={`${route?.method} ${route?.path}`}
        status={route ? (route.isActive ? "ACTIVE" : "INACTIVE") : undefined}
        backHref="/api-gateway/routes"
        editHref={`/api-gateway/routes/${id}/edit`}
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
