"use client"

import { usePathname } from "next/navigation"
import Link from "next/link"
import { ChevronRight, Home } from "lucide-react"

const labelMap: Record<string, string> = {
  dashboard: "Dashboard",
  customers: "Customers",
  products: "Products",
  orders: "Orders",
  subscriptions: "Subscriptions",
  invoices: "Invoices",
  billing: "Billing",
  payments: "Payments",
  tickets: "Tickets",
  admin: "Admin",
  collections: "Collections",
  services: "Services",
  network: "Network",
  provisioning: "Provisioning",
  workflow: "Workflow",
  notifications: "Notifications",
  reporting: "Reporting",
  audit: "Audit",
  new: "New",
  edit: "Edit",
  offers: "Offers",
  categories: "Categories",
  cycles: "Cycles",
  jobs: "Jobs",
  "credit-notes": "Credit Notes",
  disputes: "Disputes",
  reconciliation: "Reconciliation",
  refunds: "Refunds",
}

export function BreadcrumbBuilder() {
  const pathname = usePathname()
  const segments = pathname.split("/").filter(Boolean)

  return (
    <nav className="flex items-center gap-1 text-sm text-muted-foreground">
      <Link href="/dashboard" className="hover:text-foreground">
        <Home className="h-4 w-4" />
      </Link>
      {segments.map((segment, index) => {
        const href = "/" + segments.slice(0, index + 1).join("/")
        const label = labelMap[segment] || segment
        const isLast = index === segments.length - 1
        return (
          <span key={segment} className="flex items-center gap-1">
            <ChevronRight className="h-3 w-3" />
            {isLast ? (
              <span className="font-medium text-foreground">{label}</span>
            ) : (
              <Link href={href} className="hover:text-foreground">
                {label}
              </Link>
            )}
          </span>
        )
      })}
    </nav>
  )
}
