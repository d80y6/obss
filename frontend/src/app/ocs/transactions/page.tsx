"use client"

import { useOcsTransactions } from "@/api/hooks/useOcs"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"

export default function OcsTransactionsPage() {
  const { data: transactions, isLoading } = useOcsTransactions()

  if (isLoading) return <div className="p-6"><LoadingState rows={5} /></div>

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold">Charging Transactions</h1>
      </div>

      <div className="overflow-x-auto rounded border">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Timestamp</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Type</th>
              <th className="px-4 py-3 text-right font-medium text-gray-600">Amount</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Direction</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Description</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Correlation ID</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {transactions?.map((t) => (
              <tr key={t.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 text-xs">{new Date(t.createdAt).toLocaleString()}</td>
                <td className="px-4 py-3 capitalize">{t.transactionType}</td>
                <td className={`px-4 py-3 text-right font-mono ${
                  t.direction === "debit" ? "text-red-600" : "text-green-600"
                }`}>
                  {t.direction === "debit" ? "-" : "+"}{t.amount.toLocaleString()}
                </td>
                <td className="px-4 py-3 capitalize">{t.direction}</td>
                <td className="px-4 py-3 text-muted-foreground max-w-xs truncate">{t.description || "-"}</td>
                <td className="px-4 py-3 text-xs font-mono">{t.correlationId || "-"}</td>
              </tr>
            ))}
            {(!transactions || transactions.length === 0) && (
              <tr><td colSpan={6} className="px-4 py-8 text-center">
                <EmptyState title="No transactions" description="Transactions appear when charging events are processed." />
              </td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
