"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProvisioningTemplateDto } from "@/api/generated"

export default function ProvisioningTemplateDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: template, isLoading } = useQuery({
    queryKey: ["provisioning", "templates", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/provisioning/templates/${id}`)
      return res.data as ProvisioningTemplateDto
    },
    enabled: !!id,
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader title={template?.name ?? "Template"} subtitle={template?.serviceType}
        backHref="/provisioning/templates" loading={isLoading} />
      <EntityMetadata loading={isLoading} fields={[
        { label: "Name", value: template?.name ?? "-" },
        { label: "Service Type", value: template?.serviceType ?? "-" },
        { label: "Description", value: template?.description ?? "-" },
      ]} />
    </div>
  )
}
