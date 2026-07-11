"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQueryClient } from "@tanstack/react-query"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { toast } from "@/components/ui/toast"
import { useApiKey, useRevokeApiKey } from "@/api/hooks/use-api-gateway"

export default function ApiKeyDetailPage() {
  const params = useParams()
  const id = params.id as string
  const queryClient = useQueryClient()
  const { data: apiKey, isLoading } = useApiKey(id)
  const revokeMutation = useRevokeApiKey()

  const { data: auditEntries } = useAuditLog("ApiKey", id)

  const handleRevoke = () => {
    revokeMutation.mutate(id, {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["gateway", "api-keys", id] })
        toast({ title: "API key revoked" })
      },
      onError: () => toast({ title: "Error", description: "Failed to revoke.", variant: "destructive" }),
    })
  }

  const maskedKey = apiKey?.key ? `...${apiKey.key.slice(-4)}` : "-"

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          <EntityMetadata
            title="API Key Details"
            loading={isLoading}
            fields={[
              { label: "Name", value: apiKey?.name ?? "-" },
              { label: "Key", value: maskedKey },
              { label: "Partner", value: apiKey?.partnerId ?? "-" },
              { label: "Status", value: apiKey ? <StatusBadge status={apiKey.status} /> : "-" },
              { label: "Created", value: apiKey?.createdAt ? new Date(apiKey.createdAt).toLocaleDateString() : "-" },
              { label: "Expires", value: apiKey?.expiresAt ? new Date(apiKey.expiresAt).toLocaleDateString() : "Never" },
            ]}
          />
          {apiKey?.status !== "REVOKED" && (
            <Button variant="destructive" onClick={handleRevoke} disabled={revokeMutation.isPending}>
              Revoke API Key
            </Button>
          )}
        </div>
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
        title={apiKey?.name ?? "API Key"}
        subtitle={maskedKey}
        status={apiKey?.status}
        backHref="/api-gateway/api-keys"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
