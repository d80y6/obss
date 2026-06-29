"use client"

import { AlertCircle } from "lucide-react"

interface FormErrorSummaryProps {
  errors: Record<string, { message?: string } | undefined>
}

export function FormErrorSummary({ errors }: FormErrorSummaryProps) {
  const errorMessages = Object.values(errors).filter(Boolean) as { message?: string }[]

  if (errorMessages.length === 0) return null

  return (
    <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
      <div className="flex items-center gap-2 font-medium">
        <AlertCircle className="h-4 w-4" />
        <span>Please fix the following errors:</span>
      </div>
      <ul className="mt-1 list-disc list-inside space-y-1">
        {errorMessages.map((err, i) => (
          <li key={i}>{err.message}</li>
        ))}
      </ul>
    </div>
  )
}
