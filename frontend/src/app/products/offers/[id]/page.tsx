"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useOffer } from "@/api/hooks/useOffer"
import { useQuery } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import { AuditEntryDto } from "@/types/api"
import { formatCurrency } from "@/lib/formatters"

export default function OfferDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: offer, isLoading } = useOffer(id)

  const { data: auditEntries, error: auditError } = useQuery({
    queryKey: queryKeys.audit.entity("Offer", id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/Offer/${id}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!id,
  })

  const tabs = [
    {
      id: "overview",
      label: "Overview",
      content: (
        <EntityMetadata
          title="Offer Details"
          loading={isLoading}
          fields={[
            { label: "Name", value: offer?.name ?? "-" },
            { label: "Description", value: offer?.description ?? "-" },
            { label: "Product", value: offer?.productName ?? "-" },
            { label: "Price", value: offer ? formatCurrency(offer.price, offer.currency) : "-" },
            { label: "Billing Period", value: offer?.billingPeriod ?? "-" },
            { label: "Status", value: offer ? <StatusBadge status={offer.status} /> : "-" },
            { label: "Created", value: offer?.createdAt ? new Date(offer.createdAt).toLocaleDateString() : "-" },
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
              <p className="text-sm text-muted-foreground">No audit entries found.</p>
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
        title={offer?.name ?? "Offer"}
        subtitle={offer?.productName}
        status={offer?.status}
        backHref="/products/offers"
        editHref={`/products/offers/${id}/edit`}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
