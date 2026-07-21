"use client"

import Link from "next/link"
import { useOcsCreditPools } from "@/api/hooks/useOcs"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"

export default function OcsCreditPoolsPage() {
  const { data: pools, isLoading } = useOcsCreditPools()

  if (isLoading) return <div className="p-6"><LoadingState rows={5} /></div>

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold">Credit Pools</h1>
        <Link href="/ocs"
          className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
          Back to OCS
        </Link>
      </div>

      <div className="overflow-x-auto rounded border">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Name</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Pool Type</th>
              <th className="px-4 py-3 text-right font-medium text-gray-600">Total</th>
              <th className="px-4 py-3 text-right font-medium text-gray-600">Consumed</th>
              <th className="px-4 py-3 text-right font-medium text-gray-600">Remaining</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-center font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {pools?.map((p) => {
              const pct = p.totalAmount > 0 ? Math.round((p.consumedAmount / p.totalAmount) * 100) : 0
              return (
                <tr key={p.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <Link href={`/ocs/credit-pools/${p.id}`} className="text-blue-600 hover:underline font-medium">
                      {p.name}
                    </Link>
                  </td>
                  <td className="px-4 py-3 capitalize">{p.poolType}</td>
                  <td className="px-4 py-3 text-right font-mono">{p.totalAmount.toLocaleString()}</td>
                  <td className="px-4 py-3 text-right font-mono">{p.consumedAmount.toLocaleString()}</td>
                  <td className="px-4 py-3 text-right font-mono">{p.remainingAmount.toLocaleString()}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <div className="w-16 h-1.5 rounded-full bg-gray-200">
                        <div className={`h-1.5 rounded-full ${
                          pct >= 90 ? "bg-red-500" : pct >= 70 ? "bg-amber-500" : "bg-green-500"
                        }`} style={{ width: `${pct}%` }} />
                      </div>
                      <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                        p.status === "Active" ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-600"
                      }`}>{p.status}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-center">
                    <Link href={`/ocs/credit-pools/${p.id}`}
                      className="text-blue-600 hover:underline text-xs">View</Link>
                  </td>
                </tr>
              )
            })}
            {(!pools || pools.length === 0) && (
              <tr><td colSpan={7} className="px-4 py-8 text-center">
                <EmptyState title="No credit pools" description="Credit pools are created through product offerings." />
              </td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
