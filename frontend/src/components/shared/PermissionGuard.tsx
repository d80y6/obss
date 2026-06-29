"use client"

import { usePermission } from "@/providers/permission-provider"

interface PermissionGuardProps {
  role?: string
  roles?: string[]
  children: React.ReactNode
  fallback?: React.ReactNode
}

export function PermissionGuard({ role, roles, children, fallback }: PermissionGuardProps) {
  const { hasRole, hasAnyRole } = usePermission()

  if (role && !hasRole(role)) {
    return fallback ?? null
  }

  if (roles && !hasAnyRole(roles)) {
    return fallback ?? null
  }

  return <>{children}</>
}
