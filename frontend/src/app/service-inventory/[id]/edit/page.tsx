"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditServicePage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Service Edit"
        subtitle="Service Edit is not available"
        backHref="/service-inventory"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Service Edit is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
