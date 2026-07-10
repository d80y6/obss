"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { useCheckServiceQualification } from "@/api/hooks/useServiceQualification"

export default function CheckServiceQualificationPage() {
  const router = useRouter()
  const mutation = useCheckServiceQualification()

  const [street, setStreet] = useState("")
  const [city, setCity] = useState("")
  const [state, setState] = useState("")
  const [postalCode, setPostalCode] = useState("")
  const [country, setCountry] = useState("YE")
  const [serviceName, setServiceName] = useState("")
  const [services, setServices] = useState<{ serviceId: string; serviceName: string }[]>([])
  const [description, setDescription] = useState("")
  const [error, setError] = useState("")

  const addService = () => {
    if (!serviceName.trim()) return
    setServices([...services, { serviceId: crypto.randomUUID(), serviceName: serviceName.trim() }])
    setServiceName("")
  }

  const removeService = (index: number) => {
    setServices(services.filter((_, i) => i !== index))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")

    if (!street.trim() || !city.trim() || !country.trim()) {
      setError("Street, city, and country are required.")
      return
    }
    if (services.length === 0) {
      setError("At least one service must be requested.")
      return
    }

    try {
      const result = await mutation.mutateAsync({
        customerId: "00000000-0000-0000-0000-000000000000",
        address: { street, city, state: state || null, postalCode: postalCode || null, country },
        requestedServices: services,
        description: description || null,
      })
      router.push(`/service-qualification/${result.id}`)
    } catch {
      setError("Failed to submit qualification check.")
    }
  }

  return (
    <div className="p-6 max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">Check Service Qualification</h1>

      <form onSubmit={handleSubmit} className="space-y-4">
        <fieldset className="border p-4 rounded">
          <legend className="font-semibold px-2">Service Address</legend>

          <div className="grid grid-cols-2 gap-4 mt-2">
            <div className="col-span-2">
              <label className="block text-sm font-medium mb-1">Street *</label>
              <input value={street} onChange={e => setStreet(e.target.value)} className="w-full border rounded px-3 py-2" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">City *</label>
              <input value={city} onChange={e => setCity(e.target.value)} className="w-full border rounded px-3 py-2" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">State</label>
              <input value={state} onChange={e => setState(e.target.value)} className="w-full border rounded px-3 py-2" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Postal Code</label>
              <input value={postalCode} onChange={e => setPostalCode(e.target.value)} className="w-full border rounded px-3 py-2" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Country *</label>
              <input value={country} onChange={e => setCountry(e.target.value)} className="w-full border rounded px-3 py-2" />
            </div>
          </div>
        </fieldset>

        <fieldset className="border p-4 rounded">
          <legend className="font-semibold px-2">Requested Services</legend>

          <div className="flex gap-2 mt-2">
            <input
              value={serviceName}
              onChange={e => setServiceName(e.target.value)}
              placeholder="Service name (e.g., Fiber 100Mbps)"
              className="flex-1 border rounded px-3 py-2"
              onKeyDown={e => e.key === "Enter" && (e.preventDefault(), addService())}
            />
            <button type="button" onClick={addService} className="bg-gray-200 px-4 py-2 rounded hover:bg-gray-300">
              Add
            </button>
          </div>

          {services.length > 0 && (
            <ul className="mt-2 space-y-1">
              {services.map((s, i) => (
                <li key={i} className="flex justify-between items-center bg-gray-50 px-3 py-2 rounded">
                  <span>{s.serviceName}</span>
                  <button type="button" onClick={() => removeService(i)} className="text-red-500 hover:text-red-700">&times;</button>
                </li>
              ))}
            </ul>
          )}
        </fieldset>

        <div>
          <label className="block text-sm font-medium mb-1">Description (optional)</label>
          <textarea value={description} onChange={e => setDescription(e.target.value)} className="w-full border rounded px-3 py-2" rows={2} />
        </div>

        {error && <div className="text-red-500 text-sm">{error}</div>}

        <button
          type="submit"
          disabled={mutation.isPending}
          className="bg-blue-600 text-white px-6 py-2 rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {mutation.isPending ? "Checking..." : "Check Qualification"}
        </button>
      </form>
    </div>
  )
}
