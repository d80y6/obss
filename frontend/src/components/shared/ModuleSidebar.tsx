"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { cn } from "@/lib/utils"
import { useAuthStore } from "@/stores/auth-store"
import {
  LayoutDashboard,
  Users,
  Package,
  Book,
  ShoppingCart,
  ClipboardList,
  FileText,
  CreditCard,
  DollarSign,
  Scale,
  Ticket,
  Shield,
  Network,
  Cable,
  Settings,
  Bell,
  FileBarChart,
  ScrollText,
  Waypoints,
  LogOut,
  ChevronLeft,
  ChevronDown,
  BarChart3,
  FileSpreadsheet,
  CircuitBoard,
  FileSearch,
  Wallet,
  Radio,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { useState } from "react"
import { ThemeToggle } from "./ThemeToggle"
import { LocaleToggle } from "./LocaleToggle"

interface NavItem {
  href: string
  label: string
  icon: React.ElementType
}

const modules: NavItem[] = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/customers", label: "Customers", icon: Users },
  { href: "/catalogs", label: "Catalogs", icon: Book },
  { href: "/products", label: "Products", icon: Package },
  { href: "/product-specifications", label: "Product Specs", icon: FileSpreadsheet },
  { href: "/orders", label: "Orders", icon: ShoppingCart },
  { href: "/subscriptions", label: "Subscriptions", icon: ClipboardList },
  { href: "/billing", label: "Billing", icon: CreditCard },
  { href: "/invoices", label: "Invoices", icon: FileText },
  { href: "/payments", label: "Payments", icon: DollarSign },
  { href: "/payments/summary", label: "Payment Summary", icon: FileText },
  { href: "/payments/gateways", label: "Gateways", icon: CreditCard },
  { href: "/rating", label: "Rating", icon: Scale },
  { href: "/ocs", label: "OCS", icon: Wallet },
  { href: "/event-management", label: "Events", icon: Radio },
  { href: "/tickets", label: "Tickets", icon: Ticket },
  { href: "/network", label: "Network", icon: Cable },
  { href: "/provisioning", label: "Provisioning", icon: Settings },
  { href: "/workflow", label: "Workflow", icon: Waypoints },
  { href: "/notifications", label: "Notifications", icon: Bell },
  { href: "/reporting", label: "Reporting", icon: FileBarChart },
  { href: "/audit", label: "Audit", icon: ScrollText },
  { href: "/admin", label: "Admin", icon: Shield },
]

export function ModuleSidebar() {
  const pathname = usePathname()
  const { user, logout } = useAuthStore()
  const [collapsed, setCollapsed] = useState(false)
  const [collectionsOpen, setCollectionsOpen] = useState(false)
  const [serviceInventoryOpen, setServiceInventoryOpen] = useState(false)

  return (
    <aside
      className={cn(
        "flex flex-col border-r bg-background transition-all duration-300",
        collapsed ? "w-16" : "w-64"
      )}
    >
      <div className="flex h-14 items-center border-b px-4">
        {!collapsed && (
          <span className="text-lg font-bold tracking-tight">OSS/BSS</span>
        )}
        <Button
          variant="ghost"
          size="icon"
          onClick={() => setCollapsed(!collapsed)}
          className={cn("ml-auto", collapsed && "mx-auto")}
        >
          <ChevronLeft className={cn("h-4 w-4 transition-transform", collapsed && "rotate-180")} />
        </Button>
      </div>

      <nav className="flex-1 overflow-y-auto p-2 space-y-1">
        {modules.map((item) => {
          const Icon = item.icon
          const isActive = pathname.startsWith(item.href)
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                isActive ? "bg-accent text-accent-foreground" : "text-muted-foreground",
                collapsed && "justify-center px-2"
              )}
              title={collapsed ? item.label : undefined}
            >
              <Icon className="h-4 w-4 shrink-0" />
              {!collapsed && <span>{item.label}</span>}
            </Link>
          )
        })}

        {!collapsed && (
          <div>
            <button
              onClick={() => setServiceInventoryOpen(!serviceInventoryOpen)}
              className={cn(
                "flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                pathname.startsWith("/service-inventory") ? "bg-accent text-accent-foreground" : "text-muted-foreground"
              )}
            >
              <Network className="h-4 w-4 shrink-0" />
              <span className="flex-1 text-left">Service Inventory</span>
              <ChevronDown
                className={cn(
                  "h-4 w-4 transition-transform",
                  serviceInventoryOpen && "rotate-180"
                )}
              />
            </button>
            {serviceInventoryOpen && (
              <div className="ml-2 space-y-1 border-l pl-2">
                <Link
                  href="/service-inventory"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname === "/service-inventory" ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <Network className="h-4 w-4 shrink-0" />
                  <span>Services</span>
                </Link>
                <Link
                  href="/service-inventory/discovery"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname === "/service-inventory/discovery" ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <CircuitBoard className="h-4 w-4 shrink-0" />
                  <span>Discovery</span>
                </Link>
                <Link
                  href="/service-inventory/discovery/unmatched"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname === "/service-inventory/discovery/unmatched" ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <FileSearch className="h-4 w-4 shrink-0" />
                  <span>Unmatched Resources</span>
                </Link>
              </div>
            )}
          </div>
        )}
        {!collapsed && (
          <div>
            <button
              onClick={() => setCollectionsOpen(!collectionsOpen)}
              className={cn(
                "flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                pathname.startsWith("/collections") ? "bg-accent text-accent-foreground" : "text-muted-foreground"
              )}
            >
              <Settings className="h-4 w-4 shrink-0" />
              <span className="flex-1 text-left">Collections</span>
              <ChevronDown
                className={cn(
                  "h-4 w-4 transition-transform",
                  collectionsOpen && "rotate-180"
                )}
              />
            </button>
            {collectionsOpen && (
              <div className="ml-2 space-y-1 border-l pl-2">
                <Link
                  href="/collections"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname === "/collections" ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <Settings className="h-4 w-4 shrink-0" />
                  <span>Collections</span>
                </Link>
                <Link
                  href="/collections/dashboard"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname === "/collections/dashboard" ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <LayoutDashboard className="h-4 w-4 shrink-0" />
                  <span>Dashboard</span>
                </Link>
                <Link
                  href="/collections/dunning-policies"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname === "/collections/dunning-policies" ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <FileText className="h-4 w-4 shrink-0" />
                  <span>Dunning Policies</span>
                </Link>
                <Link
                  href="/collections/reports/aging"
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground",
                    pathname === "/collections/reports/aging" ? "bg-accent text-accent-foreground" : "text-muted-foreground"
                  )}
                >
                  <BarChart3 className="h-4 w-4 shrink-0" />
                  <span>Aging Report</span>
                </Link>
              </div>
            )}
          </div>
        )}
      </nav>

      <div className="border-t p-4 space-y-2">
        {!collapsed && (
          <div className="flex items-center justify-between px-1">
            <ThemeToggle />
            <LocaleToggle />
          </div>
        )}
        {collapsed && (
          <div className="flex flex-col items-center gap-2">
            <ThemeToggle />
            <LocaleToggle />
          </div>
        )}
        {!collapsed && user && (
          <div className="text-sm">
            <p className="font-medium truncate">{user.firstName} {user.lastName}</p>
            <p className="text-muted-foreground truncate">{user.email}</p>
          </div>
        )}
        <Button
          variant="ghost"
          size={collapsed ? "icon" : "default"}
          className={cn("w-full", collapsed ? "mx-auto" : "")}
          onClick={() => { localStorage.removeItem("auth-token"); logout() }}
        >
          <LogOut className="h-4 w-4" />
          {!collapsed && <span>Logout</span>}
        </Button>
      </div>
    </aside>
  )
}
