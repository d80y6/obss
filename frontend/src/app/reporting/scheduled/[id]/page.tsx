"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"

export default function ScheduledReportDetailPage() {
  const params = useParams()
  const id = params.id as string
  const { data: report, isLoading } = useQuery({
    queryKey: ["reporting", "schedule", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/reporting/schedule/${id}`)
      return res.data
    },
    enabled: !!id,
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader title="Scheduled Report" subtitle={report?.definitionId ?? "-"}
        backHref="/reporting/scheduled" loading={isLoading} />
      <EntityMetadata loading={isLoading} fields={[
        { label: "Cron Expression", value: report?.cron ?? "-" },
        { label: "Format", value: report?.format ?? "-" },
        { label: "Recipients", value: report?.recipients?.join(", ") ?? "-" },
        { label: "Status", value: report?.status ?? "-" },
      ]} />
    </div>
  )
}
