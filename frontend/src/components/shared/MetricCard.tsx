"use client"

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { LucideIcon } from "lucide-react"
import Link from "next/link"

interface MetricCardProps {
  title: string
  value: number | string
  icon?: LucideIcon
  href?: string
  loading?: boolean
  trend?: { value: number; positive: boolean }
}

export function MetricCard({ title, value, icon: Icon, href, loading, trend }: MetricCardProps) {
  const content = (
    <Card className="transition-colors hover:bg-muted/50">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        {Icon && <Icon className="h-4 w-4 text-muted-foreground" />}
      </CardHeader>
      <CardContent>
        {loading ? (
          <Skeleton className="h-8 w-20" />
        ) : (
          <>
            <p className="text-3xl font-bold">{value}</p>
            {trend && (
              <p className={`text-xs mt-1 ${trend.positive ? "text-emerald-600" : "text-destructive"}`}>
                {trend.positive ? "+" : ""}{trend.value}%
              </p>
            )}
          </>
        )}
      </CardContent>
    </Card>
  )

  if (href) {
    return <Link href={href}>{content}</Link>
  }
  return content
}
