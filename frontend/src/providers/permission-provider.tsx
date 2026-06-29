"use client"

import { createContext, useContext } from "react"
import { useAuthStore } from "@/stores/auth-store"

interface PermissionContextValue {
  hasRole: (role: string) => boolean
  hasAnyRole: (roles: string[]) => boolean
  isAuthenticated: boolean
}

const PermissionContext = createContext<PermissionContextValue>({
  hasRole: () => false,
  hasAnyRole: () => false,
  isAuthenticated: false,
})

export function PermissionProvider({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated } = useAuthStore()

  const hasRole = (role: string) => {
    return user?.role === role || user?.role === "ADMIN"
  }

  const hasAnyRole = (roles: string[]) => {
    return roles.some((r) => hasRole(r))
  }

  return (
    <PermissionContext.Provider value={{ hasRole, hasAnyRole, isAuthenticated }}>
      {children}
    </PermissionContext.Provider>
  )
}

export const usePermission = () => useContext(PermissionContext)
