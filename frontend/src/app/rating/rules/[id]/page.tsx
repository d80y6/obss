"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useRatingRule } from "@/api/hooks/use-rating"

export default function RatingRuleDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: rule, isLoading } = useRatingRule(id)

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={rule?.name ?? "Rating Rule"}
        subtitle={rule?.description}
        status={rule?.isActive ? "Active" : "Inactive"}
        backHref="/rating/rules"
        loading={isLoading}
      />

      <EntityMetadata
        title="Rule Details"
        loading={isLoading}
        columns={2}
        fields={[
          { label: "Name", value: rule?.name ?? "-" },
          { label: "Description", value: rule?.description ?? "-" },
          { label: "Priority", value: rule?.priority != null ? String(rule.priority) : "-" },
          { label: "Status", value: rule ? <StatusBadge status={rule.isActive ? "Active" : "Inactive"} /> : "-" },
          { label: "Created", value: rule?.createdAt ? new Date(rule.createdAt).toLocaleDateString() : "-" },
          { label: "Updated", value: rule?.updatedAt ? new Date(rule.updatedAt).toLocaleDateString() : "-" },
        ]}
      />
    </div>
  )
}
