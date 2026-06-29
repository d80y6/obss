"use client"

import { useParams, useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { ProvisioningJobDto, AuditEntryDto } from "@/types/api"
import { useStartProvisioningJob, useRetryProvisioningJob, useProvisioningJob } from "@/api/hooks/use-provisioning-jobs"
import { toast } from "@/components/ui/toast"
import { Play, RotateCcw, FileText } from "lucide-react"
import Link from "next/link"

export default function ProvisioningJobDetailPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: job, isLoading } = useProvisioningJob(id)

  const { data: auditEntries } = useQuery({
    queryKey: queryKeys.audit.entity("ProvisioningJob", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/ProvisioningJob/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const startJob = useStartProvisioningJob()
  const retryJob = useRetryProvisioningJob()

  const handleStart = () => {
    startJob.mutate(id, {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: queryKeys.provisioning.jobs.all })
        toast({ title: "Job started" })
      },
      onError: () => toast({ title: "Failed to start job", variant: "destructive" }),
    })
  }

  const handleRetry = () => {
    retryJob.mutate(id, {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: queryKeys.provisioning.jobs.all })
        toast({ title: "Job retry initiated" })
      },
      onError: () => toast({ title: "Failed to retry job", variant: "destructive" }),
    })
  }

  const canStart = job?.status === "pending"
  const canRetry = job?.status === "failed"

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <div className="space-y-4">
          <div className="flex items-center gap-2">
            {canStart && (
              <Button size="sm" onClick={handleStart} disabled={startJob.isPending}>
                <Play className="mr-1 h-4 w-4" /> Start
              </Button>
            )}
            {canRetry && (
              <Button size="sm" variant="outline" onClick={handleRetry} disabled={retryJob.isPending}>
                <RotateCcw className="mr-1 h-4 w-4" /> Retry
              </Button>
            )}
            <Button variant="outline" size="sm" asChild>
              <Link href={`/provisioning/jobs/${id}/logs`}><FileText className="mr-1 h-4 w-4" /> Logs</Link>
            </Button>
          </div>
          <EntityMetadata
            title="Job Details"
            loading={isLoading}
            fields={[
              { label: "Name", value: job?.name ?? "-" },
              { label: "Type", value: job?.type ?? "-" },
              { label: "Status", value: job ? <StatusBadge status={job.status} /> : "-" },
              { label: "Started", value: job?.startedAt ? new Date(job.startedAt).toLocaleString() : "-" },
              { label: "Completed", value: job?.completedAt ? new Date(job.completedAt).toLocaleString() : "-" },
              { label: "Created", value: job?.createdAt ? new Date(job.createdAt).toLocaleDateString() : "-" },
            ]}
          />
        </div>
      ),
    },
    {
      id: "audit",
      label: "Audit",
      content: (
        <Card>
          <CardHeader><CardTitle className="text-base">Audit Trail</CardTitle></CardHeader>
          <CardContent>
            {(!auditEntries || auditEntries.length === 0) ? (
              <p className="text-sm text-muted-foreground">No audit entries.</p>
            ) : (
              auditEntries.map((e) => (
                <div key={e.id} className="border-b py-3">
                  <span className="font-medium">{e.action}</span>
                  <span className="text-sm text-muted-foreground ml-2">{new Date(e.performedAt).toLocaleString()}</span>
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
      <EntityHeader title={job?.name ?? "Provisioning Job"} subtitle={job?.type} status={job?.status} backHref="/provisioning" loading={isLoading} />
      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
