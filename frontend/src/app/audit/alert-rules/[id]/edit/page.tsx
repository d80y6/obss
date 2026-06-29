"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditAuditAlertRulePage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Alert Rule Edit"
        subtitle="Alert Rule Edit is not available"
        backHref="/audit/alert-rules"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Alert Rule Edit is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
