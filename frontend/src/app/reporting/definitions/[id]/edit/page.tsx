"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditReportDefinitionPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Edit Report Definition"
        subtitle="Report editing is not available"
        backHref="/reporting/definitions"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Editing report definitions is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
