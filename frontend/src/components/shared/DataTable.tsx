"use client"

import { useState, useCallback } from "react"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Checkbox } from "@/components/ui/checkbox"
import { Button } from "@/components/ui/button"
import { ArrowUpDown, ArrowUp, ArrowDown, Columns } from "lucide-react"
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { LoadingState } from "./LoadingState"
import { EmptyState } from "./EmptyState"
import { ErrorFallback } from "./ErrorFallback"
import { Pagination } from "./Pagination"
import { BulkActions, BulkAction } from "./BulkActions"
import { cn } from "@/lib/utils"

export interface Column<T> {
  id: string
  header: string
  accessorKey?: keyof T & string
  cell?: (row: T) => React.ReactNode
  sortable?: boolean
  width?: string
  hidden?: boolean
}

interface DataTableProps<T> {
  columns: Column<T>[]
  data: T[]
  loading?: boolean
  error?: string
  emptyTitle?: string
  emptyDescription?: string
  emptyIcon?: React.ElementType
  rowKey: (row: T) => string
  selectedIds?: string[]
  onSelectionChange?: (ids: string[]) => void
  onRowClick?: (row: T) => void
  sortBy?: string
  sortOrder?: "asc" | "desc"
  onSortChange?: (column: string, order: "asc" | "desc") => void
  pagination?: {
    page: number
    pageSize: number
    total: number
    onPageChange: (page: number) => void
    onPageSizeChange?: (size: number) => void
  }
  bulkActions?: BulkAction[]
  onExportCsv?: () => void
  onExportExcel?: () => void
  columnVisibility?: Record<string, boolean>
  onColumnVisibilityChange?: (columns: Record<string, boolean>) => void
}

export function DataTable<T extends object>({
  columns,
  data,
  loading,
  error,
  emptyTitle = "No data found",
  emptyDescription,
  emptyIcon,
  rowKey,
  selectedIds = [],
  onSelectionChange,
  onRowClick,
  sortBy,
  sortOrder = "asc",
  onSortChange,
  pagination,
  bulkActions,
  onExportCsv,
  onExportExcel,
  columnVisibility,
  onColumnVisibilityChange,
}: DataTableProps<T>) {
  const [localColumnVisibility, setLocalColumnVisibility] = useState<Record<string, boolean>>(
    columnVisibility || columns.reduce((acc, col) => ({ ...acc, [col.id]: !col.hidden }), {})
  )

  const visibility = columnVisibility || localColumnVisibility
  const visibleColumns = columns.filter((col) => visibility[col.id] !== false)

  const handleSort = useCallback(
    (columnId: string) => {
      if (!onSortChange) return
      const isAsc = sortBy === columnId && sortOrder === "asc"
      onSortChange(columnId, isAsc ? "desc" : "asc")
    },
    [sortBy, sortOrder, onSortChange]
  )

  const handleSelectAll = useCallback(() => {
    if (selectedIds.length === data.length) {
      onSelectionChange?.([])
    } else {
      onSelectionChange?.(data.map((row) => rowKey(row)))
    }
  }, [data, selectedIds, onSelectionChange, rowKey])

  const handleSelectRow = useCallback(
    (id: string) => {
      const newSelected = selectedIds.includes(id)
        ? selectedIds.filter((sid) => sid !== id)
        : [...selectedIds, id]
      onSelectionChange?.(newSelected)
    },
    [selectedIds, onSelectionChange]
  )

  const toggleColumnVisibility = (columnId: string) => {
    const updated = {
      ...visibility,
      [columnId]: !visibility[columnId],
    }
    if (onColumnVisibilityChange) {
      onColumnVisibilityChange(updated)
    } else {
      setLocalColumnVisibility(updated)
    }
  }

  if (error) {
    return <ErrorFallback message={error} />
  }

  const colCount = visibleColumns.length + (onSelectionChange ? 1 : 0) + ((onColumnVisibilityChange || onExportCsv || onExportExcel) ? 1 : 0)

  return (
    <div className="space-y-4">
      {bulkActions && (
        <BulkActions selectedIds={selectedIds} actions={bulkActions} />
      )}

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              {onSelectionChange && (
                <TableHead className="w-10">
                  <Checkbox
                    checked={data.length > 0 && selectedIds.length === data.length}
                    onCheckedChange={handleSelectAll}
                    aria-label="Select all"
                  />
                </TableHead>
              )}
              {visibleColumns.map((column) => (
                <TableHead
                  key={column.id}
                  style={{ width: column.width }}
                  className={cn(column.sortable && "cursor-pointer select-none")}
                >
                  <div
                    className="flex items-center gap-1"
                    onClick={() => column.sortable && handleSort(column.id)}
                  >
                    {column.header}
                    {column.sortable && (
                      <span className="ml-1">
                        {sortBy === column.id ? (
                          sortOrder === "asc" ? (
                            <ArrowUp className="h-3 w-3" />
                          ) : (
                            <ArrowDown className="h-3 w-3" />
                          )
                        ) : (
                          <ArrowUpDown className="h-3 w-3 opacity-30" />
                        )}
                      </span>
                    )}
                  </div>
                </TableHead>
              ))}
              {(onColumnVisibilityChange || onExportCsv || onExportExcel) && (
                <TableHead className="w-14 text-right">
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="icon" className="h-8 w-8">
                        <Columns className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      {columns.map((col) => (
                        <DropdownMenuCheckboxItem
                          key={col.id}
                          checked={visibility[col.id] !== false}
                          onCheckedChange={() => toggleColumnVisibility(col.id)}
                        >
                          {col.header}
                        </DropdownMenuCheckboxItem>
                      ))}
                      {(onExportCsv || onExportExcel) && (
                        <>
                          <DropdownMenuSeparator />
                          {onExportCsv && (
                            <DropdownMenuCheckboxItem onSelect={onExportCsv}>
                              Export CSV
                            </DropdownMenuCheckboxItem>
                          )}
                          {onExportExcel && (
                            <DropdownMenuCheckboxItem onSelect={onExportExcel}>
                              Export Excel
                            </DropdownMenuCheckboxItem>
                          )}
                        </>
                      )}
                    </DropdownMenuContent>
                  </DropdownMenu>
                </TableHead>
              )}
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={colCount}>
                  <LoadingState rows={5} />
                </TableCell>
              </TableRow>
            ) : data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={colCount}>
                  <EmptyState title={emptyTitle} description={emptyDescription} icon={emptyIcon} />
                </TableCell>
              </TableRow>
            ) : (
              data.map((row) => {
                const key = rowKey(row)
                const isSelected = selectedIds.includes(key)
                return (
                  <TableRow
                    key={key}
                    className={cn(
                      "transition-colors",
                      onRowClick && "cursor-pointer",
                      isSelected && "bg-muted/50"
                    )}
                    onClick={() => onRowClick?.(row)}
                    data-state={isSelected ? "selected" : undefined}
                  >
                    {onSelectionChange && (
                      <TableCell className="w-10" onClick={(e) => e.stopPropagation()}>
                        <Checkbox
                          checked={isSelected}
                          onCheckedChange={() => handleSelectRow(key)}
                          aria-label="Select row"
                        />
                      </TableCell>
                    )}
                    {visibleColumns.map((column) => (
                      <TableCell key={column.id}>
                        {column.cell
                          ? column.cell(row)
                          : column.accessorKey
                          ? String((row as Record<string, unknown>)[column.accessorKey] ?? "")
                          : ""}
                      </TableCell>
                    ))}
                    {(onColumnVisibilityChange || onExportCsv || onExportExcel) && (
                      <TableCell />
                    )}
                  </TableRow>
                )
              })
            )}
          </TableBody>
        </Table>
      </div>

      {pagination && !loading && (
        <Pagination {...pagination} />
      )}
    </div>
  )
}
