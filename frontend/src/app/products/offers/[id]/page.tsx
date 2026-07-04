"use client"

import { useParams } from "next/navigation"
import { useRouter } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { DataTable } from "@/components/shared/DataTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useOffer } from "@/api/hooks/useOffer"
import { useDeleteOffer } from "@/api/hooks/useDeleteOffer"
import { toast } from "@/components/ui/toast"
import { useAuditLog } from "@/api/hooks/useAuditLog"
import { queryKeys } from "@/lib/query-keys"
import { useQueryClient } from "@tanstack/react-query"
import type { Column } from "@/components/shared/DataTable"
import type { ProductOfferingTermDto, BundledProductOfferingDto, OfferPricingDto } from "@/api/generated/dto"

export default function OfferDetailPage() {
  const params = useParams()
  const router = useRouter()
  const queryClient = useQueryClient()
  const id = params.id as string

  const { data: offer, isLoading } = useOffer(id)
  const deleteOffer = useDeleteOffer()

  const { data: auditEntries, error: auditError } = useAuditLog("Offer", id)

  const termColumns: Column<ProductOfferingTermDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "duration", header: "Duration", cell: (row) => `${row.duration} ${row.durationUnit}` },
    { id: "termType", header: "Type", accessorKey: "termType" },
    { id: "validFrom", header: "Valid From", cell: (row) => row.validFrom ? new Date(row.validFrom).toLocaleDateString() : "-" },
    { id: "validTo", header: "Valid To", cell: (row) => row.validTo ? new Date(row.validTo).toLocaleDateString() : "-" },
  ]

  const bundledColumns: Column<BundledProductOfferingDto>[] = [
    { id: "name", header: "Name", accessorKey: "name" },
    { id: "quantity", header: "Quantity", accessorKey: "quantity" },
    { id: "referralType", header: "Referral Type", cell: (row) => row.referralType ?? "-" },
  ]

  const pricingColumns: Column<OfferPricingDto>[] = [
    { id: "name", header: "Name", cell: (row) => row.name ?? "-" },
    { id: "pricingType", header: "Type", accessorKey: "pricingType" },
    { id: "currency", header: "Currency", accessorKey: "currency" },
    { id: "recurringPrice", header: "Recurring", cell: (row) => row.recurringPrice > 0 ? `$${row.recurringPrice.toFixed(2)}` : "-" },
    { id: "oneTimePrice", header: "One-Time", cell: (row) => row.oneTimePrice > 0 ? `$${row.oneTimePrice.toFixed(2)}` : "-" },
    { id: "usagePrice", header: "Usage", cell: (row) => row.usagePrice > 0 ? `$${row.usagePrice.toFixed(2)}` : "-" },
    { id: "status", header: "Status", cell: (row) => <StatusBadge status={row.isActive ? "Active" : "Inactive"} /> },
  ]

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
            { label: "Offer Type", value: offer?.offerType ?? "-" },
            { label: "Price", value: offer?.pricings?.[0] ? `${offer.pricings[0].currency} ${(offer.pricings[0].recurringPrice || offer.pricings[0].oneTimePrice || offer.pricings[0].usagePrice || 0).toFixed(2)}` : "-" },
            { label: "Billing Period", value: offer?.billingPeriod ?? "-" },
            { label: "Status", value: offer ? <StatusBadge status={offer.isActive ? "Active" : "Inactive"} /> : "-" },
            { label: "Created", value: offer?.createdAt ? new Date(offer.createdAt).toLocaleDateString() : "-" },
          ]}
        />
      ),
    },
    {
      id: "terms",
      label: "Terms",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Terms &amp; Conditions</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={termColumns}
              data={offer?.terms ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
              emptyTitle="No terms defined"
              emptyDescription="Add terms to specify contract durations and conditions."
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "bundled-offerings",
      label: "Bundled",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Bundled Offerings</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={bundledColumns}
              data={offer?.bundledOfferings ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
              emptyTitle="No bundled offerings"
              emptyDescription="Add bundled offerings to create product bundles."
            />
          </CardContent>
        </Card>
      ),
    },
    {
      id: "pricing",
      label: "Pricing",
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Price Configurations</CardTitle>
          </CardHeader>
          <CardContent>
            <DataTable
              columns={pricingColumns}
              data={offer?.pricings ?? []}
              loading={isLoading}
              rowKey={(row) => row.id}
              emptyTitle="No pricing defined"
              emptyDescription="Add pricing to define how this offer is charged."
            />
          </CardContent>
        </Card>
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
        subtitle={offer?.description ?? undefined}
        status={offer?.isActive ? "Active" : "Inactive"}
        backHref="/products/offers"
        editHref={`/products/offers/${id}/edit`}
        onDelete={() => {
          if (confirm("Are you sure you want to delete this offer?")) {
            deleteOffer.mutate(id, {
              onSuccess: () => {
                toast({ title: "Offer deleted", description: "Offer has been deleted." })
                queryClient.invalidateQueries({ queryKey: queryKeys.offers.lists() })
                router.push("/products/offers")
              },
              onError: () => {
                toast({ title: "Error", description: "Failed to delete offer.", variant: "destructive" })
              },
            })
          }
        }}
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
