"use client"

import { useParams } from "next/navigation"
import { useEventSubscription } from "@/api/hooks/useEventManagement"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

export default function EventSubscriptionDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: subscription, isLoading, error } = useEventSubscription(id)

  if (isLoading) return <div className="p-6"><LoadingState rows={3} /></div>
  if (error || !subscription) return <div className="p-6"><EmptyState title="Subscription not found" description="Could not load subscription details." /></div>

  return (
    <div className="flex-1 space-y-6 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">{subscription.name}</h1>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Configuration</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Event Type</span><span className="font-mono text-xs">{subscription.eventType}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Endpoint</span><span className="text-xs max-w-[250px] truncate">{subscription.endpoint}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Status</span>
              <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                subscription.status === "Active" ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-600"
              }`}>{subscription.status}</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm">Activity</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Created</span><span>{new Date(subscription.createdAt).toLocaleString()}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Last Triggered</span><span>{subscription.lastTriggeredAt ? new Date(subscription.lastTriggeredAt).toLocaleString() : "Never"}</span></div>
          </CardContent>
        </Card>
      </div>

      {subscription.filter && Object.keys(subscription.filter).length > 0 && (
        <Card>
          <CardHeader><CardTitle className="text-sm">Filter Criteria</CardTitle></CardHeader>
          <CardContent>
            <pre className="text-xs bg-gray-50 p-3 rounded">{JSON.stringify(subscription.filter, null, 2)}</pre>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
