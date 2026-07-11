"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { useNotification, useMarkNotificationRead } from "@/api/hooks/use-notifications"

export default function NotificationDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: notification, isLoading } = useNotification(id)

  const { data: auditEntries } = useAuditLog("Notification", id)

  const markReadMutation = useMarkNotificationRead()

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-6">
          <EntityMetadata
            title="Notification Details"
            loading={isLoading}
            fields={[
              { label: "Type", value: notification?.type ?? "-" },
              { label: "Channel", value: notification?.channel ?? "-" },
              { label: "Recipient", value: notification?.recipientId ?? "-" },
              { label: "Subject", value: notification?.title ?? "-" },
              { label: "Status", value: notification ? <StatusBadge status={notification.status} /> : "-" },
              { label: "Sent At", value: notification?.createdAt ? new Date(notification.createdAt).toLocaleString() : "-" },
              { label: "Read At", value: notification?.readAt ? new Date(notification.readAt).toLocaleString() : "Unread" },
            ]}
          />
          <Card>
            <CardHeader><CardTitle className="text-base">Message Body</CardTitle></CardHeader>
            <CardContent>
              <p className="text-sm whitespace-pre-wrap">{notification?.message ?? "-"}</p>
            </CardContent>
          </Card>
          {!notification?.readAt && (
            <Button onClick={() => markReadMutation.mutate(id)} disabled={markReadMutation.isPending}>
              Mark as Read
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
        title={notification?.title ?? "Notification"}
        subtitle={notification?.type}
        status={notification?.status}
        backHref="/notifications"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
