"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditProvisioningTemplatePage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Template Edit"
        subtitle="Template Edit is not available"
        backHref="/provisioning/templates"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Template Edit is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
