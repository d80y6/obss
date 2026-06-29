"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { SegmentDto } from "@/types/api"

interface SegmentAssignmentDto {
  customerId: string
  customerName?: string
  assignedAt: string
}

export default function SegmentDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: segment, isLoading, error: segmentError } = useQuery({
    queryKey: ["segments", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/segments/${id}`)
      return res.data as SegmentDto
    },
    enabled: !!id,
  })

  const { data: assignments, error: assignmentsError } = useQuery({
    queryKey: ["segments", id, "assignments"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/crm/segments/${id}/assignments`)
      return res.data as SegmentAssignmentDto[]
    },
    enabled: !!id,
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader title={segment?.name ?? "Segment"}
        subtitle={`${segment?.customerCount ?? 0} customers`}
        backHref="/customers/segments" loading={isLoading} />
      <EntityMetadata loading={isLoading} fields={[
        { label: "Name", value: segment?.name ?? "-" },
        { label: "Description", value: segment?.description ?? "-" },
        { label: "Customers", value: String(segment?.customerCount ?? 0) },
      ]} />
      <Card>
        <CardHeader><CardTitle className="text-base">Customer Assignments</CardTitle></CardHeader>
        <CardContent>
          {!assignments || assignments.length === 0 ? (
            <p className="text-sm text-muted-foreground">No customers assigned.</p>
          ) : (
            <div className="divide-y">
              {assignments.map((a, i) => (
                <div key={a.customerId ?? i} className="py-2 text-sm">
                  {a.customerName ?? a.customerId}
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
