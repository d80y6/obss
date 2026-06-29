"use client"

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { X } from "lucide-react"

interface FilterOption {
  id: string
  label: string
  type: "select" | "date-range" | "text" | "number"
  options?: { label: string; value: string }[]
  value: string
  onChange: (value: string) => void
  placeholder?: string
}

interface FilterBarProps {
  filters: FilterOption[]
  onClear: () => void
}

export function FilterBar({ filters, onClear }: FilterBarProps) {
  const hasFilters = filters.some((f) => f.value)

  return (
    <div className="flex flex-wrap items-center gap-3">
      {filters.map((filter) => {
        if (filter.type === "select") {
          return (
            <Select key={filter.id} value={filter.value} onValueChange={filter.onChange}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder={filter.placeholder || filter.label} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">{filter.placeholder || `All ${filter.label}`}</SelectItem>
                {filter.options?.map((opt) => (
                  <SelectItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )
        }
        if (filter.type === "text") {
          return (
            <div key={filter.id} className="relative">
              <Input
                placeholder={filter.placeholder || `Filter ${filter.label}...`}
                className="w-48 h-9"
                value={filter.value}
                onChange={(e) => filter.onChange(e.target.value)}
              />
            </div>
          )
        }
        if (filter.type === "date-range") {
          return (
            <Input
              key={filter.id}
              type="date"
              className="w-40 h-9"
              value={filter.value}
              onChange={(e) => filter.onChange(e.target.value)}
            />
          )
        }
        return null
      })}
      {hasFilters && (
        <Button variant="ghost" size="sm" onClick={onClear}>
          <X className="h-4 w-4 mr-1" /> Clear
        </Button>
      )}
    </div>
  )
}
