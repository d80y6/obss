"use client"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"

interface MetadataField {
  label: string
  value: string | React.ReactNode
}

interface EntityMetadataProps {
  title?: string
  fields: MetadataField[]
  loading?: boolean
  columns?: 1 | 2 | 3
}

export function EntityMetadata({ title = "Details", fields, loading, columns = 2 }: EntityMetadataProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className={`grid gap-4 ${columns === 1 ? "grid-cols-1" : columns === 2 ? "grid-cols-1 md:grid-cols-2" : "grid-cols-1 md:grid-cols-3"}`}>
          {fields.map((field) => (
            <div key={field.label} className="space-y-1">
              <p className="text-sm text-muted-foreground">{field.label}</p>
              {loading ? (
                <Skeleton className="h-5 w-32" />
              ) : (
                <p className="text-sm font-medium">{field.value}</p>
              )}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}
