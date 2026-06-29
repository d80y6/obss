"use client"

import { Button } from "@/components/ui/button"

export interface BulkAction {
  label: string
  onClick: (ids: string[]) => void
  variant?: "default" | "destructive" | "outline" | "secondary"
}

interface BulkActionsProps {
  selectedIds: string[]
  actions: BulkAction[]
}

export function BulkActions({ selectedIds, actions }: BulkActionsProps) {
  if (selectedIds.length === 0) return null

  return (
    <div className="flex items-center gap-2 px-2 py-2 bg-muted/50 rounded-md">
      <span className="text-sm text-muted-foreground">
        {selectedIds.length} selected
      </span>
      {actions.map((action) => (
        <Button
          key={action.label}
          variant={action.variant || "outline"}
          size="sm"
          onClick={() => action.onClick(selectedIds)}
        >
          {action.label}
        </Button>
      ))}
    </div>
  )
}
