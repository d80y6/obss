"use client"

import { useQuery } from "@tanstack/react-query"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { Wifi, DollarSign, Ticket, Activity } from "lucide-react"

async function fetchDashboard() {
  const res = await fetch("/api/v1/customers/me/dashboard")
  if (!res.ok) throw new Error("Failed to load dashboard")
  return res.json()
}

export default function PortalDashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["portal-dashboard"],
    queryFn: fetchDashboard,
  })

  const stats = [
    { title: "Active Services", value: data?.activeServices ?? 0, icon: Wifi, color: "text-blue-600" },
    { title: "Current Bill", value: data?.currentBill ? `$${data.currentBill}` : "$0.00", icon: DollarSign, color: "text-green-600" },
    { title: "Open Tickets", value: data?.openTickets ?? 0, icon: Ticket, color: "text-orange-600" },
    { title: "Data Usage", value: data?.usage ?? "0 GB", icon: Activity, color: "text-purple-600" },
  ]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Welcome back{data?.firstName ? `, ${data.firstName}` : ""}</h1>
        <p className="text-muted-foreground">Here&apos;s an overview of your account</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <Card key={stat.title}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium">{stat.title}</CardTitle>
              <stat.icon className={`h-4 w-4 ${stat.color}`} />
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <Skeleton className="h-8 w-20" />
              ) : (
                <div className="text-2xl font-bold">{stat.value}</div>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader><CardTitle className="text-lg">Recent Bills</CardTitle></CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="space-y-2"><Skeleton className="h-12 w-full" /><Skeleton className="h-12 w-full" /></div>
            ) : (
              <p className="text-sm text-muted-foreground">No recent bills</p>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardHeader><CardTitle className="text-lg">Recent Support Tickets</CardTitle></CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="space-y-2"><Skeleton className="h-12 w-full" /><Skeleton className="h-12 w-full" /></div>
            ) : (
              <p className="text-sm text-muted-foreground">No open tickets</p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
