"use client"

import { EntityHeader } from "@/components/shared/EntityHeader"
import { Card, CardContent } from "@/components/ui/card"

export default function EditOfferPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title="Catalog Offer Edit"
        subtitle="Catalog Offer Edit is not available"
        backHref="/products/offers"
      />
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Catalog Offer Edit is not available through this interface.
        </CardContent>
      </Card>
    </div>
  )
}
