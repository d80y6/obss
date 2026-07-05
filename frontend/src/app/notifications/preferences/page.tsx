"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent } from "@/components/ui/card"
import { useNotificationPreferences } from "@/api/hooks/use-notifications"

export default function NotificationPreferencesPage() {
  const { data: preferences, isLoading } = useNotificationPreferences()

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
