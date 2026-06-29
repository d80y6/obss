"use client"

import { Badge } from "@/components/ui/badge"

const statusVariantMap: Record<string, "default" | "secondary" | "destructive" | "outline" | "success" | "warning" | "info"> = {
  ACTIVE: "success",
  COMPLETED: "success",
  PAID: "success",
  RESOLVED: "success",
  ENABLED: "success",
  PENDING: "warning",
  SENT: "warning",
  IN_PROGRESS: "warning",
  SUSPENDED: "warning",
  DRAFT: "secondary",
  INACTIVE: "secondary",
  EXPIRED: "secondary",
  REFUNDED: "secondary",
  CLOSED: "secondary",
  CANCELLED: "destructive",
  FAILED: "destructive",
  OVERDUE: "destructive",
  OPEN: "destructive",
  DISABLED: "destructive",
}

export function StatusBadge({ status }: { status: string }) {
  const variant = statusVariantMap[status] || "default"
  return <Badge variant={variant}>{status}</Badge>
}
