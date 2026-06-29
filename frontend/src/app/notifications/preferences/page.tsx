"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"

export default function NotificationPreferencesPage() {
  const { data: preferences, isLoading } = useQuery({
    queryKey: ["notification-preferences"],
    queryFn: async () => {
      const res = await api.get("/api/v1/notifications/preferences")
      return res.data
    },
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Notification Preferences" backHref="/notifications" />
      <Card>
        <CardContent className="pt-6">
          {isLoading ? (
            <p className="text-muted-foreground">Loading...</p>
          ) : preferences ? (
            <pre className="text-sm">{JSON.stringify(preferences, null, 2)}</pre>
          ) : (
            <p className="text-muted-foreground">No preferences configured.</p>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
