"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { useCreateBillingAccount } from "@/api/hooks/useBillingAccounts"

export default function NewBillingAccountPage() {
  const router = useRouter()
  const createAccount = useCreateBillingAccount()
  const [form, setForm] = useState({
    customerId: "",
    accountType: "Standard",
    name: "",
    creditLimit: 0,
    currency: "YER",
  })

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const result = await createAccount.mutateAsync(form)
    router.push(`/billing/accounts/${result.id}`)
  }

  return (
    <div className="max-w-lg mx-auto p-6">
      <h1 className="text-2xl font-semibold mb-6">New Billing Account</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Customer ID</label>
          <input
            type="text"
            value={form.customerId}
            onChange={(e) => setForm({ ...form, customerId: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Account Type</label>
          <select
            value={form.accountType}
            onChange={(e) => setForm({ ...form, accountType: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
          >
            <option value="Standard">Standard</option>
            <option value="Prepaid">Prepaid</option>
            <option value="Postpaid">Postpaid</option>
            <option value="Corporate">Corporate</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
          <input
            type="text"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Credit Limit</label>
          <input
            type="number"
            value={form.creditLimit}
            onChange={(e) => setForm({ ...form, creditLimit: Number(e.target.value) })}
            className="w-full rounded border px-3 py-2 text-sm"
            min="0"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Currency</label>
          <select
            value={form.currency}
            onChange={(e) => setForm({ ...form, currency: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
          >
            <option value="YER">YER</option>
            <option value="USD">USD</option>
            <option value="SAR">SAR</option>
          </select>
        </div>
        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={createAccount.isPending}
            className="rounded bg-blue-600 px-6 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {createAccount.isPending ? "Creating..." : "Create Account"}
          </button>
          <button
            type="button"
            onClick={() => router.back()}
            className="rounded border px-6 py-2 text-sm text-gray-600 hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  )
}
