"use client"

import { Button } from "@/components/ui/button"
import { Plus, ArrowLeft } from "lucide-react"
import Link from "next/link"

interface PageHeaderProps {
  title: string
  description?: string
  backHref?: string
  createHref?: string
  createLabel?: string
  actions?: React.ReactNode
}

export function PageHeader({ title, description, backHref, createHref, createLabel, actions }: PageHeaderProps) {
  return (
    <div className="flex items-center justify-between">
      <div className="flex items-center gap-4">
        {backHref && (
          <Button variant="ghost" size="icon" asChild>
            <Link href={backHref}>
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
        )}
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
          {description && (
            <p className="text-sm text-muted-foreground">{description}</p>
          )}
        </div>
      </div>
      <div className="flex items-center gap-2">
        {actions}
        {createHref && (
          <Button asChild>
            <Link href={createHref}>
              <Plus className="mr-1 h-4 w-4" /> {createLabel || "Create"}
            </Link>
          </Button>
        )}
      </div>
    </div>
  )
}
