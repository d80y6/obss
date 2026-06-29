"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Clock } from "lucide-react"
import Link from "next/link"

export default function BillingCyclesPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Billing Cycles" backHref="/billing" />
      <Card>
        <CardContent className="pt-6 text-center py-12">
          <Clock className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-muted-foreground mb-4">Cycle listing is not available. Create a new billing cycle to get started.</p>
          <Button asChild>
            <Link href="/billing/cycles/new">New Cycle</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
