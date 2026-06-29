"use client"

import { AlertCircle } from "lucide-react"
import { Button } from "@/components/ui/button"

interface ErrorFallbackProps {
  message?: string
  onRetry?: () => void
}

export function ErrorFallback({ message = "An error occurred", onRetry }: ErrorFallbackProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <AlertCircle className="h-12 w-12 text-destructive/50" />
      <h3 className="mt-4 text-lg font-semibold">Error</h3>
      <p className="mt-1 text-sm text-muted-foreground">{message}</p>
      {onRetry && (
        <Button variant="outline" className="mt-4" onClick={onRetry}>
          Retry
        </Button>
      )}
    </div>
  )
}
