"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditApiRoutePage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Edit API Route"
        subtitle="Route editing is not available"
        backHref="/api-gateway/routes"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Editing API routes is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
