"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { BillingJobDto, AuditEntryDto } from "@/types/api"

export default function BillingJobDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: job, isLoading, error: jobError } = useQuery({
    queryKey: queryKeys.billing.jobs.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/billing/jobs/${id}`)
      return res.data as BillingJobDto
    },
    enabled: !!id,
  })

  const { data: auditEntries, error: auditError } = useQuery({
    queryKey: queryKeys.audit.entity("BillingJob", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/BillingJob/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const percentComplete = job ? (job.totalProcessed + job.totalErrors > 0
    ? Math.round((job.totalProcessed / (job.totalProcessed + job.totalErrors)) * 100)
    : 0) : 0

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Job Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: job?.name ?? "-" },
            { label: "Type", value: job?.type ?? "-" },
            { label: "Status", value: job ? <StatusBadge status={job.status} /> : "-" },
            { label: "Progress", value: job ? `${percentComplete}% (${job.totalProcessed} processed, ${job.totalErrors} errors)` : "-" },
            { label: "Started At", value: job?.startedAt ? new Date(job.startedAt).toLocaleString() : "-" },
            { label: "Completed At", value: job?.completedAt ? new Date(job.completedAt).toLocaleString() : "-" },
            { label: "Created", value: job?.createdAt ? new Date(job.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Audit Trail</CardTitle>
          </CardHeader>
          <CardContent>
            {(auditEntries ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No audit entries.</p>
            ) : (
              auditEntries?.map((entry) => (
                <div key={entry.id} className="border-b py-3">
                  <div className="flex justify-between">
                    <span className="font-medium">{entry.action}</span>
                    <span className="text-sm text-muted-foreground">{new Date(entry.performedAt).toLocaleString()}</span>
                  </div>
                  <p className="text-sm text-muted-foreground">By: {entry.performedByName}</p>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      ),
    },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={job?.name ?? "Billing Job"}
        subtitle={`Type: ${job?.type ?? ""}`}
        status={job?.status}
        backHref="/billing/jobs"
        loading={isLoading}
      />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
