"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditPartnerPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Edit Partner"
        subtitle="Partner editing is not available"
        backHref="/api-gateway/partners"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Editing partners is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
