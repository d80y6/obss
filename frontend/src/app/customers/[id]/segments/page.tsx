"use client"

import { useParams } from "next/navigation"
import { useState } from "react"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { useCustomerSegments } from "@/api/hooks/useCustomerSegments"
import type { SegmentDto } from "@/api/generated"
import { toast } from "@/components/ui/toast"

export default function CustomerSegmentsPage() {
  const params = useParams()
  const id = params.id as string
  const queryClient = useQueryClient()
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const { data: segments, isLoading } = useCustomerSegments(id)

  const assignMutation = useMutation({
    mutationFn: async (segmentId: string) => {
      await api.post(`/api/v1/crm/segments/${segmentId}/assign/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.segments(id) })
      toast({ title: "Segment assigned", description: "Customer has been assigned to the segment." })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to assign segment.", variant: "destructive" })
    },
  })

  const removeMutation = useMutation({
    mutationFn: async (segmentId: string) => {
      await api.delete(`/api/v1/crm/segments/${segmentId}/customers/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.customers.segments(id) })
      toast({ title: "Segment removed", description: "Customer has been removed from the segment." })
    },
    onError: () => {
      toast({ title: "Error", description: "Failed to remove segment.", variant: "destructive" })
    },
  })

  const columns: Column<SegmentDto>[] = [
    { id: "name", header: "Name", accessorKey: "name", sortable: true },
    { id: "description", header: "Description", accessorKey: "description" },
    { id: "customerCount", header: "Customers", cell: (row) => String(row.customerCount) },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Segment Assignments" backHref={`/customers/${id}`} />

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Assigned Segments</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={segments ?? []}
            loading={isLoading}
            emptyTitle="No segments assigned"
            rowKey={(row) => row.id}
            selectedIds={selectedIds}
            onSelectionChange={setSelectedIds}
            bulkActions={[
              { label: "Remove", onClick: (ids) => ids.forEach((sid) => removeMutation.mutate(sid)), variant: "destructive" },
            ]}
          />
        </CardContent>
      </Card>
    </div>
  )
}
