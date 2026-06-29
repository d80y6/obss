"use client"

import { Button } from "@/components/ui/button"

interface PaginationProps {
  page: number
  pageSize: number
  total: number
  onPageChange: (page: number) => void
  onPageSizeChange?: (pageSize: number) => void
}

export function Pagination({ page, pageSize, total, onPageChange, onPageSizeChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize))
  const hasNext = page < totalPages
  const hasPrev = page > 1

  return (
    <div className="flex items-center justify-between pt-4">
      <div className="flex items-center gap-2">
        <p className="text-sm text-muted-foreground">
          Page {page} of {totalPages} ({total} total)
        </p>
        {onPageSizeChange && (
          <select
            className="text-sm border rounded px-2 py-1 bg-background"
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
          >
            {[10, 20, 50, 100].map((size) => (
              <option key={size} value={size}>{size} / page</option>
            ))}
          </select>
        )}
      </div>
      <div className="flex gap-2">
        <Button variant="outline" size="sm" disabled={!hasPrev} onClick={() => onPageChange(page - 1)}>
          Previous
        </Button>
        <Button variant="outline" size="sm" disabled={!hasNext} onClick={() => onPageChange(page + 1)}>
          Next
        </Button>
      </div>
    </div>
  )
}
