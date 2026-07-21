"use client"

import Link from "next/link"
import { useOcsBalances } from "@/api/hooks/useOcs"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"

export default function OcsBalancesPage() {
  const { data: balances, isLoading } = useOcsBalances()

  if (isLoading) return <div className="p-6"><LoadingState rows={5} /></div>

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold">Subscriber Balances</h1>
        <Link href="/ocs"
          className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
          Back to OCS
        </Link>
      </div>

      <div className="overflow-x-auto rounded border">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Subscription</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Balance Type</th>
              <th className="px-4 py-3 text-right font-medium text-gray-600">Amount</th>
              <th className="px-4 py-3 text-right font-medium text-gray-600">Reserved</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Valid Until</th>
              <th className="px-4 py-3 text-center font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {balances?.map((b) => (
              <tr key={b.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <Link href={`/ocs/balances/${b.id}`} className="text-blue-600 hover:underline font-medium">
                    {b.subscriptionId}
                  </Link>
                </td>
                <td className="px-4 py-3 capitalize">{b.balanceType}</td>
                <td className="px-4 py-3 text-right font-mono">{b.amount.toLocaleString()} {b.currency}</td>
                <td className="px-4 py-3 text-right font-mono text-amber-600">{b.reservedAmount.toLocaleString()}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                    b.status === "Active" ? "bg-green-100 text-green-700" :
                    b.status === "Expired" ? "bg-red-100 text-red-700" :
                    "bg-gray-100 text-gray-600"
                  }`}>
                    {b.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-xs text-muted-foreground">
                  {b.validTo ? new Date(b.validTo).toLocaleDateString() : "Never"}
                </td>
                <td className="px-4 py-3 text-center">
                  <Link href={`/ocs/balances/${b.id}`}
                    className="text-blue-600 hover:underline text-xs">View</Link>
                </td>
              </tr>
            ))}
            {(!balances || balances.length === 0) && (
              <tr><td colSpan={7} className="px-4 py-8 text-center">
                <EmptyState title="No balances" description="Balances appear when subscribers are created and charged." />
              </td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
