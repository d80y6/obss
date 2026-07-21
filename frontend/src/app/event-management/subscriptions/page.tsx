"use client"

import Link from "next/link"
import { useEventSubscriptions, useCreateEventSubscription } from "@/api/hooks/useEventManagement"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"
import { useState } from "react"
import { Button } from "@/components/ui/button"

export default function EventSubscriptionsPage() {
  const { data: subscriptions, isLoading } = useEventSubscriptions()
  const createSubscription = useCreateEventSubscription()
  const [showNew, setShowNew] = useState(false)
  const [name, setName] = useState("")
  const [eventType, setEventType] = useState("")
  const [endpoint, setEndpoint] = useState("")

  if (isLoading) return <div className="p-6"><LoadingState rows={5} /></div>

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold">Event Subscriptions</h1>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={() => setShowNew(!showNew)}>
            {showNew ? "Cancel" : "New Subscription"}
          </Button>
          <Link href="/event-management"
            className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
            Back
          </Link>
        </div>
      </div>

      {showNew && (
        <div className="mb-6 rounded border p-4 space-y-3">
          <div className="grid gap-3 md:grid-cols-3">
            <div>
              <label className="block text-sm font-medium mb-1">Name</label>
              <input type="text" value={name} onChange={(e) => setName(e.target.value)}
                className="rounded border px-3 py-2 text-sm w-full" placeholder="My subscription" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Event Type</label>
              <input type="text" value={eventType} onChange={(e) => setEventType(e.target.value)}
                className="rounded border px-3 py-2 text-sm w-full" placeholder="order.created" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Endpoint URL</label>
              <input type="url" value={endpoint} onChange={(e) => setEndpoint(e.target.value)}
                className="rounded border px-3 py-2 text-sm w-full" placeholder="https://example.com/webhook" />
            </div>
          </div>
          <Button onClick={() => {
            createSubscription.mutate({ name, eventType, endpoint, status: "Active" })
            setName(""); setEventType(""); setEndpoint(""); setShowNew(false)
          }} disabled={!name || !eventType || !endpoint || createSubscription.isPending}>
            {createSubscription.isPending ? "Creating..." : "Create"}
          </Button>
        </div>
      )}

      <div className="overflow-x-auto rounded border">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Name</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Event Type</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Endpoint</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Last Triggered</th>
              <th className="px-4 py-3 text-center font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {subscriptions?.map((s) => (
              <tr key={s.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <Link href={`/event-management/subscriptions/${s.id}`} className="text-blue-600 hover:underline font-medium">
                    {s.name}
                  </Link>
                </td>
                <td className="px-4 py-3 font-mono text-xs">{s.eventType}</td>
                <td className="px-4 py-3 text-xs max-w-[200px] truncate" title={s.endpoint}>{s.endpoint}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                    s.status === "Active" ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-600"
                  }`}>{s.status}</span>
                </td>
                <td className="px-4 py-3 text-xs text-muted-foreground">
                  {s.lastTriggeredAt ? new Date(s.lastTriggeredAt).toLocaleString() : "Never"}
                </td>
                <td className="px-4 py-3 text-center">
                  <Link href={`/event-management/subscriptions/${s.id}`}
                    className="text-blue-600 hover:underline text-xs">View</Link>
                </td>
              </tr>
            ))}
            {(!subscriptions || subscriptions.length === 0) && (
              <tr><td colSpan={6} className="px-4 py-8 text-center">
                <EmptyState title="No subscriptions" description="Create an event subscription to receive webhook notifications." />
              </td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
