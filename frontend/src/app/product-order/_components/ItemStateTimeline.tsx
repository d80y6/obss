"use client"

import { ProductOrderItemDto } from "@/api/hooks/useProductOrders"

interface ItemStateTimelineProps {
  items: ProductOrderItemDto[]
  onTransition: (itemId: string, action: string, data?: Record<string, string>) => void
}

const stateColors: Record<string, string> = {
  Acknowledged: "bg-gray-100 text-gray-700",
  InProgress: "bg-blue-100 text-blue-700",
  Pending: "bg-yellow-100 text-yellow-700",
  Held: "bg-orange-100 text-orange-700",
  Assessing: "bg-purple-100 text-purple-700",
  Rejected: "bg-red-100 text-red-700",
  Cancelled: "bg-gray-200 text-gray-500",
  Completed: "bg-green-100 text-green-700",
  Failed: "bg-red-100 text-red-700",
}

function getAvailableActions(state: string): { action: string; label: string; needsInput?: boolean; inputLabel?: string }[] {
  switch (state) {
    case "Acknowledged":
      return [
        { action: "start", label: "Start" },
        { action: "cancel", label: "Cancel" },
      ]
    case "InProgress":
      return [
        { action: "hold", label: "Hold" },
        { action: "assess", label: "Assess" },
        { action: "pending", label: "Set Pending", needsInput: true, inputLabel: "Reason" },
        { action: "fail", label: "Fail", needsInput: true, inputLabel: "Error" },
        { action: "cancel", label: "Cancel" },
      ]
    case "Held":
      return [
        { action: "resume", label: "Resume" },
        { action: "cancel", label: "Cancel" },
      ]
    case "Assessing":
      return [
        { action: "reject", label: "Reject", needsInput: true, inputLabel: "Reason" },
        { action: "complete", label: "Complete" },
      ]
    case "Pending":
      return [
        { action: "start", label: "Start" },
        { action: "cancel", label: "Cancel" },
      ]
    default:
      return []
  }
}

export function ItemStateTimeline({ items, onTransition }: ItemStateTimelineProps) {
  if (!items || items.length === 0) {
    return <p className="text-sm text-gray-400">No items</p>
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-medium">Item States</h3>
      {items.map((item) => (
        <div key={item.id} className="rounded border p-3">
          <div className="flex items-center justify-between mb-2">
            <div>
              <span className="font-medium text-sm">{item.productName}</span>
              <span className="text-xs text-gray-400 ml-2">x{item.quantity}</span>
            </div>
            <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${stateColors[item.state] || "bg-gray-100 text-gray-600"}`}>
              {item.state}
            </span>
          </div>
          <div className="flex gap-1.5 flex-wrap">
            {getAvailableActions(item.state).map((action) => (
              <button
                key={action.action}
                onClick={() => {
                  if (action.needsInput) {
                    const value = prompt(action.inputLabel || "Enter value:")
                    if (value) onTransition(item.id, action.action, { [action.action === "fail" ? "error" : "reason"]: value })
                  } else {
                    onTransition(item.id, action.action)
                  }
                }}
                className="rounded border px-2 py-0.5 text-xs text-gray-600 hover:bg-gray-50"
              >
                {action.label}
              </button>
            ))}
          </div>
        </div>
      ))}
    </div>
  )
}
