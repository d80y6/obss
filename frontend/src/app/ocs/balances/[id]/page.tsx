"use client"

import { useParams } from "next/navigation"
import { useOcsBalance, useAdjustBalance } from "@/api/hooks/useOcs"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { useState } from "react"

export default function OcsBalanceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: balance, isLoading, error } = useOcsBalance(id)
  const adjustBalance = useAdjustBalance(id)
  const [adjustAmount, setAdjustAmount] = useState("")
  const [adjustDir, setAdjustDir] = useState<"credit" | "debit">("credit")

  if (isLoading) return <div className="p-6"><LoadingState rows={3} /></div>
  if (error || !balance) return <div className="p-6"><EmptyState title="Balance not found" description="Could not load balance details." /></div>

  return (
    <div className="flex-1 space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Balance Detail</h1>
          <p className="text-sm text-muted-foreground">Subscription: {balance.subscriptionId}</p>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <Card>
          <CardHeader><CardTitle className="text-sm text-muted-foreground">Current Balance</CardTitle></CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{balance.amount.toLocaleString()} <span className="text-sm font-normal text-muted-foreground">{balance.currency}</span></p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle className="text-sm text-muted-foreground">Reserved</CardTitle></CardHeader>
          <CardContent>
            <p className="text-3xl font-bold text-amber-600">{balance.reservedAmount.toLocaleString()} <span className="text-sm font-normal text-muted-foreground">{balance.currency}</span></p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle className="text-sm text-muted-foreground">Available</CardTitle></CardHeader>
          <CardContent>
            <p className="text-3xl font-bold text-green-600">{(balance.amount - balance.reservedAmount).toLocaleString()} <span className="text-sm font-normal text-muted-foreground">{balance.currency}</span></p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><CardTitle>Adjust Balance</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-4 items-end">
            <div>
              <label className="block text-sm font-medium mb-1">Amount</label>
              <input type="number" value={adjustAmount} onChange={(e) => setAdjustAmount(e.target.value)}
                className="rounded border px-3 py-2 text-sm w-40" min="0" step="0.01" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Direction</label>
              <select value={adjustDir} onChange={(e) => setAdjustDir(e.target.value as "credit" | "debit")}
                className="rounded border px-3 py-2 text-sm">
                <option value="credit">Credit (Add)</option>
                <option value="debit">Debit (Subtract)</option>
              </select>
            </div>
            <Button onClick={() => {
              const amt = parseFloat(adjustAmount)
              if (isNaN(amt) || amt <= 0) return
              adjustBalance.mutate({ amount: amt, direction: adjustDir, description: null })
              setAdjustAmount("")
            }} disabled={adjustBalance.isPending}>
              {adjustBalance.isPending ? "Adjusting..." : "Apply"}
            </Button>
          </div>
          {adjustBalance.isSuccess && <p className="text-sm text-green-600">Balance adjusted successfully.</p>}
          {adjustBalance.isError && <p className="text-sm text-red-600">Failed to adjust balance.</p>}
        </CardContent>
      </Card>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-sm">Details</CardTitle></CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">Balance Type</span><span className="capitalize">{balance.balanceType}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Status</span><span>{balance.status}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Valid From</span><span>{new Date(balance.validFrom).toLocaleString()}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Valid To</span><span>{balance.validTo ? new Date(balance.validTo).toLocaleString() : "Never"}</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Last Transaction</span><span>{balance.lastTransactionAt ? new Date(balance.lastTransactionAt).toLocaleString() : "None"}</span></div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
