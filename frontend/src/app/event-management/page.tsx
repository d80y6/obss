"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Bell, Webhook, Activity } from "lucide-react"
import Link from "next/link"

const modules = [
  {
    title: "Event Subscriptions",
    description: "Configure event subscriptions for notifications",
    href: "/event-management/subscriptions",
    icon: Bell,
    color: "text-blue-600",
    bgColor: "bg-blue-50",
  },
  {
    title: "Webhook Events",
    description: "View webhook event delivery status and retries",
    href: "/event-management/events",
    icon: Webhook,
    color: "text-green-600",
    bgColor: "bg-green-50",
  },
  {
    title: "Event Log",
    description: "Audit log of all system events",
    href: "#",
    icon: Activity,
    color: "text-purple-600",
    bgColor: "bg-purple-50",
  },
]

export default function EventManagementPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Event Management" description="Configure event subscriptions, webhooks, and view event history" />
      <div className="grid gap-6 md:grid-cols-3">
        {modules.map((mod) => {
          const Icon = mod.icon
          return (
            <Card key={mod.href} className="hover:shadow-md transition-shadow">
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className={`rounded-lg p-2 ${mod.bgColor}`}>
                    <Icon className={`h-5 w-5 ${mod.color}`} />
                  </div>
                  <CardTitle className="text-base">{mod.title}</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-sm text-muted-foreground">{mod.description}</p>
                <Button variant="outline" size="sm" className="w-full" asChild>
                  <Link href={mod.href}>Manage {mod.title}</Link>
                </Button>
              </CardContent>
            </Card>
          )
        })}
      </div>
    </div>
  )
}
