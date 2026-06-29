"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditWorkflowDefinitionPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Workflow Definition Edit"
        subtitle="Workflow Definition Edit is not available"
        backHref="/workflow/definitions"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Workflow Definition Edit is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
