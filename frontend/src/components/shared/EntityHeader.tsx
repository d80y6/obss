"use client"

import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { ArrowLeft, Edit, Trash2 } from "lucide-react"
import Link from "next/link"
import { Skeleton } from "@/components/ui/skeleton"

interface EntityHeaderProps {
  title: string
  subtitle?: string
  status?: string
  backHref: string
  editHref?: string
  onDelete?: () => void
  loading?: boolean
}

export function EntityHeader({ title, subtitle, status, backHref, editHref, onDelete, loading }: EntityHeaderProps) {
  if (loading) {
    return (
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Skeleton className="h-9 w-9 rounded-md" />
          <div>
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-32 mt-1" />
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="flex items-center justify-between">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href={backHref}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
            {status && <Badge>{status}</Badge>}
          </div>
          {subtitle && (
            <p className="text-sm text-muted-foreground">{subtitle}</p>
          )}
        </div>
      </div>
      <div className="flex items-center gap-2">
        {editHref && (
          <Button variant="outline" size="sm" asChild>
            <Link href={editHref}>
              <Edit className="mr-1 h-4 w-4" /> Edit
            </Link>
          </Button>
        )}
        {onDelete && (
          <Button variant="destructive" size="sm" onClick={onDelete}>
            <Trash2 className="mr-1 h-4 w-4" /> Delete
          </Button>
        )}
      </div>
    </div>
  )
}
