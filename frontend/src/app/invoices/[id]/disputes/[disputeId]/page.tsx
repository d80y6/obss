"use client"

import { useParams, useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Button } from "@/components/ui/button"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { DisputeDto } from "@/types/api"
import { toast } from "@/components/ui/toast"
import { CheckCircle, XCircle } from "lucide-react"

export default function DisputeDetailPage() {
  const params = useParams()
  const router = useRouter()
  const invoiceId = params.id as string
  const disputeId = params.disputeId as string
  const queryClient = useQueryClient()

  const { data: dispute, isLoading } = useQuery({
    queryKey: ["invoices", invoiceId, "disputes", disputeId],
    queryFn: async () => {
      const res = await api.get(`/api/v1/invoices/invoices/disputes/${disputeId}`)
      return res.data as DisputeDto
    },
    enabled: !!invoiceId && !!disputeId,
  })

  const resolveMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/invoices/invoices/disputes/${disputeId}/resolve`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.disputes.all })
      toast({ title: "Dispute resolved" })
      router.push(`/invoices/${invoiceId}`)
    },
    onError: () => {
      toast({ title: "Failed to resolve dispute", variant: "destructive" })
    },
  })

  const rejectMutation = useMutation({
    mutationFn: async () => {
      await api.post(`/api/v1/invoices/invoices/disputes/${disputeId}/reject`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.invoices.disputes.all })
      toast({ title: "Dispute rejected" })
      router.push(`/invoices/${invoiceId}`)
    },
    onError: () => {
      toast({ title: "Failed to reject dispute", variant: "destructive" })
    },
  })

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={`Dispute ${disputeId.substring(0, 8)}`}
        subtitle={`Invoice ${dispute?.invoiceNumber ?? ""}`}
        status={dispute?.status}
        backHref={`/invoices/${invoiceId}`}
        loading={isLoading}
      />
      {dispute && (
        <EntityMetadata
          title="Dispute Details"
          loading={isLoading}
          fields={[
            { label: "Invoice", value: dispute.invoiceNumber },
            { label: "Reason", value: dispute.reason },
            { label: "Amount", value: `$${(dispute.amount ?? 0).toLocaleString()}` },
            { label: "Status", value: <StatusBadge status={dispute.status} /> },
            { label: "Resolution", value: dispute.resolution || "-" },
            { label: "Created", value: new Date(dispute.createdAt).toLocaleDateString() },
          ]}
        />
      )}
      {dispute && dispute.status === "OPEN" && (
        <div className="flex gap-2">
          <Button variant="default" size="sm" onClick={() => resolveMutation.mutate()}>
            <CheckCircle className="mr-1 h-4 w-4" /> Resolve
          </Button>
          <Button variant="destructive" size="sm" onClick={() => rejectMutation.mutate()}>
            <XCircle className="mr-1 h-4 w-4" /> Reject
          </Button>
        </div>
      )}
    </div>
  )
}
