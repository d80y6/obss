"use client"

import { useParams } from "next/navigation"
import { EntityHeader } from "@/components/shared/EntityHeader"
import { EntityMetadata } from "@/components/shared/EntityMetadata"
import { EntityTabs } from "@/components/shared/EntityTabs"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { toast } from "@/components/ui/toast"
import { usePromotion, useDeactivatePromotion } from "@/api/hooks/use-rating"
import { formatCurrency, formatDate, formatDateTime } from "@/lib/formatters"

export default function PromotionDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: promotion, isLoading } = usePromotion(id)
  const deactivateMutation = useDeactivatePromotion()

  const handleDeactivate = () => {
    if (!promotion?.isActive) return
    deactivateMutation.mutate(id, {
      onSuccess: () => {
        toast({ title: "Promotion deactivated", description: "Promotion has been deactivated." })
      },
      onError: () => {
        toast({ title: "Error", description: "Failed to deactivate promotion.", variant: "destructive" })
      },
    })
  }

  const overviewContent = (
    <EntityMetadata
      title="Promotion Details"
      loading={isLoading}
      columns={2}
      fields={[
        { label: "Name", value: promotion?.name ?? "-" },
        { label: "Code", value: promotion?.code ?? "-" },
        { label: "Description", value: promotion?.description ?? "-" },
        { label: "Discount Type", value: promotion?.discountType ?? "-" },
        { label: "Discount Value", value: promotion ? formatCurrency(promotion.discountValue) : "-" },
        { label: "Status", value: promotion ? <StatusBadge status={promotion.isActive ? "Active" : "Inactive"} /> : "-" },
        { label: "Valid From", value: promotion?.validFrom ? formatDate(promotion.validFrom) : "-" },
        { label: "Valid To", value: promotion?.validTo ? formatDate(promotion.validTo) : "-" },
        { label: "Created", value: promotion?.createdAt ? formatDateTime(promotion.createdAt) : "-" },
        { label: "Updated", value: promotion?.updatedAt ? formatDateTime(promotion.updatedAt) : "-" },
      ]}
    />
  )

  const actionsContent = (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Actions</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-muted-foreground">
          Deactivate this promotion to prevent it from being applied to new orders.
        </p>
        <Button
          variant="destructive"
          size="sm"
          onClick={handleDeactivate}
          disabled={!promotion?.isActive || deactivateMutation.isPending}
        >
          {deactivateMutation.isPending ? "Deactivating..." : "Deactivate Promotion"}
        </Button>
      </CardContent>
    </Card>
  )

  const tabs = [
    { id: "overview", label: "Overview", content: overviewContent },
    { id: "actions", label: "Actions", content: actionsContent },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <EntityHeader
        title={promotion?.name ?? "Promotion"}
        subtitle={`Code: ${promotion?.code ?? "-"}`}
        status={promotion?.isActive ? "Active" : "Inactive"}
        backHref="/rating/promotions"
        loading={isLoading}
      />

      <EntityTabs tabs={tabs} defaultTab="overview" />
    </div>
  )
}
