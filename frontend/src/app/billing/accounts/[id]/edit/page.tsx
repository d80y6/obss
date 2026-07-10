"use client"

import { useState, useEffect, useRef } from "react"
import { useParams, useRouter } from "next/navigation"
import { useBillingAccount, useUpdateBillingAccount } from "@/api/hooks/useBillingAccounts"

export default function EditBillingAccountPage() {
  const { id } = useParams<{ id: string }>()
  const router = useRouter()
  const { data: account, isLoading } = useBillingAccount(id)
  const updateAccount = useUpdateBillingAccount()
  const [form, setForm] = useState({
    name: "",
    creditLimit: 0,
    currency: "YER",
    description: "",
  })
  const initialized = useRef(false)

  useEffect(() => {
    if (account && !initialized.current) {
      initialized.current = true
      setForm({
        name: account.name,
        creditLimit: account.creditLimit,
        currency: account.currency,
        description: account.description ?? "",
      })
    }
  }, [account])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await updateAccount.mutateAsync({ id, ...form, description: form.description || null })
    router.push(`/billing/accounts/${id}`)
  }

  if (isLoading) return <div className="p-6 text-gray-500">Loading...</div>
  if (!account) return <div className="p-6 text-red-500">Account not found</div>

  return (
    <div className="max-w-lg mx-auto p-6">
      <h1 className="text-2xl font-semibold mb-6">Edit Billing Account</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
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
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
          <textarea
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
            rows={3}
          />
        </div>
        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={updateAccount.isPending}
            className="rounded bg-blue-600 px-6 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {updateAccount.isPending ? "Saving..." : "Save"}
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
