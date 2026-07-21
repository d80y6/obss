"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useAuthStore } from "@/stores/auth-store"
import { Button } from "@/components/ui/button"
import { Phone, Menu, X } from "lucide-react"
import { useState } from "react"

const navItems = [
  { href: "/portal/dashboard", label: "Dashboard" },
  { href: "/portal/services", label: "My Services" },
  { href: "/portal/bills", label: "Bills & Payments" },
  { href: "/portal/tickets", label: "Support Tickets" },
  { href: "/portal/profile", label: "Profile" },
]

export function SelfCareShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname()
  const { isAuthenticated, logout } = useAuthStore()
  const [mobileOpen, setMobileOpen] = useState(false)

  const isLoginOrRegister = pathname === "/portal/login" || pathname === "/portal/register"

  if (isLoginOrRegister || !pathname.startsWith("/portal")) {
    return <>{children}</>
  }

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <Link href="/portal/dashboard" className="flex items-center gap-2 font-semibold">
            <Phone className="h-5 w-5 text-primary" />
            <span>Customer Portal</span>
          </Link>

          <nav className="hidden md:flex items-center gap-6 text-sm">
            {navItems.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={`transition-colors hover:text-foreground/80 ${
                  pathname.startsWith(item.href) ? "text-foreground font-medium" : "text-foreground/60"
                }`}
              >
                {item.label}
              </Link>
            ))}
          </nav>

          <div className="flex items-center gap-2">
            <Button variant="ghost" size="sm" onClick={() => logout()} className="hidden md:inline-flex">
              Sign Out
            </Button>
            <Button variant="ghost" size="icon" className="md:hidden" onClick={() => setMobileOpen(!mobileOpen)}>
              {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </Button>
          </div>
        </div>

        {mobileOpen && (
          <div className="md:hidden border-t p-4">
            <nav className="flex flex-col gap-3">
              {navItems.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  onClick={() => setMobileOpen(false)}
                  className={`text-sm transition-colors ${
                    pathname.startsWith(item.href) ? "text-foreground font-medium" : "text-foreground/60"
                  }`}
                >
                  {item.label}
                </Link>
              ))}
              <Button variant="ghost" size="sm" className="justify-start px-0" onClick={() => logout()}>
                Sign Out
              </Button>
            </nav>
          </div>
        )}
      </header>

      <main className="container mx-auto px-4 py-6">{children}</main>

      <footer className="border-t py-6 text-center text-xs text-muted-foreground">
        &copy; {new Date().getFullYear()} Telecom OSS/BSS Platform. All rights reserved.
      </footer>
    </div>
  )
}
