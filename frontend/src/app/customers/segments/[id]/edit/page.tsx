"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditSegmentPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Segment Edit"
        subtitle="Segment Edit is not available"
        backHref="/customers/segments"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Segment Edit is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
