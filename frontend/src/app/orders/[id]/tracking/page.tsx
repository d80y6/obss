"use client"

import { useParams } from "next/navigation"
import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useOrderFulfillment } from "@/api/hooks/useOrderFulfillment"

export default function OrderTrackingPage() {
  const params = useParams()
  const id = params.id as string

  const { data: fulfillment } = useOrderFulfillment(id)

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Order Fulfillment" backHref={`/orders/${id}`} />

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Fulfillment Status</CardTitle>
        </CardHeader>
        <CardContent>
          {!fulfillment ? (
            <p className="text-sm text-muted-foreground">No fulfillment data available.</p>
          ) : (
            <div className="space-y-4">
              <div className="border rounded-lg p-4">
                <div className="flex items-center justify-between mb-2">
                  <StatusBadge status={fulfillment.status} />
                </div>
                <p className="text-sm text-muted-foreground">
                  Started: {fulfillment.startedAt ? new Date(fulfillment.startedAt).toLocaleString() : "-"}
                </p>
                {fulfillment.completedAt && (
                  <p className="text-sm text-muted-foreground">
                    Completed: {new Date(fulfillment.completedAt).toLocaleString()}
                  </p>
                )}
                {fulfillment.errorMessage && (
                  <p className="text-sm text-destructive mt-1">{fulfillment.errorMessage}</p>
                )}
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
