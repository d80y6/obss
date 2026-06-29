"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { toast } from "@/components/ui/toast"
import { useUsageRecord, useRateUsageRealtime } from "@/api/hooks/use-rating"
import { Zap } from "lucide-react"

export default function UsageRecordDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: record, isLoading } = useUsageRecord(id)
  const rateRealtimeMutation = useRateUsageRealtime()

  const handleRateRealTime = () => {
    rateRealtimeMutation.mutate(id, {
      onSuccess: () => {
        toast({ title: "Rating complete", description: "Usage record has been rated in real-time." })
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to rate usage record.", variant: "destructive" })
      },
    })
  }

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={`Usage Record`}
        subtitle={`ID: ${id}`}
        status={record?.status}
        backHref="/rating/usage"
        loading={isLoading}
      />

      <EntityMetadata
        title="Record Details"
        loading={isLoading}
        columns={2}
        fields={[
          { label: "Subscription ID", value: record?.subscriptionId ?? "-" },
          { label: "Usage Type", value: record?.usageType ?? "-" },
          { label: "Quantity", value: record ? `${record.quantity} ${record.unit}` : "-" },
          { label: "Unit", value: record?.unit ?? "-" },
          { label: "Status", value: record ? <StatusBadge status={record.status} /> : "-" },
          { label: "Recorded At", value: record?.recordedAt ? new Date(record.recordedAt).toLocaleDateString() : "-" },
        ]}
      />

      {record && record.status === "Unrated" && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Actions</CardTitle>
          </CardHeader>
          <CardContent>
            <Button
              onClick={handleRateRealTime}
              disabled={rateRealtimeMutation.isPending}
            >
              <Zap className="mr-1 h-4 w-4" />
              {rateRealtimeMutation.isPending ? "Rating..." : "Rate in Real-Time"}
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
