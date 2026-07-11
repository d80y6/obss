"use client"

import { useState } from "react"
import { ProductOrderMilestoneDto } from "@/api/hooks/useProductOrders"

interface MilestonesPanelProps {
  milestones: ProductOrderMilestoneDto[]
  onAdd: (name: string, description: string, milestoneDate: string) => void
  onUpdate: (milestoneId: string, status: string) => void
  onRemove: (milestoneId: string) => void
}

const statusBadges: Record<string, string> = {
  Achieved: "bg-green-100 text-green-700",
  Missed: "bg-red-100 text-red-700",
  Pending: "bg-gray-100 text-gray-600",
  Cancelled: "bg-yellow-100 text-yellow-700",
}

export function MilestonesPanel({ milestones, onAdd, onUpdate, onRemove }: MilestonesPanelProps) {
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [milestoneDate, setMilestoneDate] = useState("")

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault()
    onAdd(name, description, milestoneDate)
    setName("")
    setDescription("")
    setMilestoneDate("")
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-medium">Milestones</h3>
      {milestones && milestones.length > 0 ? (
        <div className="space-y-2">
          {milestones.map((m) => (
            <div key={m.id} className="flex items-center justify-between rounded border p-3">
              <div className="flex-1">
                <div className="flex items-center gap-2">
                  <span className="font-medium text-sm">{m.name}</span>
                  <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${statusBadges[m.status] || "bg-gray-100 text-gray-600"}`}>
                    {m.status}
                  </span>
                </div>
                {m.description && <p className="text-xs text-gray-500 mt-0.5">{m.description}</p>}
                <p className="text-xs text-gray-400 mt-0.5">{new Date(m.milestoneDate).toLocaleDateString()}</p>
              </div>
              <div className="flex gap-1">
                {m.status === "Pending" && (
                  <>
                    <button onClick={() => onUpdate(m.id, "Achieved")} className="rounded border px-2 py-0.5 text-xs text-green-600 hover:bg-green-50">
                      Achieve
                    </button>
                    <button onClick={() => onUpdate(m.id, "Missed")} className="rounded border px-2 py-0.5 text-xs text-red-600 hover:bg-red-50">
                      Missed
                    </button>
                    <button onClick={() => onUpdate(m.id, "Cancelled")} className="rounded border px-2 py-0.5 text-xs text-yellow-600 hover:bg-yellow-50">
                      Cancel
                    </button>
                  </>
                )}
                <button onClick={() => onRemove(m.id)} className="rounded border px-2 py-0.5 text-xs text-red-600 hover:bg-red-50">
                  Remove
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-sm text-gray-400">No milestones</p>
      )}
      <form onSubmit={handleAdd} className="flex gap-2 items-end">
        <input
          placeholder="Milestone name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="rounded border px-2 py-1 text-xs flex-1"
          required
        />
        <input
          placeholder="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          className="rounded border px-2 py-1 text-xs flex-1"
        />
        <input
          type="date"
          value={milestoneDate}
          onChange={(e) => setMilestoneDate(e.target.value)}
          className="rounded border px-2 py-1 text-xs w-36"
          required
        />
        <button type="submit" className="rounded bg-blue-600 px-3 py-1 text-xs text-white hover:bg-blue-700">
          Add
        </button>
      </form>
    </div>
  )
}
