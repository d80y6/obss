"use client"

import Link from "next/link"
import { useBillingAccounts, useDeleteBillingAccount } from "@/api/hooks/useBillingAccounts"
import { LoadingState } from "@/components/shared/LoadingState"
import { EmptyState } from "@/components/shared/EmptyState"

export default function BillingAccountsPage() {
  const { data: accounts, isLoading } = useBillingAccounts()
  const deleteAccount = useDeleteBillingAccount()

  if (isLoading) return <div className="p-6"><LoadingState rows={5} /></div>

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold">Billing Accounts</h1>
        <Link
          href="/billing/accounts/new"
          className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
        >
          New Account
        </Link>
      </div>

      <div className="overflow-x-auto rounded border">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Name</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Type</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-right font-medium text-gray-600">Credit Limit</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Currency</th>
              <th className="px-4 py-3 text-center font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {accounts?.map((a) => (
              <tr key={a.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <Link href={`/billing/accounts/${a.id}`} className="text-blue-600 hover:underline font-medium">
                    {a.name}
                  </Link>
                </td>
                <td className="px-4 py-3 capitalize">{a.accountType.toLowerCase()}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                    a.status === "Active" ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-600"
                  }`}>
                    {a.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">{a.creditLimit.toLocaleString()}</td>
                <td className="px-4 py-3">{a.currency}</td>
                <td className="px-4 py-3 text-center">
                  <Link
                    href={`/billing/accounts/${a.id}/edit`}
                    className="text-blue-600 hover:underline text-xs mr-3"
                  >
                    Edit
                  </Link>
                  <button
                    onClick={() => { if (confirm("Delete this account?")) deleteAccount.mutate(a.id) }}
                    className="text-red-600 hover:underline text-xs"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
            {(!accounts || accounts.length === 0) && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center">
                  <EmptyState title="No billing accounts" description="Create one to get started." />
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
