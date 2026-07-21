"use client"

import { useWebhookEvents } from "@/api/hooks/useEventManagement"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"

export default function WebhookEventsPage() {
  const { data: events, isLoading } = useWebhookEvents()

  if (isLoading) return <div className="p-6"><LoadingState rows={5} /></div>

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold">Webhook Events</h1>
      </div>

      <div className="overflow-x-auto rounded border">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Timestamp</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Event Type</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-center font-medium text-gray-600">Retries</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Last Attempt</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Error</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {events?.map((e) => (
              <tr key={e.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 text-xs">{new Date(e.createdAt).toLocaleString()}</td>
                <td className="px-4 py-3 font-mono text-xs">{e.eventType}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                    e.status === "Delivered" ? "bg-green-100 text-green-700" :
                    e.status === "Failed" ? "bg-red-100 text-red-700" :
                    e.status === "Pending" ? "bg-amber-100 text-amber-700" :
                    "bg-gray-100 text-gray-600"
                  }`}>{e.status}</span>
                </td>
                <td className="px-4 py-3 text-center">{e.retryCount}</td>
                <td className="px-4 py-3 text-xs text-muted-foreground">
                  {e.lastAttemptAt ? new Date(e.lastAttemptAt).toLocaleString() : "-"}
                </td>
                <td className="px-4 py-3 text-xs text-red-600 max-w-[200px] truncate" title={e.errorMessage || ""}>
                  {e.errorMessage || "-"}
                </td>
              </tr>
            ))}
            {(!events || events.length === 0) && (
              <tr><td colSpan={6} className="px-4 py-8 text-center">
                <EmptyState title="No webhook events" description="Events appear when subscribed events are triggered." />
              </td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
