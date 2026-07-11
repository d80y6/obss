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
import type { ApiKeyDto } from "@/api/generated"
import { usePartner } from "@/api/hooks/use-api-gateway"

export default function PartnerDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: partner, isLoading } = usePartner(id)

  const { data: apiKeys } = useQuery({
    queryKey: ["gateway", "api-keys", "by-partner", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/gateway/api-keys?partnerId=${id}`)
      return res.data as ApiKeyDto[]
    },
    enabled: !!id,
  })

  const { data: auditEntries } = useAuditLog("Partner", id)

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Partner Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: partner?.name ?? "-" },
            { label: "Contact Email", value: partner?.contactEmail ?? "-" },
            { label: "Status", value: partner ? <StatusBadge status={partner.isActive ? "ACTIVE" : "INACTIVE"} /> : "-" },
            { label: "API Keys", value: String(partner?.apiKeys?.length ?? 0) },
          ]}
        />
      ),
    },
    {
      id: "apiKeys",
      label: `API Keys (${(apiKeys ?? []).length})`,
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">API Keys</CardTitle></CardHeader>
          <CardContent>
            {(!apiKeys || apiKeys.length === 0) ? (
              <p className="text-sm text-muted-foreground">No API keys for this partner.</p>
            ) : (
              <div className="divide-y">
                {apiKeys.map((k) => (
                  <div key={k.id} className="py-2 flex justify-between">
                    <div>
                      <span className="font-medium">{k.name}</span>
                      <span className="text-sm text-muted-foreground ml-2">...{k.key.slice(-4)}</span>
                    </div>
                    <StatusBadge status={k.status} />
                  </div>
                ))}
              </div>
            )}
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
        title={partner?.name ?? "Partner"}
        subtitle={partner?.contactEmail}
        status={partner ? (partner.isActive ? "ACTIVE" : "INACTIVE") : undefined}
        backHref="/api-gateway/partners"
        editHref={`/api-gateway/partners/${id}/edit`}
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
