"use client"

import { PageHeader } from "@/components/shared/PageHeader"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Wallet, PiggyBank, ArrowRightLeft, Zap } from "lucide-react"
import Link from "next/link"

const modules = [
  {
    title: "Balances",
    description: "View and adjust subscriber balances in real-time",
    href: "/ocs/balances",
    icon: Wallet,
    color: "text-blue-600",
    bgColor: "bg-blue-50",
  },
  {
    title: "Credit Pools",
    description: "Manage shared credit pools and bundles",
    href: "/ocs/credit-pools",
    icon: PiggyBank,
    color: "text-green-600",
    bgColor: "bg-green-50",
  },
  {
    title: "Transactions",
    description: "Audit real-time charging transactions",
    href: "/ocs/transactions",
    icon: ArrowRightLeft,
    color: "text-purple-600",
    bgColor: "bg-purple-50",
  },
  {
    title: "Reservation",
    description: "Reserve credit for in-progress sessions",
    href: "#",
    icon: Zap,
    color: "text-orange-600",
    bgColor: "bg-orange-50",
  },
]

export default function OcsPage() {
  return (
    <div className="flex-1 space-y-6 p-6">
      <PageHeader title="Online Charging System" description="Real-time credit control, balance management, and charging" />
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        {modules.map((mod) => {
          const Icon = mod.icon
          return (
            <Card key={mod.href} className="hover:shadow-md transition-shadow">
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className={`rounded-lg p-2 ${mod.bgColor}`}>
                    <Icon className={`h-5 w-5 ${mod.color}`} />
                  </div>
                  <CardTitle className="text-base">{mod.title}</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-sm text-muted-foreground">{mod.description}</p>
                <Button variant="outline" size="sm" className="w-full" asChild>
                  <Link href={mod.href}>Manage {mod.title}</Link>
                </Button>
              </CardContent>
            </Card>
          )
        })}
      </div>
    </div>
  )
}
