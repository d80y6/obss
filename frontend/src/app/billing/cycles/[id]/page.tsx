"use client"

import { Card, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { Clock } from "lucide-react"
import Link from "next/link"
import { Button } from "@/components/ui/button"

export default function BillingCycleDetailPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Billing Cycle Details" backHref="/billing/cycles" />
      <Card>
        <CardContent className="pt-6 text-center py-12">
          <Clock className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
          <p className="text-muted-foreground mb-4">Cycle details are not available via API.</p>
          <Button asChild variant="outline">
            <Link href="/billing/cycles">Back to Cycles</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
