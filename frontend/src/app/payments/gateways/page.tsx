"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { Card, CardContent } from "@/components/ui/card"
import { usePaymentGateways } from "@/api/hooks/usePaymentGateways"
import type { PaymentGatewayInfo } from "@/api/generated/dto"
import { CreditCard } from "lucide-react"

export default function PaymentGatewaysPage() {
  const { data, isLoading } = usePaymentGateways()

  const columns: Column<PaymentGatewayInfo>[] = [
    { id: "provider", header: "Provider", accessorKey: "provider" },
    { id: "displayName", header: "Name", accessorKey: "displayName" },
    { id: "isAvailable", header: "Available", cell: (row) => <StatusBadge status={row.isAvailable ? "ACTIVE" : "INACTIVE"} /> },
  ]

  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Payment Gateways" backHref="/payments" createHref="/payments/gateways/new" createLabel="Register Gateway" />
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={data ?? []}
            loading={isLoading}
            emptyTitle="No gateways registered"
            emptyIcon={CreditCard}
            rowKey={(row) => row.provider}
          />
        </CardContent>
      </Card>
    </div>
  )
}
