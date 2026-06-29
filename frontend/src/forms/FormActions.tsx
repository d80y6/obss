"use client"

import { Button } from "@/components/ui/button"
import { Loader2 } from "lucide-react"
import Link from "next/link"

interface FormActionsProps {
  backHref: string
  loading?: boolean
  loadingText?: string
  submitLabel?: string
}

export function FormActions({ backHref, loading, loadingText = "Saving...", submitLabel = "Save" }: FormActionsProps) {
  return (
    <div className="flex justify-end gap-3 pt-4">
      <Button variant="outline" type="button" asChild>
        <Link href={backHref}>Cancel</Link>
      </Button>
      <Button type="submit" disabled={loading}>
        {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
        {loading ? loadingText : submitLabel}
      </Button>
    </div>
  )
}
