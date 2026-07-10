"use client"

import { useParams } from "next/navigation"
import Link from "next/link"
import {
  useBillingAccount,
  useBillingAccountBalance,
  useAddBillingAccountRelatedParty,
  useRemoveBillingAccountRelatedParty,
} from "@/api/hooks/useBillingAccounts"
import { useState } from "react"

export default function BillingAccountDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: account, isLoading } = useBillingAccount(id)
  const { data: balance } = useBillingAccountBalance(id)
  const addParty = useAddBillingAccountRelatedParty()
  const removeParty = useRemoveBillingAccountRelatedParty()
  const [newParty, setNewParty] = useState({ partyId: "", partyName: "", role: "" })

  if (isLoading) return <div className="p-6 text-gray-500">Loading...</div>
  if (!account) return <div className="p-6 text-red-500">Account not found</div>

  const handleAddParty = async (e: React.FormEvent) => {
    e.preventDefault()
    await addParty.mutateAsync({ billingAccountId: id, ...newParty })
    setNewParty({ partyId: "", partyName: "", role: "" })
  }

  return (
    <div className="max-w-3xl mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">{account.name}</h1>
          <p className="text-sm text-gray-500">{account.accountType} &middot; {account.status}</p>
        </div>
        <Link
          href={`/billing/accounts/${id}/edit`}
          className="rounded border px-4 py-2 text-sm text-gray-600 hover:bg-gray-50"
        >
          Edit
        </Link>
      </div>

      {/* Info card */}
      <div className="rounded border p-4 grid grid-cols-2 gap-4 text-sm">
        <div><span className="text-gray-500">Customer ID:</span> <span className="font-mono">{account.customerId}</span></div>
        <div><span className="text-gray-500">Currency:</span> {account.currency}</div>
        <div><span className="text-gray-500">Credit Limit:</span> {account.creditLimit.toLocaleString()}</div>
        <div><span className="text-gray-500">Description:</span> {account.description ?? "—"}</div>
        {account.accountHolder && (
          <>
            <div><span className="text-gray-500">Holder Name:</span> {account.accountHolder.name}</div>
            <div><span className="text-gray-500">Holder Email:</span> {account.accountHolder.email ?? "—"}</div>
          </>
        )}
      </div>

      {/* Balance widget */}
      {balance && (
        <div className="rounded border p-4">
          <h2 className="text-lg font-medium mb-3">Balance</h2>
          <div className="grid grid-cols-3 gap-4 text-sm">
            <div className="bg-blue-50 rounded p-3">
              <div className="text-blue-600 font-medium">Current</div>
              <div className="text-lg font-semibold">{balance.currentBalance.toLocaleString()} {balance.currency}</div>
            </div>
            <div className="bg-yellow-50 rounded p-3">
              <div className="text-yellow-600 font-medium">Outstanding</div>
              <div className="text-lg font-semibold">{balance.outstandingBalance.toLocaleString()} {balance.currency}</div>
            </div>
            <div className="bg-green-50 rounded p-3">
              <div className="text-green-600 font-medium">Available Credit</div>
              <div className="text-lg font-semibold">{balance.availableCredit.toLocaleString()} {balance.currency}</div>
            </div>
          </div>
        </div>
      )}

      {/* Related Parties */}
      <div className="rounded border p-4">
        <h2 className="text-lg font-medium mb-3">Related Parties</h2>
        {account.relatedParties && account.relatedParties.length > 0 ? (
          <table className="w-full text-sm mb-4">
            <thead>
              <tr className="border-b text-left text-gray-500">
                <th className="pb-2">Party ID</th>
                <th className="pb-2">Name</th>
                <th className="pb-2">Role</th>
                <th className="pb-2"></th>
              </tr>
            </thead>
            <tbody>
              {account.relatedParties.map((rp) => (
                <tr key={rp.partyId} className="border-b">
                  <td className="py-2 font-mono text-xs">{rp.partyId}</td>
                  <td className="py-2">{rp.partyName}</td>
                  <td className="py-2">{rp.role}</td>
                  <td className="py-2 text-right">
                    <button
                      onClick={() => removeParty.mutate({ billingAccountId: id, partyId: rp.partyId })}
                      className="text-xs text-red-600 hover:underline"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p className="text-sm text-gray-400 mb-4">No related parties</p>
        )}
        <form onSubmit={handleAddParty} className="flex gap-2 items-end">
          <input
            placeholder="Party ID"
            value={newParty.partyId}
            onChange={(e) => setNewParty({ ...newParty, partyId: e.target.value })}
            className="rounded border px-2 py-1 text-xs flex-1"
            required
          />
          <input
            placeholder="Name"
            value={newParty.partyName}
            onChange={(e) => setNewParty({ ...newParty, partyName: e.target.value })}
            className="rounded border px-2 py-1 text-xs flex-1"
            required
          />
          <input
            placeholder="Role"
            value={newParty.role}
            onChange={(e) => setNewParty({ ...newParty, role: e.target.value })}
            className="rounded border px-2 py-1 text-xs w-24"
            required
          />
          <button type="submit" className="rounded bg-blue-600 px-3 py-1 text-xs text-white hover:bg-blue-700">
            Add
          </button>
        </form>
      </div>
    </div>
  )
}
