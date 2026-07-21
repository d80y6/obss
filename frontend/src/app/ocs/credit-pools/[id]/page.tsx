"use client"

import { useParams } from "next/navigation"
import { useOcsCreditPool } from "@/api/hooks/useOcs"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

export default function OcsCreditPoolDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: pool, isLoading, error } = useOcsCreditPool(id)

  if (isLoading) return <div className="p-6"><LoadingState rows={3} /></div>
  if (error || !pool) return <div className="p-6"><EmptyState title="Credit pool not found" description="Could not load credit pool details." /></div>

  const pct = pool.totalAmount > 0 ? Math.round((pool.consumedAmount / pool.totalAmount) * 100) : 0

  return (
    <div className="flex-1 space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">{pool.name}</h1>
          <p className="text-sm text-muted-foreground capitalize">{pool.poolType} Pool</p>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <Card>
          <CardHeader><CardTitle className="text-sm text-muted-foreground">Total</CardTitle></CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{pool.totalAmount.toLocaleString()} <span className="text-sm font-normal text-muted-foreground">{pool.currency}</span></p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle className="text-sm text-muted-foreground">Consumed</CardTitle></CardHeader>
          <CardContent>
            <p className="text-3xl font-bold text-amber-600">{pool.consumedAmount.toLocaleString()} <span className="text-sm font-normal text-muted-foreground">{pool.currency}</span></p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle className="text-sm text-muted-foreground">Remaining</CardTitle></CardHeader>
          <CardContent>
            <p className="text-3xl font-bold text-green-600">{pool.remainingAmount.toLocaleString()} <span className="text-sm font-normal text-muted-foreground">{pool.currency}</span></p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle>Usage</CardTitle></CardHeader>
        <CardContent>
          <div className="space-y-2">
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">{pct}% consumed</span>
              <span>{pool.consumedAmount.toLocaleString()} / {pool.totalAmount.toLocaleString()}</span>
            </div>
            <div className="w-full h-3 rounded-full bg-gray-200">
              <div className={`h-3 rounded-full ${
                pct >= 90 ? "bg-red-500" : pct >= 70 ? "bg-amber-500" : "bg-green-500"
              }`} style={{ width: `${pct}%` }} />
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Status</span><span>{pool.status}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Valid From</span><span>{new Date(pool.validFrom).toLocaleString()}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Valid To</span><span>{pool.validTo ? new Date(pool.validTo).toLocaleString() : "Never"}</span></div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
