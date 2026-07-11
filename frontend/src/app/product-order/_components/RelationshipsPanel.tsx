"use client"

import { useState } from "react"
import { ProductOrderItemRelationshipDto } from "@/api/hooks/useProductOrders"

interface RelationshipsPanelProps {
  relationships: ProductOrderItemRelationshipDto[]
  onAdd: (itemId: string, targetItemId: string, type: string) => void
  onRemove: (relationshipId: string) => void
}

export function RelationshipsPanel({ relationships, onAdd, onRemove }: RelationshipsPanelProps) {
  const [itemId, setItemId] = useState("")
  const [targetItemId, setTargetItemId] = useState("")
  const [type, setType] = useState("Requires")

  const handleAdd = (e: React.FormEvent) => {
    e.preventDefault()
    onAdd(itemId, targetItemId, type)
    setItemId("")
    setTargetItemId("")
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-medium">Item Relationships</h3>
      {relationships && relationships.length > 0 ? (
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b text-left text-gray-500">
              <th className="pb-2">Item ID</th>
              <th className="pb-2">Target ID</th>
              <th className="pb-2">Type</th>
              <th className="pb-2"></th>
            </tr>
          </thead>
          <tbody>
            {relationships.map((r) => (
              <tr key={r.id} className="border-b">
                <td className="py-2 font-mono text-xs">{r.productOrderItemId}</td>
                <td className="py-2 font-mono text-xs">{r.targetItemId}</td>
                <td className="py-2">
                  <span className="inline-flex rounded-full px-2 py-0.5 text-xs font-medium bg-blue-100 text-blue-700">
                    {r.type}
                  </span>
                </td>
                <td className="py-2 text-right">
                  <button onClick={() => onRemove(r.id)} className="text-xs text-red-600 hover:underline">
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p className="text-sm text-gray-400">No relationships</p>
      )}
      <form onSubmit={handleAdd} className="flex gap-2 items-end">
        <input
          placeholder="Item ID"
          value={itemId}
          onChange={(e) => setItemId(e.target.value)}
          className="rounded border px-2 py-1 text-xs flex-1"
          required
        />
        <input
          placeholder="Target Item ID"
          value={targetItemId}
          onChange={(e) => setTargetItemId(e.target.value)}
          className="rounded border px-2 py-1 text-xs flex-1"
          required
        />
        <select
          value={type}
          onChange={(e) => setType(e.target.value)}
          className="rounded border px-2 py-1 text-xs"
        >
          <option value="Requires">Requires</option>
          <option value="OptionalFor">Optional For</option>
          <option value="ReliesOn">Relies On</option>
        </select>
        <button type="submit" className="rounded bg-blue-600 px-3 py-1 text-xs text-white hover:bg-blue-700">
          Add
        </button>
      </form>
    </div>
  )
}
