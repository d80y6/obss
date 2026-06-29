"use client"

import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { useState } from "react"
import { ThemeProvider } from "@/providers/theme-provider"
import { LocaleProvider } from "@/providers/locale-provider"
import { PermissionProvider } from "@/providers/permission-provider"
import { TenantProvider } from "@/providers/tenant-provider"

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000,
            retry: 1,
          },
        },
      })
  )

  return (
    <QueryClientProvider client={queryClient}>
      <TenantProvider>
        <ThemeProvider>
          <LocaleProvider>
            <PermissionProvider>
              {children}
            </PermissionProvider>
          </LocaleProvider>
        </ThemeProvider>
      </TenantProvider>
    </QueryClientProvider>
  )
}
