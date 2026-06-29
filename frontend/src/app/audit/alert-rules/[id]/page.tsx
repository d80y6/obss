"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { AlertRuleDto } from "@/types/api"

export default function AlertRuleDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: rule, isLoading } = useQuery({
    queryKey: ["audit", "alert-rules", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/alert-rules/${id}`)
      return res.data as AlertRuleDto
    },
    enabled: !!id,
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader title={rule?.name ?? "Alert Rule"} subtitle={rule?.severity}
        status={rule?.isActive ? "ACTIVE" : "INACTIVE"}
        backHref="/audit/alert-rules" loading={isLoading} />
      <EntityMetadata loading={isLoading} fields={[
        { label: "Name", value: rule?.name ?? "-" },
        { label: "Severity", value: rule?.severity ?? "-" },
        { label: "Threshold", value: String(rule?.threshold ?? "-") },
        { label: "Window (minutes)", value: String(rule?.windowMinutes ?? "-") },
        { label: "Status", value: rule ? <StatusBadge status={rule.isActive ? "ACTIVE" : "INACTIVE"} /> : "-" },
      ]} />
    </div>
  )
}
